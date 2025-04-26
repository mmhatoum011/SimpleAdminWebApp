using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SimpleAdminWebApp.Services;
using MimeKit;
using MailKit.Net.Smtp;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;
using iText.Layout.Element;
using System.IO;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using SimpleAdminWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using iText.Layout.Properties;
using System.Text.Json;
using iText.Layout.Borders;

namespace SimpleAdminWebApp.Controllers
{
    public class ReportController : Controller
    {
        private readonly IUserService _userService;

        public ReportController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                TempData["Message"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var reports = _userService.GetReportsAssignedToUser(username);
            return View(reports);
        }

        public IActionResult Report()
        {
            var currentUser = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUser))
            {
                TempData["Message"] = "User not logged in.";
                return RedirectToAction("Login", "Login");
            }

            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "savedReports");
            var latestFile = Directory.GetFiles(reportsDir, $"{currentUser}_*.json")
     .OrderByDescending(f => new FileInfo(f).CreationTime)
     .FirstOrDefault();


            if (latestFile != null)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(latestFile);
                    var parsed = JsonSerializer.Deserialize<ReportDataModel>(json);

                    if (parsed != null && parsed.TableData != null)
                    {
                        ViewBag.TableData = parsed.TableData;
                    }
                    else
                    {
                        ViewBag.TableData = null;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Error reading report data: " + ex.Message;
                    ViewBag.TableData = null;
                }
            }
            else
            {
                ViewBag.TableData = null;
            }

            return View();
        }



    }
}

