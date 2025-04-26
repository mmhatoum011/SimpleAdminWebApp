using SimpleAdminWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAdminWebApp.Services
{
    public class UserService : IUserService
    {
        // In-memory user list
        private static List<User> users = new List<User>
        {
            new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin" }
        };

        // In-memory report list
        private static List<Models.SimpleAdminWebApp.Models.ReportModel> _reports = new List<Models.SimpleAdminWebApp.Models.ReportModel>();

        public UserService()
        {
            // Optional constructor logic
        }

        // Login method
        public bool Login(string username, string password)
        {
            return users.Any(u => u.Username == username && u.Password == password);
        }

        // Create a new user
        public bool CreateUser(string username, string password, string role)
        {
            if (users.Any(u => u.Username == username))
                return false;

            // Generate a new unique ID for the user
            var newUserId = users.Max(u => u.Id) + 1;

            // Add the new user with the generated ID
            users.Add(new User
            {
                Id = newUserId,  // Ensure we assign an integer ID
                Username = username,
                Password = password,
                Role = role
            });

            return true;
        }

        public List<User> GetAllUsers() => users;


        // Save or update a report
        public void SaveReport(Models.SimpleAdminWebApp.Models.ReportModel report)
        {
            var existing = _reports.FirstOrDefault(r => r.Id == report.Id);
            if (existing != null)
            {
                existing.Data = report.Data;
                existing.Signature = report.Signature;
                existing.AssignedUserId = report.AssignedUserId;
            }
            else
            {
                _reports.Add(report);
            }
        }

        // Get all reports assigned to a specific user
        public List<Models.SimpleAdminWebApp.Models.ReportModel> GetReportsAssignedToUser(string userId)
        {
            return _reports.Where(r => r.AssignedUserId == userId).ToList();
        }

        // Get a single report by ID
        
    }
}
