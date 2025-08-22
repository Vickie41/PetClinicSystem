using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetClinicSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PetClinicSystem.Controllers
{
    [Authorize(Roles = "Veterinarian,Staff,Admin")]
    public class ConsultationsController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(PetClinicContext context, ILogger<ConsultationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Consultations
        public async Task<IActionResult> Index(int? patientId, DateTime? fromDate, DateTime? toDate, string searchDiagnosis)
        {
            try
            {
                IQueryable<Consultation> query = _context.Consultations
                    .Include(c => c.Patient)
                    .Include(c => c.Vet)
                    .OrderByDescending(c => c.ConsultationDate);

                // Filter by patient if specified
                if (patientId.HasValue)
                {
                    query = query.Where(c => c.PatientId == patientId.Value);
                }

                // Filter by date range if specified
                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.ConsultationDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(c => c.ConsultationDate <= toDate.Value.AddDays(1));
                }

                // Filter by diagnosis search term
                if (!string.IsNullOrEmpty(searchDiagnosis))
                {
                    query = query.Where(c => c.Diagnosis.Contains(searchDiagnosis));
                }

                // For veterinarians, show only their consultations
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    query = query.Where(c => c.VetId == vetId);
                }

                var consultations = await query.ToListAsync();

                // Populate patient dropdown for filter
                ViewBag.Patients = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.Name} ({p.Species})"
                    })
                    .ToListAsync();

                return View(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consultations");
                return StatusCode(500, "An error occurred while loading consultations");
            }
        }

        // GET: Consultations/Details/5
        //public async Task<IActionResult> Details(int id)
        //{
        //    var consultation = await _context.Consultations
        //            .Include(c => c.Patient)
        //                .ThenInclude(p => p.Owner)
        //            .Include(c => c.Vet)
        //            .Include(c => c.ConsultationTreatments)
        //                .ThenInclude(ct => ct.Treatment)
        //            .Include(c => c.Prescriptions)
        //            .Include(c => c.DiagnosticTests)
        //            .Include(c => c.VaccineRecords)
        //                .ThenInclude(vr => vr.Vaccine)
        //            .FirstOrDefaultAsync(c => c.ConsultationId == id);

        //        if (consultation == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verify access for veterinarians
        //        if (User.IsInRole("Veterinarian"))
        //        {
        //            var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //            if (consultation.VetId != vetId)
        //            {
        //                return Forbid();
        //            }
        //        }

        //        return View(consultation);  
        //}


        // GET: Consultations/Details/5
        // GET: Consultations/Details/5
        //public async Task<IActionResult> Details(int id)
        //{
        //    try
        //    {
        //        // Load the consultation with all related data
        //        var consultation = await _context.Consultations
        //            .Include(c => c.Patient)
        //                .ThenInclude(p => p.Owner)
        //            .Include(c => c.Vet)
        //            .Include(c => c.ConsultationTreatments)
        //                .ThenInclude(ct => ct.Treatment)
        //            .Include(c => c.Prescriptions)
        //            .Include(c => c.DiagnosticTests)
        //            .Include(c => c.VaccineRecords)
        //                .ThenInclude(vr => vr.Vaccine)
        //            .FirstOrDefaultAsync(c => c.ConsultationId == id);

        //        if (consultation == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verify access for veterinarians
        //        if (User.IsInRole("Veterinarian"))
        //        {
        //            var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //            if (consultation.VetId != vetId)
        //            {
        //                return Forbid();
        //            }
        //        }

        //        return View(consultation);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error loading consultation details for ID {id}");

        //        TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
        //        if (ex.InnerException != null)
        //        {
        //            TempData["ErrorMessage"] += $" - {ex.InnerException.Message}";
        //        }

        //        return RedirectToAction("Index");
        //    }
        //}

        // GET: Consultations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                    var consultation = await (
                                            from c in _context.Consultations
                                            join p in _context.Patients on c.PatientId equals p.PatientId into patientJoin
                                            from p in patientJoin.DefaultIfEmpty()

                                            join o in _context.Owners on p.OwnerId equals o.OwnerId into ownerJoin
                                            from o in ownerJoin.DefaultIfEmpty()

                                            join u in _context.Users on c.VetId equals u.UserId into vetJoin
                                            from u in vetJoin.DefaultIfEmpty()

                                            join ct in _context.ConsultationTreatments on c.ConsultationId equals ct.ConsultationId into ctJoin
                                            from ct in ctJoin.DefaultIfEmpty()

                                            join t in _context.Treatments on ct.TreatmentId equals t.TreatmentId into tJoin
                                            from t in tJoin.DefaultIfEmpty()

                                            join pr in _context.Prescriptions on c.ConsultationId equals pr.ConsultationId into prJoin
                                            from pr in prJoin.DefaultIfEmpty()

                                            join dt in _context.DiagnosticTests on c.ConsultationId equals dt.ConsultationId into dtJoin
                                            from dt in dtJoin.DefaultIfEmpty()

                                            join vr in _context.VaccineRecords on c.PatientId equals vr.PatientId into vrJoin
                                            from vr in vrJoin.DefaultIfEmpty()

                                            join vac in _context.Vaccinations on vr.VaccineId equals vac.VaccineId into vacJoin
                                            from vac in vacJoin.DefaultIfEmpty()

                                            where c.ConsultationId == id
                                            select new
                                            {
                                                // Consultation
                                                c.ConsultationId,
                                                c.AppointmentId,
                                                c.VetId,
                                                c.PatientId,
                                                c.ConsultationDate,
                                                c.Weight,
                                                c.Temperature,
                                                c.HeartRate,
                                                c.RespirationRate,
                                                c.Diagnosis,
                                                c.Notes,
                                                c.FollowUpDate,
                                                c.IsFollowUp,

                                                // Patient
                                                Patient = new
                                                {
                                                    p.PatientId,
                                                    p.Name,
                                                    p.Species,
                                                    p.Breed,
                                                    p.DateOfBirth,
                                                    p.Gender
                                                },

                                                // Owner
                                                Owner = o == null ? null : new
                                                {
                                                    o.OwnerId,
                                                    o.FirstName,
                                                    o.LastName,
                                                    o.Email,
                                                    o.Phone
                                                },

                                                // Vet
                                                Vet = u == null ? null : new
                                                {
                                                    u.UserId,
                                                    u.Username,
                                                    UserEmail = u.Email,
                                                    u.Role,
                                                    u.FirstName,
                                                    u.LastName
                                                },

                                                // Treatment
                                                Treatment = ct == null ? null : new
                                                {
                                                    ct.ConsultationTreatmentId,
                                                    ct.TreatmentId,
                                                    //t.TreatmentId,
                                                    TreatmentName = t.Name,
                                                    TreatmentDescription = t.Description,
                                                    TreatmentCost = t.DefaultCost
                                                },

                                                // Prescription
                                                Prescription = pr == null ? null : new
                                                {
                                                    pr.PrescriptionId,
                                                    pr.MedicationName,
                                                    pr.Dosage,
                                                    pr.Frequency,
                                                    pr.Duration,
                                                    pr.Instructions
                                                },

                                                // Diagnostic Test
                                                DiagnosticTest = dt == null ? null : new
                                                {
                                                    dt.TestId,
                                                    dt.TestName,
                                                    dt.TestDate,
                                                    dt.Results
                                                },

                                                // Vaccine Record
                                                VaccineRecord = vr == null ? null : new
                                                {
                                                    VaccineRecordId = vr.RecordId,
                                                    vr.VaccineId,
                                                    VaccinationDate = vr.DateGiven,
                                                    vr.NextDueDate,

                                                    Vaccine = vac == null ? null : new
                                                    {
                                                        vac.VaccineId,
                                                        VaccineName = vac.Name,
                                                        VaccineDescription = vac.Description
                                                    }
                                                }
                                            }).FirstOrDefaultAsync();


                Console.WriteLine($"Consultation ID: {id}");
                if (consultation == null)
                {
                    return NotFound();
                }

                // Verify access for veterinarians
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (consultation.Vet.UserId != vetId)
                    {
                        return Forbid();
                    }
                }

                return View(consultation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading consultation details for ID {id}");

                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += $" - {ex.InnerException.Message}";
                }

                return RedirectToAction("Index");
            }
        }



        // GET: Consultations/Create
        [Authorize(Roles = "Veterinarian")]
        public IActionResult Create()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var username = User.Identity.Name;

            var viewModel = new ConsultationViewModel
            {
                ConsultationDate = DateTime.Now,
                VetName = username, // For display only
                AvailablePatients = _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.Name} ({p.Species})"
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Consultations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Create(ConsultationViewModel model)
        {
            var vetId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                // Find the most recent scheduled appointment for this patient
                var appointment = await _context.Appointments
                    .Where(a => a.PatientId == model.PatientId && a.Status == "Scheduled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (appointment == null)
                {
                    // If no appointment found, create one automatically
                    appointment = new Appointment
                    {
                        PatientId = model.PatientId,
                        VetId = vetId,
                        AppointmentDate = DateTime.Now,
                        Duration = 30, // Default duration
                        Reason = "Consultation",
                        Status = "Completed",
                        CreatedDate = DateTime.Now
                    };
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                }

                var consultation = new Consultation
                {
                    AppointmentId = appointment.AppointmentId, // CRITICAL: Set the AppointmentId
                    PatientId = model.PatientId,
                    VetId = vetId,
                    ConsultationDate = model.ConsultationDate,
                    Diagnosis = model.Diagnosis,
                    Notes = model.Notes,
                    Weight = model.Weight,
                    Temperature = model.Temperature,
                    HeartRate = model.HeartRate,
                    RespirationRate = model.RespirationRate,
                    IsFollowUp = model.IsFollowUp,
                    FollowUpDate = model.FollowUpDate
                };

                _context.Add(consultation);
                await _context.SaveChangesAsync();

                // Update appointment status to completed
                if (appointment.Status != "Completed")
                {
                    appointment.Status = "Completed";
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Details", new { id = consultation.ConsultationId });
            }
            catch (Exception ex)
            {
                // Log the error (ex.Message)
                ModelState.AddModelError("", "An error occurred while creating the consultation. Please try again.");

                // Reload dropdown data
                model.AvailablePatients = _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = $"{p.Name} ({p.Species})"
                    })
                    .ToList();

                return View(model);
            }
        }



        private async Task ReloadDropdowns(ConsultationViewModel model)
        {
            model.AvailablePatients = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = $"{p.Name} ({p.Species})",
                    Selected = (p.PatientId == model.PatientId)
                })
                .ToListAsync();

            // Reload vet name for display
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var vet = await _context.Users.FindAsync(userId);
            model.VetName = vet?.Username;
        }


        //private async Task LoadDropdowns(ConsultationViewModel model)
        //{
        //    model.AvailablePatients = await _context.Patients
        //        .Select(p => new SelectListItem
        //        {
        //            Value = p.PatientId.ToString(),
        //            Text = $"{p.Name} ({p.Species})",
        //            Selected = p.PatientId == model.PatientId
        //        })
        //        .ToListAsync();

        //    model.AvailableVets = await _context.Users
        //        .Where(u => u.Role == "Veterinarian")
        //        .Select(u => new SelectListItem
        //        {
        //            Value = u.UserId.ToString(),
        //            Text = u.Username,
        //            Selected = u.UserId == model.VetId
        //        })
        //        .ToListAsync();
        //}

        // GET: Consultations/Edit/5
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var consultation = await _context.Consultations
                    .Include(c => c.Patient)
                    .Include(c => c.Vet)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id && c.VetId == vetId);

                if (consultation == null)
                {
                    return NotFound();
                }

                var model = new ConsultationViewModel
                {
                    ConsultationId = (int)consultation.ConsultationId,
                    PatientId = consultation.PatientId,
                    PatientName = $"{consultation.Patient.Name} ({consultation.Patient.Species})",
                    //VetId = consultation.VetId,
                    VetName = consultation.Vet.Username,
                    ConsultationDate = DateTime.Now,
                    Diagnosis = consultation.Diagnosis,
                    Notes = consultation.Notes,
                    Weight = consultation.Weight,
                    Temperature = consultation.Temperature,
                    HeartRate = consultation.HeartRate,
                    RespirationRate = consultation.RespirationRate,
                    IsFollowUp = consultation.IsFollowUp ?? false,
                    FollowUpDate = consultation.FollowUpDate
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading consultation edit form for ID {id}");
                return StatusCode(500, "An error occurred while loading the form");
            }
        }

        // POST: Consultations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> Edit(int id, ConsultationViewModel model)
        {
            if (id != model.ConsultationId)
            {
                return NotFound();
            }

            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (ModelState.IsValid)
                {
                    var consultation = await _context.Consultations
                        .FirstOrDefaultAsync(c => c.ConsultationId == id && c.VetId == vetId);

                    if (consultation == null)
                    {
                        return NotFound();
                    }

                    consultation.PatientId = model.PatientId;
                    consultation.ConsultationDate = model.ConsultationDate;
                    consultation.Diagnosis = model.Diagnosis;
                    consultation.Notes = model.Notes;
                    consultation.Weight = model.Weight;
                    consultation.Temperature = model.Temperature;
                    consultation.HeartRate = model.HeartRate;
                    consultation.RespirationRate = model.RespirationRate;
                    consultation.IsFollowUp = model.IsFollowUp;
                    consultation.FollowUpDate = model.FollowUpDate;

                    _context.Update(consultation);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Consultation updated successfully!";
                    return RedirectToAction("Details", new { id = consultation.ConsultationId });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating consultation ID {id}");
                ModelState.AddModelError("", "An error occurred while updating the consultation");
                return View(model);
            }
        }

        // GET: Consultations/QuickActions/5
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> QuickActions(int id)
        {
            try
            {
                var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var consultation = await _context.Consultations
                    .Include(c => c.Patient)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id && c.VetId == vetId);

                if (consultation == null)
                {
                    return NotFound();
                }

                return View(consultation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading quick actions for consultation {id}");
                return StatusCode(500, "An error occurred while loading quick actions");
            }
        }
    }
}