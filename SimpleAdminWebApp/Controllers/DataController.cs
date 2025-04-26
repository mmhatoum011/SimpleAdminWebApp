using Microsoft.AspNetCore.Mvc;
using SimpleAdminWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleAdminWebApp.Services;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System.Text.Json;
using iText.Layout;
using iText.Layout.Properties;
using iText.Layout.Borders;

namespace SimpleAdminWebApp.Controllers

{

    public class DataController : Controller
    {
        private readonly UserService _userService;

        public DataController(UserService userService)
        {
            _userService = userService;
        }

        // GET: Data/Index
       public IActionResult Index()
{
    var model = new DataEntryModel
    {
        // Fetch the updated list of users from the service
        Users = _userService.GetAllUsers()
            .Where(u => u.Role != "Admin") // Optional: filter out admins
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),  // Convert the Id to string
                Text = u.Username
            }).ToList()
    };

    // Ensure the Items list is initialized and has at least 4 entries
    if (model.Items == null)
        model.Items = new List<ReportItem>();

    while (model.Items.Count < 7)
    {
        model.Items.Add(new ReportItem());
    }

    return View(model);
}


        // POST: Data/SaveData
        [HttpPost]
        [HttpPost]
        public IActionResult SaveData(DataEntryModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ Convert Items to tableData
                string[][] tableData = model.Items.Select(item => new string[]
                {
                    item.DeliveryNo,
            item.Project,
            item.Customer,
            item.Date,
            item.PumpUse,
            item.RequiredQuantity.ToString(),
            item.DeliveredQuantity.ToString(),
            item.TruckQuantity.ToString()
                }).ToArray();

                // ✅ Define the report save location and file name
                var userId = model.SelectedUserId.ToString();


                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "savedReports");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, $"{userId}_{timestamp}.json");

                // ✅ Serialize the report
                var reportData = new { UserId = userId, TableData = tableData };
                var json = System.Text.Json.JsonSerializer.Serialize(reportData);
                System.IO.File.WriteAllText(filePath, json);

                TempData["Message"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }

            // Re-populate the Users list if ModelState is invalid
            model.Users = _userService.GetAllUsers()
                .Where(u => u.Role != "Admin")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                }).ToList();

            return View(model);
        }


        // Optionally, add CreateUser action to handle user creation
        [HttpPost]
        public IActionResult CreateUser(string username, string password, string role)
        {
            var result = _userService.CreateUser(username, password, role);

            // Optionally, you can handle the result and show a message
            if (result)
            {
                TempData["Message"] = "User created successfully!";
            }
            else
            {
                TempData["Message"] = "User creation failed!";
            }

            // After creating the user, redirect to Index to show updated users list
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GeneratePdf()
        {
            var tableData = HttpContext.Session.GetString("TableData");
            if (string.IsNullOrEmpty(tableData))
            {
                TempData["Message"] = "No data to generate PDF.";
                return RedirectToAction("Report");
            }

            var parsedData = JsonSerializer.Deserialize<string[][]>(tableData);
            var currentUser = HttpContext.Session.GetString("Username");

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("Delivery Report").SetFontSize(18).SetBold());

            var table = new Table(4).UseAllAvailableWidth();
            table.AddHeaderCell("Project&Location");
            table.AddHeaderCell("Description");
            table.AddHeaderCell("Quantity");
            table.AddHeaderCell("Remarks");

            for (int i = 0; i < parsedData.Length; i++)
            {
                table.AddCell((i + 1).ToString());
                table.AddCell(parsedData[i][0]);
                table.AddCell(parsedData[i][1]);
                table.AddCell(parsedData[i][2]);
            }

            doc.Add(table);
            doc.Close();

            // Save the PDF to wwwroot/savedReports
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{currentUser}_{timestamp}.pdf";
            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "savedReports");

            if (!Directory.Exists(reportsDir))
                Directory.CreateDirectory(reportsDir);

            var filePath = Path.Combine(reportsDir, filename);
            System.IO.File.WriteAllBytes(filePath, ms.ToArray());

            TempData["Message"] = "PDF generated and saved successfully!";
            return RedirectToAction("Report");
        }

        public IActionResult DownloadReport()
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser))
            {
                TempData["Message"] = "User not logged in.";
                return RedirectToAction("Login", "Login");
            }

            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "savedReports");
            var latestFile = Directory.GetFiles(reportsDir, $"{currentUser}_*.json")
                           .OrderByDescending(System.IO.File.GetCreationTime)
                           .FirstOrDefault();

            if (latestFile != null)
            {
                // Use the File method available in Controller class
                var fileBytes = System.IO.File.ReadAllBytes(latestFile);  // Read the file as bytes
                var fileName = Path.GetFileName(latestFile);  // Get the file name
                return File(fileBytes, "application/json", fileName);  // Return file for download
            }

            TempData["Message"] = "No report available to download.";
            return RedirectToAction("Report");  // Redirect back to the report page if no file is found
        }
    }


}
