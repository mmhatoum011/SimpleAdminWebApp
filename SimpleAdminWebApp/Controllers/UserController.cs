using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Required for HttpContext.Session
using SimpleAdminWebApp.Services;
using SimpleAdminWebApp.Models;  // Make sure ReportModel is here

namespace SimpleAdminWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult ViewReports()
        {
            var userId = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var reports = _userService.GetReportsAssignedToUser(userId);

            if (reports == null || !reports.Any())
            {
                TempData["Message"] = "No reports found for the logged-in user.";
            }

            return View(reports);  // Ensure reports are passed here
        }
    }
}

