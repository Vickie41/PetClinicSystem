using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;




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

            var model = new AppointmentViewModel
            {
                AvailablePets = await _context.Patients
                    .Where(p => p.OwnerId == owner.OwnerId)
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.Name} ({p.Species})"
                    })
                    .ToListAsync(),
                AvailableVets = await _context.Users
                    .Where(u => u.Role == "Vet")
                    .Select(v => new SelectListItem
                    {
                        Value = v.UserId.ToString(),
                        Text = $"Dr. {v.LastName}"
                    })
                    .ToListAsync(),
                AppointmentDate = DateTime.Now.AddDays(1)
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

            if (ModelState.IsValid)
            {
                // Verify pet belongs to the owner
                var pet = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == model.PatientId && p.OwnerId == owner.OwnerId);

                if (pet == null)
                {
                    ModelState.AddModelError("PatientId", "Invalid pet selection");
                    return View(model);
                }

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
                                 a.AppointmentDate <= model.AppointmentDate.AddMinutes(30) &&
                                 a.AppointmentDate.AddMinutes(30) >= model.AppointmentDate &&
                                 a.Status != "Cancelled");

                if (overlappingAppointment)
                {
                    ModelState.AddModelError("AppointmentDate", "The selected time slot is not available");
                    return View(model);
                }

                var appointment = new Appointment
                {
                    PatientId = model.PatientId,
                    VetId = model.VetId,
                    AppointmentDate = model.AppointmentDate,
                    Reason = model.Reason,
                    Status = "Scheduled",
                    CreatedDate = DateTime.Now
                };

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if model is invalid
            model.AvailablePets = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = $"{p.Name} ({p.Species})"
                })
                .ToListAsync();
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
    }
}
