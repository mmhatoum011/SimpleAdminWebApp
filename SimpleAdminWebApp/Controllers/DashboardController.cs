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
using iText.Layout.Borders;
using System;


namespace SimpleAdminWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly UserService _userService;
        private readonly int index;

        public DashboardController(UserService userService)
        {
            _userService = userService;

        }

        public IActionResult Index()
        {
            var reportsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");


            if (!Directory.Exists(reportsPath))
                Directory.CreateDirectory(reportsPath);

            var reportFiles = Directory.GetFiles(reportsPath, "*.pdf")
                .Select(Path.GetFileName)
                .OrderByDescending(name => name)
                .ToList();

            ViewBag.ReportFiles = reportFiles;
            ViewBag.ActiveTab = "Dashboard";
            return View();
        }

        [HttpPost]
        public IActionResult SubmitSignature(string SignatureData)
        {
            if (!string.IsNullOrEmpty(SignatureData))
            {
                // 1. Ensure the directory exists in wwwroot/signatures
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures");

                // If the directory doesn't exist, create it
                if (!Directory.Exists(directoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = $"Error creating directory: {ex.Message}";
                        return RedirectToAction("Report");
                    }
                }

                // 2. Save the signature image
                try
                {
                    var base64 = SignatureData.Split(',')[1];
                    var bytes = Convert.FromBase64String(base64);
                    var fileName = $"signature_{DateTime.Now.Ticks}.png";
                    var filePath = Path.Combine(directoryPath, fileName);
                    System.IO.File.WriteAllBytes(filePath, bytes);
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Error saving signature: {ex.Message}";
                    return RedirectToAction("Report");
                }
            }

            TempData["Message"] = "Signature saved successfully!";
            return RedirectToAction("Report");
        }

        // Generate PDF from table and signature
        [HttpPost]

        public async Task<IActionResult> GeneratePdf()
        {
            byte[] pdfBytes;

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Path to your logo image
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Logo.png");

                if (System.IO.File.Exists(logoPath))
                {
                    var logoData = ImageDataFactory.Create(logoPath);
                    var logo = new iText.Layout.Element.Image(logoData)
                        .ScaleToFit(100, 50)
                        .SetMarginRight(10);

                    // Wider logo column, narrower info column (adjust the ratio to control spacing)
                    var headerTable = new Table(new float[] { 1, 4 }).UseAllAvailableWidth();

                    // Logo cell
                    var logoCell = new Cell()
                        .Add(logo)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                    headerTable.AddCell(logoCell);

                    // Company info paragraph with left margin for more spacing
                    var companyInfo = new Paragraph()
                        .Add("United Brother's Concrete Co.\n")
                        .Add("Ready Mix Concrete & Block\n")
                        .Add("Mobile: 0559373300\n")
                        .Add("www.ub-cc.com")
                        .Add("info@ub-cc.com")
                        .SetFontSize(10)
                        .SetMarginLeft(300);  // <-- adjust this value to move it more right

                    var infoCell = new Cell()
                        .Add(companyInfo)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                    headerTable.AddCell(infoCell);

                    document.Add(headerTable);
                    document.Add(new Paragraph("\n")); // Space after header
                }
                else
                {
                    document.Add(new Paragraph("Logo not found at path: " + logoPath));
                }

                var data = HttpContext.Session.GetString("TableData");
                string[][] tableData = null;

                if (!string.IsNullOrEmpty(data))
                {
                    tableData = JsonSerializer.Deserialize<string[][]>(data);
                }

                if (tableData != null && tableData.Length > 0)
                {
                    // Add table title
                    document.Add(new Paragraph("Delivery Note Report")
                        .SetFontSize(16)
                        .SetBold()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginBottom(30));

                    // Manually define your column headers
                    var columnHeaders = new string[] {"Delivery NO", "Project & Location", "Customer Name", "Date", "Pump Use", "Quantity Required" , "Quantity Delivered", "Quantity Truck" };

                    var table = new Table(columnHeaders.Length);

                    // Add headers to the table
                    foreach (var header in columnHeaders)
                    {
                        table.AddHeaderCell(new Cell().Add(new Paragraph(header)).SetBold());
                    }

                    // Add data rows
                    foreach (var row in tableData)
                    {
                        foreach (var cell in row)
                        {
                            table.AddCell(cell ?? "N/A");
                        }
                    }

                    document.Add(table);
                }

                else
                {
                    document.Add(new Paragraph("No data available for the report."));
                }

                var signaturesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signatures");
                var latestSignature = Directory.GetFiles(signaturesDir, "*.png")
                    .OrderByDescending(f => new FileInfo(f).CreationTime)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(latestSignature))
                {
                    var imageData = ImageDataFactory.Create(latestSignature);
                    var image = new iText.Layout.Element.Image(imageData).ScaleToFit(200, 100);
                    document.Add(new Paragraph("Signature:"));
                    document.Add(image);
                }

                document.Close();
                pdfBytes = stream.ToArray();

                // Save the PDF to wwwroot/Reports
                var currentUser = HttpContext.Session.GetString("Username") ?? "UnknownUser";
                var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                var fileName = $"{currentUser}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(reportsDir, fileName);
                System.IO.File.WriteAllBytes(filePath, pdfBytes);

            }

            // Send email with PDF attachment
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Admin", "mmhatoum011@gmail.com"));
           
            message.To.Add(new MailboxAddress("Recipient", "m.h_1995@hotmail.com"));
            message.Subject = "Delivery Note Report";

            var builder = new BodyBuilder
            {
                TextBody = "Please find the attached report."
            };

            builder.Attachments.Add("Report.pdf", pdfBytes, new ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("mmhatoum011@gmail.com", "tzom monh umpg lmro");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            // Return the PDF as a file for download
            return File(pdfBytes, "application/pdf", "Report.pdf");
        }





        public IActionResult Report()
        {
            var currentUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["Message"] = "User not logged in.";
                return RedirectToAction("Login", "Login");
            }

            var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "savedReports");

            // Log for debugging
            Console.WriteLine($"Looking for reports in directory: {reportsDir}");

            var latestFile = Directory.GetFiles(reportsDir, $"{currentUserId}_*.json")
                                      .OrderByDescending(f => new FileInfo(f).CreationTime)
                                      .FirstOrDefault();

            if (latestFile != null)
            {
                var json = System.IO.File.ReadAllText(latestFile);
                var parsed = JsonSerializer.Deserialize<ReportDataModel>(json);

                ViewBag.TableData = parsed.TableData;

                // ✅ Save to session so GeneratePdf can access it
                HttpContext.Session.SetString("TableData", JsonSerializer.Serialize(parsed.TableData));
            }
            else
            {
                ViewBag.TableData = null;
                Console.WriteLine("No report found for the user.");  // Log for debugging
            }

            return View();
        }


        [HttpPost]
        public IActionResult SaveData(DataEntryModel model)
        {
            if (ModelState.IsValid && model.Items != null)
            {
                string[][] tableData = model.Items.Select(item => new string[]
            {       item.DeliveryNo,
                    item.Project,
                    item.Customer,
                    item.Date,
                    item.PumpUse,
                    item.RequiredQuantity.ToString(),
                    item.DeliveredQuantity.ToString(),
                    item.TruckQuantity.ToString()
                     }).ToArray();
                

                var jsonData = JsonSerializer.Serialize(tableData);
                HttpContext.Session.SetString("TableData", jsonData);

                TempData["Message"] = "Data saved successfully!";
                return RedirectToAction("Index");
            }
            if (model.Items == null)
                model.Items = new List<ReportItem>();

            // Ensure 4 items
            while (model.Items.Count < 9)
            {
                model.Items.Add(new ReportItem());
            }


            return View(model);
        }
       
       


    }
}
