//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using PetClinicSystem.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;


//namespace PetClinicSystem.Controllers
//{
//    public class PrescriptionsController : Controller
//    {
//        private readonly PetClinicContext _context;
//        //private readonly UserManager<IdentityUser> _userManager;


//        public PrescriptionsController(PetClinicContext context) /*, UserManager<IdentityUser> userManager)*/
//        {
//            _context = context;
//            //_userManager = userManager;
//        }

//        // GET: My Pet's Prescriptions
//        public async Task<IActionResult> Index(int? patientId)
//        {
//            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
//            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

//            if (owner == null)
//            {
//                return NotFound("Owner record not found");
//            }

//            IQueryable<Prescription> prescriptionsQuery = _context.Prescriptions
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Patient)
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Vet)
//                .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId);

//            if (patientId.HasValue)
//            {
//                prescriptionsQuery = prescriptionsQuery.Where(p => p.Consultation.PatientId == patientId.Value);
//            }

//            var prescriptions = await prescriptionsQuery
//                .OrderByDescending(p => p.PrescribedDate)
//                .ToListAsync();

//            ViewBag.Pets = await _context.Patients
//                .Where(p => p.OwnerId == owner.OwnerId)
//                .Select(p => new SelectListItem
//                {
//                    Value = p.PatientId.ToString(),
//                    Text = p.Name
//                })
//                .ToListAsync();

//            return View(prescriptions);
//        }

//        // GET: Prescription Details
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
//            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

//            var prescription = await _context.Prescriptions
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Patient)
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Vet)
//                .FirstOrDefaultAsync(p => p.PrescriptionId == id &&
//                                       p.Consultation.Patient.OwnerId == owner.OwnerId);

//            if (prescription == null)
//            {
//                return NotFound();
//            }

//            return View(prescription);
//        }

//        // GET: Active Prescriptions
//        public async Task<IActionResult> Active()
//        {
//            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
//            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

//            if (owner == null)
//            {
//                return NotFound("Owner record not found");
//            }

//            var activePrescriptions = await _context.Prescriptions
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Patient)
//                .Include(p => p.Consultation)
//                    .ThenInclude(c => c.Vet)
//                .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId &&
//                           p.IsDispensed == true 
//                           )
//                .OrderByDescending(p => p.PrescribedDate)
//                .ToListAsync();

