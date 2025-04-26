using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimpleAdminWebApp.Models
{
    public class DataEntryModel
    {
        public string SelectedUserId { get; set; }

        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();

        public List<ReportItem> Items { get; set; } = new List<ReportItem>();
    }


}

