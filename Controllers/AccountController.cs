using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System.Security.Claims;

namespace PetClinicSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly PetClinicContext _context;

        public AccountController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password && u.IsActive == true);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to specific dashboard based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                "Veterinarian" => RedirectToAction("VetDashboard", "Home"),
                "Staff" => RedirectToAction("StaffDashboard", "Home"),
                "Client" => RedirectToAction("ClientDashboard", "Home"),
                _ => RedirectToAction("ClientDashboard", "Home") // Default fallback
            };
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}