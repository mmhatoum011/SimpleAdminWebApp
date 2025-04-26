using Microsoft.AspNetCore.Mvc;
using SimpleAdminWebApp.Models;
using SimpleAdminWebApp.Services;

namespace SimpleAdminWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // GET: Account/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            // Fetch all users and pass them to the view
            var users = _userService.GetAllUsers(); // Assuming GetAllUsers() returns a list of users
            ViewBag.Users = users;

            return View();
        }

        // POST: Account/CreateUser
        [HttpPost]
        public IActionResult CreateUser(string username, string password, string role)
        {
            if (_userService.CreateUser(username, password, role))
            {
                TempData["Message"] = "User created successfully!";
            }
            else
            {
                TempData["Message"] = "Username already exists.";
            }

            return RedirectToAction("CreateUser");
        }

        // GET: Account/ManageUsers
        [HttpGet]
        public IActionResult ManageUsers()
        {
            var users = _userService.GetAllUsers(); // Fetch all users
            return View(users); // Pass them to the view for display
        }
    }
}
