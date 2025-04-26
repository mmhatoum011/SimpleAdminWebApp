using Microsoft.AspNetCore.Mvc;

namespace SimpleAdminWebApp.Models
{
    public class ReportDataModel
    {
        public string UserId { get; set; }
        public string[][] TableData { get; set; }
    }
}
