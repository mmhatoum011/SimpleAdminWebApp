using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SimpleAdminWebApp.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public int Id { get; set; }

        public List<SelectListItem> ExistingUsers { get; set; } = new List<SelectListItem>();
    }

}
