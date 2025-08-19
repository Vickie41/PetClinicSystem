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
//    public class ConsultationsController : Controller
//    {
//        private readonly PetClinicContext _context;
//        //private readonly UserManager<IdentityUser> _userManager;

//        public ConsultationsController(PetClinicContext context) /*UserManager<IdentityUser> userManager)*/
//        {
//            _context = context;
//            //_userManager = userManager;
//        }

//        // GET: My Pet's Consultations
//        public async Task<IActionResult> Index(int? patientId)
//        {
//            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
//            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

//            if (owner == null)
//            {
//                return NotFound("Owner record not found");
//            }

//            IQueryable<Consultation> consultationsQuery = _context.Consultations
//                .Include(c => c.Patient)
//                .Include(c => c.Vet)
//                .Where(c => c.Patient.OwnerId == owner.OwnerId);

//            if (patientId.HasValue)
//            {
//                consultationsQuery = consultationsQuery.Where(c => c.PatientId == patientId.Value);
//            }

//            var consultations = await consultationsQuery
//                .OrderByDescending(c => c.ConsultationDate)
//                .ToListAsync();

//            ViewBag.Pets = await _context.Patients
//                .Where(p => p.OwnerId == owner.OwnerId)
//                .Select(p => new SelectListItem
//                {
//                    Value = p.PatientId.ToString(),
//                    Text = p.Name
//                })
//                .ToListAsync();

//            return View(consultations);
//        }

//        // GET: Consultation Details
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
//            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

//            var consultation = await _context.Consultations
//                .Include(c => c.Patient)
//                .Include(c => c.Vet)
//                .Include(c => c.ConsultationTreatments)
//                    .ThenInclude(ct => ct.Treatment)
//                .Include(c => c.DiagnosticTests)
//                .Include(c => c.Prescriptions)
//                .FirstOrDefaultAsync(c => c.ConsultationId == id && c.Patient.OwnerId == owner.OwnerId);

//            if (consultation == null)
//            {
//                return NotFound();
//            }

//            return View(consultation);
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PetClinicSystem.Controllers
{
    public class ConsultationsController : Controller
    {
        private readonly PetClinicContext _context;

        public ConsultationsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: Consultations/Create
        public IActionResult Create()
        {
            var viewModel = new ConsultationViewModel
            {
                AvailablePatients = _context.Patients
                    .Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.Name })
                    .ToList(),
                AvailableVets = _context.Users
                    .Where(u => u.Role == "Veterinarian")
                    .Select(v => new SelectListItem { Value = v.UserId.ToString(), Text = v.Username })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Consultations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsultationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var consultation = new Consultation
                {
                    PatientId = model.PatientId,
                    VetId = model.VetId,
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
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            model.AvailablePatients = _context.Patients
                .Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.Name })
                .ToList();
            model.AvailableVets = _context.Users
                .Where(u => u.Role == "Veterinarian")
                .Select(v => new SelectListItem { Value = v.UserId.ToString(), Text = v.Username })
                .ToList();

            return View(model);
        }

        // GET: Consultations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return NotFound();

            var viewModel = new ConsultationViewModel
            {
                ConsultationId = consultation.ConsultationId,
                PatientId = consultation.PatientId,
                VetId = consultation.VetId,
                ConsultationDate = DateTime.Now,
                Diagnosis = consultation.Diagnosis,
                Notes = consultation.Notes,
                Weight = consultation.Weight,
                Temperature = consultation.Temperature,
                HeartRate = consultation.HeartRate,
                RespirationRate = consultation.RespirationRate,
                IsFollowUp = consultation.IsFollowUp==true,
                FollowUpDate = consultation.FollowUpDate,

                AvailablePatients = _context.Patients
                    .Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.Name })
                    .ToList(),
                AvailableVets = _context.Users
                    .Where(u => u.Role == "Veterinarian")
                    .Select(v => new SelectListItem { Value = v.UserId.ToString(), Text = v.Username })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Consultations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsultationViewModel model)
        {
            if (id != model.ConsultationId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var consultation = await _context.Consultations.FindAsync(id);
                    if (consultation == null) return NotFound();

                    consultation.PatientId = model.PatientId;
                    consultation.VetId = model.VetId;
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Consultations.Any(e => e.ConsultationId == model.ConsultationId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            model.AvailablePatients = _context.Patients
                .Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.Name })
                .ToList();
            model.AvailableVets = _context.Users
                .Where(u => u.Role == "Veterinarian")
                .Select(v => new SelectListItem { Value = v.UserId.ToString(), Text = v.Username })
                .ToList();

            return View(model);
        }

        // List page
        public async Task<IActionResult> Index()
        {
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Vet)
                .ToListAsync();

            return View(consultations);
        }
    }
}
