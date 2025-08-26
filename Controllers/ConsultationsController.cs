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
                // First get the main consultation with patient, owner, and vet
                var consultationData = await (
                    from c in _context.Consultations
                    join p in _context.Patients on c.PatientId equals p.PatientId
                    join o in _context.Owners on p.OwnerId equals o.OwnerId
                    join u in _context.Users on c.VetId equals u.UserId
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
                        Patient = new PatientViewModel
                        {
                            PatientId = p.PatientId,
                            Name = p.Name,
                            Species = p.Species,
                            Breed = p.Breed,
                            DateOfBirth = p.DateOfBirth.Value,
                            Gender = p.Gender,
                            Color = p.Color,
                            MicrochipId = p.MicrochipId,
                            OwnerId = p.OwnerId,
                            Allergies = p.Allergies,
                            MedicalNotes = p.MedicalNotes,
                            PhotoPath = p.PhotoPath
                        },

                        // Owner
                        Owner = new OwnerViewModel
                        {
                            OwnerId = o.OwnerId,
                            FirstName = o.FirstName,
                            LastName = o.LastName,
                            Email = o.Email,
                            Phone = o.Phone
                        },

                        // Vet
                        Vet = new VetViewModel
                        {
                            UserId = u.UserId,
                            Username = u.Username,
                            Email = u.Email,
                            Role = u.Role,
                            FirstName = u.FirstName,
                            LastName = u.LastName
                        }
                    }).FirstOrDefaultAsync();

                if (consultationData == null)
                {
                    return NotFound();
                }

                // Veterinarian access check
                if (User.IsInRole("Veterinarian"))
                {
                    var vetId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (consultationData.Vet.UserId != vetId)
                    {
                        return Forbid();
                    }
                }

                // Get treatments
                var consultationTreatments = await (
                    from ct in _context.ConsultationTreatments
                    join t in _context.Treatments on ct.TreatmentId equals t.TreatmentId
                    where ct.ConsultationId == id
                    select new ConsultationTreatmentViewModel
                    {
                        ConsultationTreatmentId = ct.ConsultationTreatmentId.Value,
                        TreatmentId = ct.TreatmentId.Value,
                        Details = ct.Details,
                        Cost = ct.Cost,
                        Notes = ct.Notes,
                        Treatment = new TreatmentViewModel
                        {
                            TreatmentId = t.TreatmentId.Value,
                            Name = t.Name,
                            Description = t.Description,
                            DefaultCost = t.DefaultCost
                        }
                    }).ToListAsync();

                // Get prescriptions
                var prescriptions = await (
                    from pr in _context.Prescriptions
                    where pr.ConsultationId == id
                    select new PrescriptionViewModel
                    {
                        PrescriptionId = pr.PrescriptionId.Value,
                        ConsultationId = pr.ConsultationId.Value,
                        PatientId = consultationData.PatientId,
                        MedicationName = pr.MedicationName,
                        Dosage = pr.Dosage,
                        Frequency = pr.Frequency,
                        Duration = pr.Duration,
                        Instructions = pr.Instructions,
                        PrescribedDate = pr.PrescribedDate.Value,
                        ExpiryDate = pr.PrescribedDate.HasValue ? pr.PrescribedDate.Value.AddDays(30) : DateTime.Now.AddDays(30),
                        IsDispensed = pr.IsDispensed ?? false,
                        RefillsRemaining = pr.Refills ?? 0,
                        PatientName = consultationData.Patient.Name
                    }).ToListAsync();

                // Get diagnostic tests
                var diagnosticTests = await (
                    from dt in _context.DiagnosticTests
                    where dt.ConsultationId == id
                    select new DiagnosticTestViewModel
                    {
                        TestId = dt.TestId.Value,
                        TestName = dt.TestName,
                        TestDate = dt.TestDate.Value,
                        Results = dt.Results
                    }).ToListAsync();

                // Get vaccine records
                var vaccineRecords = await (
                    from vr in _context.VaccineRecords
                    join vac in _context.Vaccinations on vr.VaccineId equals vac.VaccineId
                    where vr.PatientId == consultationData.Patient.PatientId
                    select new VaccineRecordViewModel
                    {
                        RecordId = vr.RecordId,
                        VaccineId = vr.VaccineId,
                        VaccinationDate = DateTime.Today,
                        NextDueDate = vr.DateGiven.ToDateTime(TimeOnly.MinValue).AddYears(1), // example: yearly calculation
                        Vaccine = new VaccineViewModel
                        {
                            VaccineId = vac.VaccineId.Value,
                            Name = vac.Name,
                            Description = vac.Description
                        }
                    }).ToListAsync();

                // Build final view model
                var viewModel = new ConsultationDetailsViewModel
                {
                    ConsultationId = consultationData.ConsultationId.Value,
                    AppointmentId = consultationData.AppointmentId,
                    VetId = consultationData.VetId,
                    PatientId = consultationData.PatientId,
                    ConsultationDate = consultationData.ConsultationDate.Value,
                    Weight = consultationData.Weight,
                    Temperature = consultationData.Temperature,
                    HeartRate = consultationData.HeartRate,
                    RespirationRate = consultationData.RespirationRate,
                    Diagnosis = consultationData.Diagnosis,
                    Notes = consultationData.Notes,
                    FollowUpDate = consultationData.FollowUpDate,
                    IsFollowUp = consultationData.IsFollowUp.Value,
                    Patient = consultationData.Patient,
                    Owner = consultationData.Owner,
                    Vet = consultationData.Vet,
                    ConsultationTreatments = consultationTreatments,
                    Prescriptions = prescriptions,
                    DiagnosticTests = diagnosticTests,
                    VaccineRecords = vaccineRecords
                };

                // CRITICAL: Populate the form models with dropdown data
                viewModel.TreatmentForm = new TreatmentFormModel
                {
                    ConsultationId = viewModel.ConsultationId,
                    AvailableTreatments = await GetTreatmentsSelectList()
                };

                viewModel.PrescriptionForm = new PrescriptionFormModel
                {
                    ConsultationId = viewModel.ConsultationId,
                    PatientId = viewModel.PatientId
                };

                viewModel.DiagnosticForm = new DiagnosticTestFormModel
                {
                    ConsultationId = viewModel.ConsultationId,
                    AvailableTests = await GetTestsSelectList()

                };

                viewModel.VaccineForm = new VaccineFormModel
                {
                    ConsultationId = viewModel.ConsultationId,
                    PatientId = viewModel.PatientId,
                    AvailableVaccines = await GetVaccinesSelectList()
                };

                return View(viewModel);
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



        // POST: Add Treatment
        [HttpPost]
        public async Task<IActionResult> AddTreatment(TreatmentFormModel model)
        {
            if (!ModelState.IsValid)
            {
                var treatment = new ConsultationTreatment
                {
                    ConsultationId = model.ConsultationId,
                    TreatmentId = model.SelectedTreatmentId,
                    Details = model.Details,
                    Cost = model.Cost,
                    Notes = model.Notes
                };

                _context.ConsultationTreatments.Add(treatment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Treatment added successfully!";
                return RedirectToAction("Details", new { id = model.ConsultationId });

            }
            try
            {
                var treatment = new ConsultationTreatment
                {
                    ConsultationId = model.ConsultationId,
                    TreatmentId = model.SelectedTreatmentId,
                    Details = model.Details,
                    Cost = model.Cost,
                    Notes = model.Notes
                };

                _context.ConsultationTreatments.Add(treatment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Treatment added successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding treatment");
                TempData["ErrorMessage"] = "Error adding treatment: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = model.ConsultationId });
        }

        // POST: Add Prescription
        [HttpPost]
        public async Task<IActionResult> AddPrescription(PrescriptionFormModel model)
        {
            
                var prescription = new Prescription
                {
                    ConsultationId = model.ConsultationId,
                    MedicationName = model.MedicationName,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Instructions = model.Instructions,
                    PrescribedDate = DateTime.Now,
                    IsDispensed = model.IsDispensed,
                    Refills = model.Refills
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Prescription added successfully!";
            
            return RedirectToAction("Details", new { id = model.ConsultationId });
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> AddDiagnosticTest(DiagnosticTestFormModel model)
        {
            
                var diagnosticTest = new DiagnosticTest
                {
                    ConsultationId = model.ConsultationId,
                    TestName = model.TestName,
                    Results = model.Results,
                    TestDate = model.TestDate
                };

                _context.DiagnosticTests.Add(diagnosticTest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Diagnostic test added successfully!";
            
            return RedirectToAction("Details", new { id = model.ConsultationId });
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> AddVaccine(VaccineFormModel model)
        {
                // Get the current user (veterinarian) ID
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var vaccineRecord = new VaccineRecord
                {
                    PatientId = model.PatientId,
                    ConsultationId = model.ConsultationId,
                    VaccineId = model.SelectedVaccineId,
                    AdministeredBy = userId, // This field is required in your entity
                    DateGiven = DateOnly.FromDateTime(model.DateGiven), // Convert DateTime to DateOnly
                    NextDueDate = model.NextDueDate.HasValue ? DateOnly.FromDateTime(model.NextDueDate.Value) : null,
                    // LotNumber and Notes can be added to your form model if needed
                    LotNumber = null,
                    Notes = null
                };

                _context.VaccineRecords.Add(vaccineRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vaccine added successfully!";
            
            return RedirectToAction("Details", new { id = model.ConsultationId });
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> DeleteTreatment(int id)
        {
            var treatment = await _context.ConsultationTreatments.FindAsync(id);
            if (treatment != null)
            {
                var consultationId = treatment.ConsultationId;
                _context.ConsultationTreatments.Remove(treatment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Treatment deleted successfully!";
                return RedirectToAction("Details", new { id = consultationId });
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                var consultationId = prescription.ConsultationId;
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Prescription deleted successfully!";
                return RedirectToAction("Details", new { id = consultationId });
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> DeleteDiagnosticTest(int id)
        {
            var test = await _context.DiagnosticTests.FindAsync(id);
            if (test != null)
            {
                var consultationId = test.ConsultationId;
                _context.DiagnosticTests.Remove(test);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Diagnostic test deleted successfully!";
                return RedirectToAction("Details", new { id = consultationId });
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> DeleteVaccine(int id)
        {
            var vaccine = await _context.VaccineRecords.FindAsync(id);
            if (vaccine != null)
            {
                var consultationId = vaccine.ConsultationId;
                _context.VaccineRecords.Remove(vaccine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vaccine record deleted successfully!";
                return RedirectToAction("Details", new { id = consultationId });
            }
            return NotFound();
        }

        // Utility: Get list of treatments for dropdown
        private async Task<List<SelectListItem>> GetTreatmentsSelectList()
        {
            return await _context.Treatments
                .Select(t => new SelectListItem
                {
                    Value = t.TreatmentId.ToString(),
                    Text = t.Name
                }).ToListAsync();
        }

        // Utility: Get list of vaccines for dropdown
        private async Task<List<SelectListItem>> GetVaccinesSelectList()
        {
            return await _context.Vaccinations
                .Select(v => new SelectListItem
                {
                    Value = v.VaccineId.ToString(),
                    Text = v.Name
                }).ToListAsync();
        }

        private async Task<List<SelectListItem>> GetTestsSelectList()
        {
            return await _context.DiagnosticTests
                .Select(v => new SelectListItem
                {
                    Value = v.TestId.ToString(),
                    Text = v.TestName
                }).ToListAsync();
        }

    }


}