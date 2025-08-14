using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace PetClinicSystem.Controllers
{
    public class ActivityLogController : Controller
    {
        private readonly PetClinicContext _context;

        public ActivityLogController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: ActivityLog
        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .Include(a => a.User) // Assumes User navigation property exists
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(logs);
        }
    }
}
