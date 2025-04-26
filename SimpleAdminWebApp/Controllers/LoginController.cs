using Microsoft.AspNetCore.Mvc;
using SimpleAdminWebApp.Services;
using Microsoft.AspNetCore.Http;

namespace SimpleAdminWebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserService _userService;

        public LoginController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Use the Login method to validate user credentials
            if (_userService.Login(username, password))
            {
                var user = _userService.GetAllUsers().FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    // Store username and role in session
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserId", user.Id.ToString()); // ✅ Add this
                    return RedirectToAction("Index", "Dashboard"); // or Index if that's your main page
                }
            }

            ViewBag.Message = "Invalid credentials.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Index");
        }


    }
}
