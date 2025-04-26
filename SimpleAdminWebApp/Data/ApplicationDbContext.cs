using Microsoft.AspNetCore.Mvc;

namespace SimpleAdminWebApp.Data
{
    public class ApplicationDbContext : Controller
    {
        internal IEnumerable<object> Reports;

        public IActionResult Index()
        {
            return View();
        }
    }
}
