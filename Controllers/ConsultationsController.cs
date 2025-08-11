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
    public class ConsultationsController : Controller
    {
        private readonly PetClinicContext _context;
        //private readonly UserManager<IdentityUser> _userManager;

        public ConsultationsController(PetClinicContext context) /*UserManager<IdentityUser> userManager)*/
        {
            _context = context;
            //_userManager = userManager;
        }

        // GET: My Pet's Consultations
        public async Task<IActionResult> Index(int? patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            IQueryable<Consultation> consultationsQuery = _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Vet)
                .Where(c => c.Patient.OwnerId == owner.OwnerId);

            if (patientId.HasValue)
            {
                consultationsQuery = consultationsQuery.Where(c => c.PatientId == patientId.Value);
            }

            var consultations = await consultationsQuery
                .OrderByDescending(c => c.ConsultationDate)
                .ToListAsync();

            ViewBag.Pets = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Name
                })
                .ToListAsync();

            return View(consultations);
        }

        // GET: Consultation Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var consultation = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Vet)
                .Include(c => c.ConsultationTreatments)
                    .ThenInclude(ct => ct.Treatment)
                .Include(c => c.DiagnosticTests)
                .Include(c => c.Prescriptions)
                .FirstOrDefaultAsync(c => c.ConsultationId == id && c.Patient.OwnerId == owner.OwnerId);

            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }
    }
}