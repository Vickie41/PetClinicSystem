using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace PetClinicSystem.Controllers
{
    public class SystemSettingsController : Controller
    {
        private readonly PetClinicContext _context;

        public SystemSettingsController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: SystemSettings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return View(settings);
        }

        // GET: SystemSettings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == id);

            if (setting == null) return NotFound();

            return View(setting);
        }

        // GET: SystemSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SystemSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemSetting setting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(setting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(setting);
        }

        // GET: SystemSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var setting = await _context.SystemSettings.FindAsync(id);
            if (setting == null) return NotFound();

            return View(setting);
        }

        // POST: SystemSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SystemSetting setting)
        {
            if (id != setting.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(setting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SystemSettings.Any(s => s.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(setting);
        }

        // GET: SystemSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == id);

            if (setting == null) return NotFound();

            return View(setting);
        }

        // POST: SystemSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var setting = await _context.SystemSettings.FindAsync(id);
            if (setting != null)
            {
                _context.SystemSettings.Remove(setting);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
