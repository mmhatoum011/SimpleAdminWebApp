using Microsoft.AspNetCore.Mvc;

namespace SimpleAdminWebApp.Models
{
    public class ReportItem
    {
        public string DeliveryNo { get; set; }
        public string Project { get; set; }
        public string Customer { get; set; }
        public string Date { get; set; }
        public string PumpUse { get; set; }
        public int RequiredQuantity { get; set; }
        public int DeliveredQuantity { get; set; }
        public int TruckQuantity { get; set; }
    }
}

