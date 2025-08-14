using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace PetClinicSystem.Controllers
{
    public class PatientsController : Controller
    {
        private readonly PetClinicContext _context;
        //private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(PetClinicContext context) /*UserManager<IdentityUser> userManager*/
        {
            _context = context;
            //_userManager = userManager;
        }


        [Authorize(Roles = "Client,Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners
                .Include(o => o.Patients)
                    .ThenInclude(p => p.Consultations)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            // Get patients with related data that the view needs
            var patients = await _context.Patients
                .Where(p => p.OwnerId == owner.OwnerId)
                .Include(p => p.Owner)  // Include Owner for displaying owner name
                .Include(p => p.Consultations)  // Include Consultations for last visit info
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
            ViewData["Action"] = "Create";
            return View(new PatientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId) ??
                        await CreateOwnerForUser(userId);

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
                        Allergies = model.Allergies,
                        MedicalNotes = model.MedicalNotes,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    // Handle photo upload if provided
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        patient.PhotoPath = await SaveUploadedFile(model.PhotoFile);
                    }
                    else
                    {
                        patient.PhotoPath = null; // or set a default image path
                    }

                    _context.Add(patient);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Pet registered successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving pet: {ex.Message}");
                }
            }

            ViewData["Action"] = "Create";
            return View(model);
        }

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine("wwwroot", "UserUploadImage");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/UserUploadImage/{uniqueFileName}";
        }

        private async Task<Owner> CreateOwnerForUser(int userId)
        {
            var owner = new Owner
            {
                UserId = userId,
                FirstName = User.Identity.Name.Split(' ')[0],
                LastName = User.Identity.Name.Split(' ').Length > 1 ? User.Identity.Name.Split(' ')[1] : "",
                Email = User.Identity.Name,
                Phone = ""
            };
            _context.Add(owner);
            await _context.SaveChangesAsync();
            return owner;
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
                DateOfBirth = patient.DateOfBirth ?? default(DateOnly),
                Gender = patient.Gender,
                Color = patient.Color,
                MicrochipId = patient.MicrochipId,
                Allergies = patient.Allergies,
                MedicalNotes = patient.MedicalNotes,
                PhotoPath = patient.PhotoPath
            };

            ViewData["Action"] = "Edit";  // Add this line
            return View(model);
        }

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
                try
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.PatientId == id && p.OwnerId == owner.OwnerId);

                    if (patient == null)
                    {
                        return NotFound();
                    }

                    // Handle file upload if a new file was provided
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        // Delete old photo if exists
                        if (!string.IsNullOrEmpty(patient.PhotoPath))
                        {
                            var oldFilePath = Path.Combine("wwwroot", patient.PhotoPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload new photo
                        var uploadsFolder = Path.Combine("wwwroot", "UserUploadImage");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.PhotoFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.PhotoFile.CopyToAsync(fileStream);
                        }

                        patient.PhotoPath = $"/UserUploadImage/{uniqueFileName}";
                    }

                    // Update other properties
                    patient.Name = model.Name;
                    patient.Species = model.Species;
                    patient.Breed = model.Breed;
                    patient.DateOfBirth = model.DateOfBirth;
                    patient.Gender = model.Gender;
                    patient.Color = model.Color;
                    patient.MicrochipId = model.MicrochipId;
                    patient.Allergies = model.Allergies;
                    patient.MedicalNotes = model.MedicalNotes;

                    _context.Update(patient);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Pet record updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(model.PatientId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            ViewData["Action"] = "Edit";
            return View(model);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}