//            return View(activePrescriptions);
//        }
//    }
//}



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
    [Authorize(Roles = "Staff,Veterinarian,Admin")]
    public class PrescriptionsController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(PetClinicContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Prescriptions
        public async Task<IActionResult> Index(int? patientId)
        {
            try
            {
                IQueryable<Prescription> query = _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Vet)
                    .OrderByDescending(p => p.PrescribedDate);

                // For Veterinarians, show only their prescriptions
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    query = query.Where(p => p.Consultation.VetId == vetId);
                }
                // For Staff, show all prescriptions (view only)
                else if (User.IsInRole("Staff"))
                {
                    // No additional filtering needed
                }

                // Filter by patient if specified
                if (patientId.HasValue)
                {
                    query = query.Where(p => p.Consultation.PatientId == patientId.Value);
                }

                var prescriptions = await query.ToListAsync();

                // Get patients for filter dropdown
                ViewBag.Pets = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.Name} ({p.Species})"
                    })
                    .ToListAsync();

                return View(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescriptions");
                return StatusCode(500, "An error occurred while loading prescriptions");
            }
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var prescription = await _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Vet)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id);

                if (prescription == null)
                {
                    return NotFound();
                }

                // Verify access for veterinarians
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (prescription.Consultation.VetId != vetId)
                    {
                        return Forbid();
                    }
                }

                return View(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading prescription details for ID {id}");
                return StatusCode(500, "An error occurred while loading prescription details");
            }
        }

        // GET: Prescriptions/Active
        public async Task<IActionResult> Active()
        {
            try
            {
                IQueryable<Prescription> query = _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Vet)
                    .Where(p => p.IsDispensed == true)
                    .OrderByDescending(p => p.PrescribedDate);

                // For Veterinarians, show only their active prescriptions
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    query = query.Where(p => p.Consultation.VetId == vetId);
                }

                var activePrescriptions = await query.ToListAsync();

                return View(activePrescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active prescriptions");
                return StatusCode(500, "An error occurred while loading active prescriptions");
            }
        }

        // GET: Prescriptions/Create
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Create(int? consultationId)
        {
            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var model = new PrescriptionViewModel
                {
                    AvailableConsultations = await GetAvailableConsultations(vetId),
                    PrescribedDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(30) // Default 30-day expiry
                };

                if (consultationId.HasValue)
                {
                    var consultation = await _context.Consultations
                        .Include(c => c.Patient)
                        .FirstOrDefaultAsync(c => c.ConsultationId == consultationId.Value && c.VetId == vetId);

                    if (consultation != null)
                    {
                        model.ConsultationId = consultation.ConsultationId ?? 0;
                        model.PatientId = consultation.PatientId;
                        model.PatientName = consultation.Patient.Name;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading prescription create form");
                return StatusCode(500, "An error occurred while loading the form");
            }
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Create(PrescriptionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var prescription = new Prescription
                    {
                        ConsultationId = model.ConsultationId,
                        MedicationName = model.MedicationName,
                        Dosage = model.Dosage,
                        Frequency = model.Frequency,
                        Duration = model.Duration,
                        Instructions = model.Instructions,
                        PrescribedDate = model.PrescribedDate,
                        //ExpiryDate = model.ExpiryDate, // This will need to be added to your Prescription entity
                        IsDispensed = model.IsDispensed,
                        Refills = model.RefillsRemaining // Map RefillsRemaining to Refills
                    };

                    _context.Add(prescription);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Prescription created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed; redisplay form
                model.AvailableConsultations = await GetAvailableConsultations(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                ModelState.AddModelError("", "An error occurred while creating the prescription");
                model.AvailableConsultations = await GetAvailableConsultations(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                return View(model);
            }
        }

        // GET: Prescriptions/Edit/5
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var prescription = await _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id && p.Consultation.VetId == vetId);

                if (prescription == null)
                {
                    return NotFound();
                }

                var model = new PrescriptionViewModel
                {
                    PrescriptionId =(int) prescription.PrescriptionId,
                    ConsultationId =(int) prescription.ConsultationId,
                    PatientId = prescription.Consultation.PatientId,
                    PatientName = prescription.Consultation.Patient.Name,
                    MedicationName = prescription.MedicationName,
                    Dosage = prescription.Dosage,
                    Frequency = prescription.Frequency,
                    Duration = prescription.Duration,
                    Instructions = prescription.Instructions,
                    PrescribedDate = prescription.PrescribedDate ?? DateTime.Now, // Handle nullable
                    //ExpiryDate = prescription.ExpiryDate ?? DateTime.Now.AddDays(30), // Handle nullable
                    IsDispensed = prescription.IsDispensed ?? false, // Handle nullable
                    RefillsRemaining = prescription.Refills ?? 0 // Map Refills to RefillsRemaining
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading prescription edit form for ID {id}");
                return StatusCode(500, "An error occurred while loading the form");
            }
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Edit(int id, PrescriptionViewModel model)
        {
            if (id != model.PrescriptionId)
            {
                return NotFound();
            }

            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (ModelState.IsValid)
                {
                    var prescription = await _context.Prescriptions
                        .FirstOrDefaultAsync(p => p.PrescriptionId == id && p.Consultation.VetId == vetId);

                    if (prescription == null)
                    {
                        return NotFound();
                    }

                    prescription.MedicationName = model.MedicationName;
                    prescription.Dosage = model.Dosage;
                    prescription.Frequency = model.Frequency;
                    prescription.Duration = model.Duration;
                    prescription.Instructions = model.Instructions;
                    prescription.PrescribedDate = model.PrescribedDate;
                    //prescription.ExpiryDate = model.ExpiryDate; // This will need to be added to your Prescription entity
                    prescription.IsDispensed = model.IsDispensed;
                    prescription.Refills = model.RefillsRemaining; 

                                        _context.Update(prescription);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Prescription updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating prescription ID {id}");
                ModelState.AddModelError("", "An error occurred while updating the prescription");
                return View(model);
            }
        }

        // GET: Prescriptions/Delete/5
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var prescription = await _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Vet)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id && p.Consultation.VetId == vetId);

                if (prescription == null)
                {
                    return NotFound();
                }

                return View(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading prescription delete confirmation for ID {id}");
                return StatusCode(500, "An error occurred while loading the confirmation");
            }
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var prescription = await _context.Prescriptions
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id && p.Consultation.VetId == vetId);

                if (prescription == null)
                {
                    return NotFound();
                }

                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Prescription deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting prescription ID {id}");
                return StatusCode(500, "An error occurred while deleting the prescription");
            }
        }

        private async Task<List<SelectListItem>> GetAvailableConsultations(int vetId)
        {
            return await _context.Consultations
                .Where(c => c.VetId == vetId)
                .Include(c => c.Patient)
                .Select(c => new SelectListItem
                {
                    Value = c.ConsultationId.ToString(),
                    Text = $"{c.Patient.Name} - {c.ConsultationDate:d} ({c.Diagnosis})"
                })
                .ToListAsync();
        }
    }
}