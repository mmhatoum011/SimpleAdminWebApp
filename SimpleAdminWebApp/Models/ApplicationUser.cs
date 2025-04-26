using Microsoft.AspNetCore.Mvc;

namespace SimpleAdminWebApp.Models
{
    public class ApplicationUser : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
