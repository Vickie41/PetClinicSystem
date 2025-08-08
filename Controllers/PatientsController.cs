using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


namespace PetClinicSystem.Controllers
{
    public class PatientsController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(PetClinicContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: My Pets
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            var patients = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(patients);
        }

        // GET: Pet Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var patient = await _context.Patients
                .Include(p => p.Owner)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Vet)
                .Include(p => p.Consultations)
                    .ThenInclude(c => c.Vet)
                .Include(p => p.VaccineRecords)
                    .ThenInclude(v => v.Vaccine)
                .FirstOrDefaultAsync(p => p.PatientId == id && p.OwnerId == owner.OwnerId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Register New Pet
        public IActionResult Create()
        {
            return View(new PatientViewModel());
        }

        // POST: Register New Pet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

                if (owner == null)
                {
                    // Create owner record if it doesn't exist
                    owner = new Owner
                    {
                        UserId = userId,
                        FirstName = User.Identity.Name.Split(' ')[0],
                        LastName = User.Identity.Name.Split(' ').Length > 1 ? User.Identity.Name.Split(' ')[1] : "",
                        Email = User.Identity.Name,
                        Phone = ""
                    };
                    _context.Add(owner);
                    await _context.SaveChangesAsync();
                }

                var patient = new Patient
                {
                    OwnerId = owner.OwnerId,
                    Name = model.Name,
                    Species = model.Species,
                    Breed = model.Breed,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Color = model.Color,
                    MicrochipId = model.MicrochipId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Edit Pet Details
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id && p.OwnerId == owner.OwnerId);

            if (patient == null)
            {
                return NotFound();
            }

            var model = new PatientViewModel
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Species = patient.Species,
                Breed = patient.Breed,
                DateOfBirth = (DateOnly)patient.DateOfBirth,
                Gender = patient.Gender,
                Color = patient.Color,
                MicrochipId = patient.MicrochipId
            };

            return View(model);
        }

        // POST: Edit Pet Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientViewModel model)
        {
            if (id != model.PatientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == id && p.OwnerId == owner.OwnerId);

                if (patient == null)
                {
                    return NotFound();
                }

                patient.Name = model.Name;
                patient.Species = model.Species;
                patient.Breed = model.Breed;
                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.Color = model.Color;
                patient.MicrochipId = model.MicrochipId;

                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}