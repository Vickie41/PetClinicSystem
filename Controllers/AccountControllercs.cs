using Microsoft.AspNetCore.Mvc;

namespace PetClinicSystem.Controllers
{
    public class AccountControllercs : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
