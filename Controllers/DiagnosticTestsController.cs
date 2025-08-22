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
    public class DiagnosticTestsController : Controller
    {
        private readonly PetClinicContext _context;

        public DiagnosticTestsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: DiagnosticTests
        public async Task<IActionResult> Index()
        {
            var petClinicContext = _context.DiagnosticTests.Include(d => d.Consultation);
            return View(await petClinicContext.ToListAsync());
        }

        // GET: DiagnosticTests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diagnosticTest = await _context.DiagnosticTests
                .Include(d => d.Consultation)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (diagnosticTest == null)
            {
                return NotFound();
            }

            return View(diagnosticTest);
        }

        // GET: DiagnosticTests/Create
        public IActionResult Create()
        {
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId");
            return View();
        }

        // POST: DiagnosticTests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,ConsultationId,TestType,TestName,TestDate,Results,Notes,Status,FilePath")] DiagnosticTest diagnosticTest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(diagnosticTest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", diagnosticTest.ConsultationId);
            return View(diagnosticTest);
        }

        // GET: DiagnosticTests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diagnosticTest = await _context.DiagnosticTests.FindAsync(id);
            if (diagnosticTest == null)
            {
                return NotFound();
            }
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", diagnosticTest.ConsultationId);
            return View(diagnosticTest);
        }

        // POST: DiagnosticTests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TestId,ConsultationId,TestType,TestName,TestDate,Results,Notes,Status,FilePath")] DiagnosticTest diagnosticTest)
        {
            if (id != diagnosticTest.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(diagnosticTest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiagnosticTestExists((int)diagnosticTest.TestId))
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
            ViewData["ConsultationId"] = new SelectList(_context.Consultations, "ConsultationId", "ConsultationId", diagnosticTest.ConsultationId);
            return View(diagnosticTest);
        }

        // GET: DiagnosticTests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diagnosticTest = await _context.DiagnosticTests
                .Include(d => d.Consultation)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (diagnosticTest == null)
            {
                return NotFound();
            }

            return View(diagnosticTest);
        }

        // POST: DiagnosticTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diagnosticTest = await _context.DiagnosticTests.FindAsync(id);
            if (diagnosticTest != null)
            {
                _context.DiagnosticTests.Remove(diagnosticTest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiagnosticTestExists(int id)
        {
            return _context.DiagnosticTests.Any(e => e.TestId == id);
        }
    }
}
