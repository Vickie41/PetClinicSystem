using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        //private readonly UserManager<IdentityUser> _userManager;


        public AppointmentsController(PetClinicContext context) /*UserManager<IdentityUser> userManager*/
        {
            _context = context;
            //_userManager = userManager;
        }

        // GET: My Appointments
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .Where(a => a.Patient.OwnerId == owner.OwnerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointment Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.Patient.OwnerId == owner.OwnerId);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Schedule New Appointment
        public async Task<IActionResult> Schedule()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            ViewData["Action"] = "Schedule";

            var model = new AppointmentViewModel
            {
                AvailablePets = await GetPetsForOwner(owner.OwnerId),
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            ViewData["Action"] = "Schedule";
            Console.WriteLine($"Received model - VetId: {model.VetId}, PatientId: {model.PatientId}");

            
                var vetExists = await _context.Users.AnyAsync(u => u.UserId == model.VetId && u.Role == "Vet");
                Console.WriteLine($"Vet exists check: {vetExists} for ID {model.VetId}");

                if (!vetExists)
                {
                    ModelState.AddModelError("VetId", "Invalid veterinarian selection");
                    Console.WriteLine("Invalid vet selected");
                }

                // Verify pet belongs to owner
                if (!await _context.Patients.AnyAsync(p => p.PatientId == model.PatientId && p.OwnerId == owner.OwnerId))
                {
                    ModelState.AddModelError("PatientId", "Invalid pet selection");
                }

                // Verify vet exists
                if (!await _context.Users.AnyAsync(u => u.UserId == model.VetId && u.Role == "Vet"))
                {
                    ModelState.AddModelError("VetId", "Invalid veterinarian selection");
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
                            Status = model.Status,
                            CreatedDate = model.CreatedDate
                        };

                        _context.Add(appointment);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                
            

            // Repopulate dropdowns if model is invalid
            model.AvailablePets = await GetPetsForOwner(owner.OwnerId);
            model.AvailableVets = await GetAvailableVets();
            Console.WriteLine($"Available vets count: {model.AvailableVets.Count}");
            Console.WriteLine($"Available pets count: {model.AvailablePets.Count}");
            return View(model);
        }

        private async Task<List<SelectListItem>> GetPetsForOwner(int ownerId)
        {
            return await _context.Patients
                .Where(p => p.OwnerId == ownerId && p.IsActive == true)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = $"{p.Name} ({p.Species})"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetAvailableVets()
        {
            var vets = await _context.Users
                .Where(u => u.Role == "Veterinarian")
                .ToListAsync();

            // Debug output - check your debug console
            Console.WriteLine($"Found {vets.Count} vets:");
            foreach (var vet in vets)
            {
                Console.WriteLine($"- {vet.UserId}: {vet.FirstName} {vet.LastName} (Role: {vet.Role})");
            }

            return vets.Select(v => new SelectListItem
            {
                Value = v.UserId.ToString(),
                Text = $"Dr. {v.FirstName} {v.LastName}" // Include both names for clarity
            }).ToList();
        }

        // GET: Cancel Appointment
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                       a.Patient.OwnerId == owner.OwnerId &&
                                       a.Status == "Scheduled");

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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                        a.Patient.OwnerId == owner.OwnerId &&
                                        a.Status == "Scheduled");

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

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                       a.Patient.OwnerId == owner.OwnerId &&
                                       a.Status == "Scheduled");

            if (appointment == null)
            {
                return NotFound();
            }

            var model = new RescheduleViewModel
            {
                AppointmentId = appointment.AppointmentId,
                CurrentAppointmentDate = appointment.AppointmentDate,
                VetId = appointment.VetId,
                AvailableVets = await _context.Users
                    .Where(u => u.Role == "Vet")
                    .Select(v => new SelectListItem
                    {
                        Value = v.UserId.ToString(),
                        Text = $"Dr. {v.LastName}"
                    })
                    .ToListAsync()
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

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                       a.Patient.OwnerId == owner.OwnerId &&
                                       a.Status == "Scheduled");

            if (appointment == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verify vet exists
                var vet = await _context.Users.FindAsync(model.VetId);
                if (vet == null || vet.Role != "Vet")
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
            model.AvailableVets = await _context.Users
                .Where(u => u.Role == "Vet")
                .Select(v => new SelectListItem
                {
                    Value = v.UserId.ToString(),
                    Text = $"Dr. {v.LastName}"
                })
                .ToListAsync();

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
                AvailablePets = await GetPetsForOwner(appointment.Patient.OwnerId),
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

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner not found");
            }

           
                try
                {
                    // Get existing appointment
                    var appointment = await _context.Appointments
                        .FirstOrDefaultAsync(a => a.AppointmentId == id &&
                                                a.Patient.OwnerId == owner.OwnerId);

                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    // Update properties
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
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!AppointmentExists(model.AppointmentId))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError("", "Concurrency error. The appointment was modified by another user.");
                    Console.WriteLine($"Concurrency error: {ex}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving changes: {ex.Message}");
                    Console.WriteLine($"Error saving appointment: {ex}");
                }
            

            // If we got this far, something failed; repopulate dropdowns
            model.AvailablePets = await GetPetsForOwner(owner.OwnerId);
            model.AvailableVets = await GetAvailableVets();
            ViewData["Action"] = "Edit";

            return View(model);
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        // GET: Appointments/Calendar
        [Authorize]
        public async Task<IActionResult> Calendar()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Appointment> appointments;

            if (User.IsInRole("Client"))
            {
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
                if (owner == null) return NotFound();

                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .Where(a => a.Patient.OwnerId == owner.OwnerId)
                    .ToListAsync();
            }
            else
            {
                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .ToListAsync();
            }

            // Convert to ViewModel
            var viewModel = appointments.Select(a => new AppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient?.Name,
                VetName = $"{a.Vet?.FirstName} {a.Vet?.LastName}",
                AppointmentDate = a.AppointmentDate,
                Duration = a.Duration,
                Reason = a.Reason,
                Status = a.Status
            }).ToList();

            return View(viewModel);
        }

        // GET: Appointments/GetCalendarEvents (for FullCalendar)
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<CalendarEventViewModel> events;

            if (User.IsInRole("Client"))
            {
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
                if (owner == null) return Json(new List<CalendarEventViewModel>());

                events = await _context.Appointments
                    .Where(a => a.Patient.OwnerId == owner.OwnerId)
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
            else
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

            return Json(events);
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
