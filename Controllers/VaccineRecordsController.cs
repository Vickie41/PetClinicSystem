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
    public class VaccineRecordsController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VaccineRecordsController(PetClinicContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: My Pet's Vaccination Records
        public async Task<IActionResult> Index(int? patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            IQueryable<VaccineRecord> recordsQuery = _context.VaccineRecords
                .Include(v => v.Patient)
                .Include(v => v.Vaccine)
                .Include(v => v.AdministeredByNavigation)
                .Where(v => v.Patient.OwnerId == owner.OwnerId);

            if (patientId.HasValue)
            {
                recordsQuery = recordsQuery.Where(v => v.PatientId == patientId.Value);
            }

            var records = await recordsQuery
                .OrderByDescending(v => v.DateGiven)
                .ToListAsync();

            ViewBag.Pets = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Name
                })
                .ToListAsync();

            return View(records);
        }

        // GET: Vaccine Record Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var record = await _context.VaccineRecords
                .Include(v => v.Patient)
                .Include(v => v.Vaccine)
                .Include(v => v.AdministeredByNavigation)
                .FirstOrDefaultAsync(v => v.RecordId == id && v.Patient.OwnerId == owner.OwnerId);

            if (record == null)
            {
                return NotFound();
            }

            return View(record);
        }

        // GET: Upcoming Vaccinations
        public async Task<IActionResult> Upcoming()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            var upcomingVaccines = await _context.VaccineRecords
                .Include(v => v.Patient)
                .Include(v => v.Vaccine)
                .Where(v => v.Patient.OwnerId == owner.OwnerId &&
                           v.NextDueDate.HasValue &&
                           v.NextDueDate >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(v => v.NextDueDate)
                .ToListAsync();

            return View(upcomingVaccines);
        }
    }
}