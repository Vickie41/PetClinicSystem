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
    public class ConsultationTreatmentsController : Controller
    {
        private readonly PetClinicContext _context;

        public ConsultationTreatmentsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: ConsultationTreatments
        public async Task<IActionResult> Index()
        {
            var petClinicContext = _context.ConsultationTreatments.Include(c => c.Consultation).Include(c => c.Treatment);
            return View(await petClinicContext.ToListAsync());
        }

        // GET: ConsultationTreatments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultationTreatment = await _context.ConsultationTreatments
                .Include(c => c.Consultation)
                .Include(c => c.Treatment)
                .FirstOrDefaultAsync(m => m.ConsultationTreatmentId == id);
            if (consultationTreatment == null)
            {
                return NotFound();
            }

            return View(consultationTreatment);
        }

        // GET: ConsultationTreatments/Create
        public IActionResult Create()
        {
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId");
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "TreatmentId", "TreatmentId");
            return View();
        }

        // POST: ConsultationTreatments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ConsultationTreatmentId,ConsultationId,TreatmentId,Details,Cost,Notes")] ConsultationTreatment consultationTreatment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(consultationTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", consultationTreatment.ConsultationId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "TreatmentId", "TreatmentId", consultationTreatment.TreatmentId);
            return View(consultationTreatment);
        }

        // GET: ConsultationTreatments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultationTreatment = await _context.ConsultationTreatments.FindAsync(id);
            if (consultationTreatment == null)
            {
                return NotFound();
            }
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", consultationTreatment.ConsultationId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "TreatmentId", "TreatmentId", consultationTreatment.TreatmentId);
            return View(consultationTreatment);
        }

        // POST: ConsultationTreatments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ConsultationTreatmentId,ConsultationId,TreatmentId,Details,Cost,Notes")] ConsultationTreatment consultationTreatment)
        {
            if (id != consultationTreatment.ConsultationTreatmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(consultationTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsultationTreatmentExists((int)consultationTreatment.ConsultationTreatmentId))
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
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", consultationTreatment.ConsultationId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatments, "TreatmentId", "TreatmentId", consultationTreatment.TreatmentId);
            return View(consultationTreatment);
        }

        // GET: ConsultationTreatments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultationTreatment = await _context.ConsultationTreatments
                .Include(c => c.Consultation)
                .Include(c => c.Treatment)
                .FirstOrDefaultAsync(m => m.ConsultationTreatmentId == id);
            if (consultationTreatment == null)
            {
                return NotFound();
            }

            return View(consultationTreatment);
        }

        // POST: ConsultationTreatments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultationTreatment = await _context.ConsultationTreatments.FindAsync(id);
            if (consultationTreatment != null)
            {
                _context.ConsultationTreatments.Remove(consultationTreatment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConsultationTreatmentExists(int id)
        {
            return _context.ConsultationTreatments.Any(e => e.ConsultationTreatmentId == id);
        }
    }
}
