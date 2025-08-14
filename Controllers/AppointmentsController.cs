using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PetClinicSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly PetClinicContext _context;

        public AppointmentsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Appointment> appointments;

            if (User.IsInRole("Admin"))
            {
                // Admin can see all appointments
                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }
            else if (User.IsInRole("Staff"))
            {
                // Staff sees appointments for today and future
                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .Where(a => a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();
            }
            else if (User.IsInRole("Client"))
            {
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
                if (owner == null) return NotFound();

                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .Where(a => a.Patient.OwnerId == owner.OwnerId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();
            }
            else if (User.IsInRole("Veterinarian"))
            {
                // Vet sees only their own appointments
                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .Where(a => a.VetId == userId && a.AppointmentDate >= DateTime.Today)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();
            }
            else
            {
                return Forbid(); // Unauthorized role
            }

            return View(appointments);
        }

        // GET: Appointment Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Additional check for Staff to prevent viewing past appointments
            if (User.IsInRole("Staff") && appointment.AppointmentDate < DateTime.Today)
            {
                return Forbid();
            }

            return View(appointment);
        }

        // GET: Schedule New Appointment
        public async Task<IActionResult> Schedule()
        {
            ViewData["Action"] = "Schedule";

            var model = new AppointmentViewModel
            {
                AvailablePets = await GetActivePets(),
                AvailableVets = await GetAvailableVets(),
                AppointmentDate = DateTime.Now.AddHours(1) // Default to 1 hour from now
            };

            return View(model);
        }

        // POST: Schedule New Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verify vet exists
                var vetExists = await _context.Users.AnyAsync(u => u.UserId == model.VetId && u.Role == "Veterinarian");
                if (!vetExists)
                {
                    ModelState.AddModelError("VetId", "Invalid veterinarian selection");
                }

                // Verify pet is active
                if (!await _context.Patients.AnyAsync(p => p.PatientId == model.PatientId && p.IsActive == true))
                {
                    ModelState.AddModelError("PatientId", "Invalid pet selection");
                }

                // Check for overlapping appointments
                var isOverlapping = await _context.Appointments
                    .AnyAsync(a => a.VetId == model.VetId &&
                                 a.AppointmentDate <= model.AppointmentDate.AddMinutes(model.Duration ?? 30) &&
                                 a.AppointmentDate.AddMinutes(a.Duration ?? 30) >= model.AppointmentDate &&
                                 a.Status != "Cancelled");

                if (isOverlapping)
                {
                    ModelState.AddModelError("", "The selected time slot is not available");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        PatientId = model.PatientId,
                        VetId = model.VetId,
                        AppointmentDate = model.AppointmentDate,
                        Duration = model.Duration,
                        Reason = model.Reason,
                        Notes = model.Notes,
                        Status = "Scheduled",
                        CreatedDate = DateTime.Now
                    };

                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Repopulate dropdowns if model is invalid
            model.AvailablePets = await GetActivePets();
            model.AvailableVets = await GetAvailableVets();
            return View(model);
        }

        // GET: Cancel Appointment
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.Status == "Scheduled");

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Cancel Appointment
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Cancelled";
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Reschedule Appointment
        public async Task<IActionResult> Reschedule(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.Status == "Scheduled");

            if (appointment == null)
            {
                return NotFound();
            }

            var model = new RescheduleViewModel
            {
                AppointmentId = appointment.AppointmentId,
                CurrentAppointmentDate = appointment.AppointmentDate,
                VetId = appointment.VetId,
                AvailableVets = await GetAvailableVets()
            };

            return View(model);
        }

        // POST: Reschedule Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, RescheduleViewModel model)
        {
            if (id != model.AppointmentId)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verify vet exists
                var vet = await _context.Users.FindAsync(model.VetId);
                if (vet == null || vet.Role != "Veterinarian")
                {
                    ModelState.AddModelError("VetId", "Invalid veterinarian selection");
                    return View(model);
                }

                // Check for overlapping appointments
                var overlappingAppointment = await _context.Appointments
                    .AnyAsync(a => a.VetId == model.VetId &&
                                 a.AppointmentId != id &&
                                 a.AppointmentDate <= model.NewAppointmentDate.AddMinutes(30) &&
                                 a.AppointmentDate.AddMinutes(30) >= model.NewAppointmentDate &&
                                 a.Status != "Cancelled");

                if (overlappingAppointment)
                {
                    ModelState.AddModelError("NewAppointmentDate", "The selected time slot is not available");
                    return View(model);
                }

                appointment.VetId = model.VetId;
                appointment.AppointmentDate = model.NewAppointmentDate;
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if model is invalid
            model.AvailableVets = await GetAvailableVets();
            return View(model);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var model = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                VetId = appointment.VetId,
                AppointmentDate = appointment.AppointmentDate,
                Duration = appointment.Duration,
                Reason = appointment.Reason,
                Notes = appointment.Notes,
                Status = appointment.Status,
                AvailablePets = await GetActivePets(),
                AvailableVets = await GetAvailableVets()
            };

            return View(model);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel model)
        {
            if (id != model.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    appointment.VetId = model.VetId;
                    appointment.PatientId = model.PatientId;
                    appointment.AppointmentDate = model.AppointmentDate;
                    appointment.Duration = model.Duration;
                    appointment.Reason = model.Reason;
                    appointment.Notes = model.Notes;
                    appointment.Status = model.Status;

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Appointment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(model.AppointmentId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            model.AvailablePets = await GetActivePets();
            model.AvailableVets = await GetAvailableVets();
            return View(model);
        }

        // GET: Appointments/Calendar
        public async Task<IActionResult> Calendar()
        {
            return View();
        }

        // GET: Appointments/GetCalendarEvents (for FullCalendar)
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            List<CalendarEventViewModel> events;

            if (User.IsInRole("Admin"))
            {
                events = await _context.Appointments
                    .Select(a => new CalendarEventViewModel
                    {
                        Id = a.AppointmentId,
                        Title = $"{a.Patient.Name} - {a.Reason}",
                        Start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        End = a.AppointmentDate.AddMinutes(a.Duration ?? 30).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Color = GetStatusColor(a.Status),
                        VetName = $"{a.Vet.FirstName} {a.Vet.LastName}"
                    })
                    .ToListAsync();
            }
            else // Staff
            {
                events = await _context.Appointments
                    .Where(a => a.AppointmentDate >= DateTime.Today)
                    .Select(a => new CalendarEventViewModel
                    {
                        Id = a.AppointmentId,
                        Title = $"{a.Patient.Name} - {a.Reason}",
                        Start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        End = a.AppointmentDate.AddMinutes(a.Duration ?? 30).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Color = GetStatusColor(a.Status),
                        VetName = $"{a.Vet.FirstName} {a.Vet.LastName}"
                    })
                    .ToListAsync();
            }

            return Json(events);
        }

        private async Task<List<SelectListItem>> GetActivePets()
        {
            return await _context.Patients
                .Where(p => p.IsActive == true)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = $"{p.Name} ({p.Species})"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetAvailableVets()
        {
            return await _context.Users
                .Where(u => u.Role == "Veterinarian")
                .Select(v => new SelectListItem
                {
                    Value = v.UserId.ToString(),
                    Text = $"Dr. {v.FirstName} {v.LastName}"
                })
                .ToListAsync();
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Completed" => "#28a745", // Green
                "Cancelled" => "#dc3545", // Red
                "Confirmed" => "#17a2b8", // Teal
                _ => "#007bff" // Blue (default for scheduled)
            };
        }
    }
}