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
    public class PrescriptionsController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public PrescriptionsController(PetClinicContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: My Pet's Prescriptions
        public async Task<IActionResult> Index(int? patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            IQueryable<Prescription> prescriptionsQuery = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Vet)
                .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId);

            if (patientId.HasValue)
            {
                prescriptionsQuery = prescriptionsQuery.Where(p => p.Consultation.PatientId == patientId.Value);
            }

            var prescriptions = await prescriptionsQuery
                .OrderByDescending(p => p.PrescribedDate)
                .ToListAsync();

            ViewBag.Pets = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Name
                })
                .ToListAsync();

            return View(prescriptions);
        }

        // GET: Prescription Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Vet)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id &&
                                       p.Consultation.Patient.OwnerId == owner.OwnerId);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // GET: Active Prescriptions
        public async Task<IActionResult> Active()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            var activePrescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Vet)
                .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId &&
                           p.IsDispensed == true 
                           )
                .OrderByDescending(p => p.PrescribedDate)
                .ToListAsync();

            return View(activePrescriptions);
        }
    }
}