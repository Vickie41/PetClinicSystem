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
    public class BillingDetailsController : Controller
    {
        private readonly PetClinicContext _context;

        public BillingDetailsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: BillingDetails
        public async Task<IActionResult> Index()
        {
            var petClinicContext = _context.BillingDetails.Include(b => b.Bill);
            return View(await petClinicContext.ToListAsync());
        }

        // GET: BillingDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingDetail = await _context.BillingDetails
                .Include(b => b.Bill)
                .FirstOrDefaultAsync(m => m.BillingDetailId == id);
            if (billingDetail == null)
            {
                return NotFound();
            }

            return View(billingDetail);
        }

        // GET: BillingDetails/Create
        public IActionResult Create()
        {
            ViewData["BillId"] = new SelectList(_context.Billings, "BillId", "BillId");
            return View();
        }

        // POST: BillingDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillingDetailId,BillId,ItemType,ItemId,Description,Quantity,UnitPrice,TotalPrice")] BillingDetail billingDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(billingDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillId"] = new SelectList(_context.Billings, "BillId", "BillId", billingDetail.BillId);
            return View(billingDetail);
        }

        // GET: BillingDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingDetail = await _context.BillingDetails.FindAsync(id);
            if (billingDetail == null)
            {
                return NotFound();
            }
            ViewData["BillId"] = new SelectList(_context.Billings, "BillId", "BillId", billingDetail.BillId);
            return View(billingDetail);
        }

        // POST: BillingDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BillingDetailId,BillId,ItemType,ItemId,Description,Quantity,UnitPrice,TotalPrice")] BillingDetail billingDetail)
        {
            if (id != billingDetail.BillingDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(billingDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingDetailExists(billingDetail.BillingDetailId))
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
            ViewData["BillId"] = new SelectList(_context.Billings, "BillId", "BillId", billingDetail.BillId);
            return View(billingDetail);
        }

        // GET: BillingDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingDetail = await _context.BillingDetails
                .Include(b => b.Bill)
                .FirstOrDefaultAsync(m => m.BillingDetailId == id);
            if (billingDetail == null)
            {
                return NotFound();
            }

            return View(billingDetail);
        }

        // POST: BillingDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var billingDetail = await _context.BillingDetails.FindAsync(id);
            if (billingDetail != null)
            {
                _context.BillingDetails.Remove(billingDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BillingDetailExists(int id)
        {
            return _context.BillingDetails.Any(e => e.BillingDetailId == id);
        }
    }
}
