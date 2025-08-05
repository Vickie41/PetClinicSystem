using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;

namespace PetClinicSystem.Controllers
{
    public class VaccineRecordsController : Controller
    {
        private readonly PetClinicContext _context;

        public VaccineRecordsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: VaccineRecords
        public async Task<IActionResult> Index()
        {
            var petClinicContext = _context.VaccineRecords.Include(v => v.AdministeredByNavigation).Include(v => v.Patient).Include(v => v.Vaccine);
            return View(await petClinicContext.ToListAsync());
        }

        // GET: VaccineRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccineRecord = await _context.VaccineRecords
                .Include(v => v.AdministeredByNavigation)
                .Include(v => v.Patient)
                .Include(v => v.Vaccine)
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (vaccineRecord == null)
            {
                return NotFound();
            }

            return View(vaccineRecord);
        }

        // GET: VaccineRecords/Create
        public IActionResult Create()
        {
            ViewData["AdministeredBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            ViewData["VaccineId"] = new SelectList(_context.Vaccinations, "VaccineId", "VaccineId");
            return View();
        }

        // POST: VaccineRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecordId,VaccineId,PatientId,AdministeredBy,DateGiven,NextDueDate,LotNumber,Notes")] VaccineRecord vaccineRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vaccineRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdministeredBy"] = new SelectList(_context.Users, "UserId", "UserId", vaccineRecord.AdministeredBy);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", vaccineRecord.PatientId);
            ViewData["VaccineId"] = new SelectList(_context.Vaccinations, "VaccineId", "VaccineId", vaccineRecord.VaccineId);
            return View(vaccineRecord);
        }

        // GET: VaccineRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccineRecord = await _context.VaccineRecords.FindAsync(id);
            if (vaccineRecord == null)
            {
                return NotFound();
            }
            ViewData["AdministeredBy"] = new SelectList(_context.Users, "UserId", "UserId", vaccineRecord.AdministeredBy);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", vaccineRecord.PatientId);
            ViewData["VaccineId"] = new SelectList(_context.Vaccinations, "VaccineId", "VaccineId", vaccineRecord.VaccineId);
            return View(vaccineRecord);
        }

        // POST: VaccineRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,VaccineId,PatientId,AdministeredBy,DateGiven,NextDueDate,LotNumber,Notes")] VaccineRecord vaccineRecord)
        {
            if (id != vaccineRecord.RecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vaccineRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VaccineRecordExists(vaccineRecord.RecordId))
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
            ViewData["AdministeredBy"] = new SelectList(_context.Users, "UserId", "UserId", vaccineRecord.AdministeredBy);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", vaccineRecord.PatientId);
            ViewData["VaccineId"] = new SelectList(_context.Vaccinations, "VaccineId", "VaccineId", vaccineRecord.VaccineId);
            return View(vaccineRecord);
        }

        // GET: VaccineRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccineRecord = await _context.VaccineRecords
                .Include(v => v.AdministeredByNavigation)
                .Include(v => v.Patient)
                .Include(v => v.Vaccine)
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (vaccineRecord == null)
            {
                return NotFound();
            }

            return View(vaccineRecord);
        }

        // POST: VaccineRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vaccineRecord = await _context.VaccineRecords.FindAsync(id);
            if (vaccineRecord != null)
            {
                _context.VaccineRecords.Remove(vaccineRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VaccineRecordExists(int id)
        {
            return _context.VaccineRecords.Any(e => e.RecordId == id);
        }
    }
}
