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
    public class BillingsController : Controller
    {
        private readonly PetClinicContext _context;
        //private readonly UserManager<IdentityUser> _userManager;


        public BillingsController(PetClinicContext context) /*, UserManager<IdentityUser> userManager)*/
        {
            _context = context;
            //_userManager = userManager;
        }

        // GET: My Invoices
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            if (owner == null)
            {
                return NotFound("Owner record not found");
            }

            var invoices = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Consultation)
                .Where(b => b.Patient.OwnerId == owner.OwnerId)
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();

            return View(invoices);
        }

        // GET: Invoice Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var invoice = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Consultation)
                .Include(b => b.BillingDetails)
                    .ThenInclude(bd => bd.Service)
                .FirstOrDefaultAsync(b => b.BillId == id && b.Patient.OwnerId == owner.OwnerId);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Make Payment
        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var invoice = await _context.Billings
                .FirstOrDefaultAsync(b => b.BillId == id &&
                                       b.Patient.OwnerId == owner.OwnerId &&
                                       b.Status == "Unpaid");

            if (invoice == null)
            {
                return NotFound();
            }

            var model = new PaymentViewModel
            {
                BillingId = invoice.BillId,
                AmountDue = invoice.TotalAmount - (invoice.PaidAmount ?? 0),
                //InvoiceNumber = invoice.InvoiceNumber
            };

            return View(model);
        }

        // POST: Process Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, PaymentViewModel model)
        {
            if (id != model.BillingId)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var invoice = await _context.Billings
                .FirstOrDefaultAsync(b => b.BillId == id &&
                                       b.Patient.OwnerId == owner.OwnerId &&
                                       b.Status == "Unpaid");

            if (invoice == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.PaymentAmount > (invoice.TotalAmount - (invoice.PaidAmount ?? 0)))
                {
                    ModelState.AddModelError("PaymentAmount", "Payment amount cannot exceed the due amount");
                    return View(model);
                }

                // Update invoice
                invoice.PaidAmount = (invoice.PaidAmount ?? 0) + model.PaymentAmount;
                invoice.Balance = invoice.TotalAmount - invoice.PaidAmount;
                invoice.BillDate = DateTime.Now;

                if (invoice.PaidAmount >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                }

                _context.Update(invoice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(PaymentConfirmation), new { id = invoice.BillId });
            }

            return View(model);
        }

        // GET: Payment Confirmation
        public async Task<IActionResult> PaymentConfirmation(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

            var invoice = await _context.Billings
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BillId == id && b.Patient.OwnerId == owner.OwnerId);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}