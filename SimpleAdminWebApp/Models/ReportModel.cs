namespace SimpleAdminWebApp.Models
{
    namespace SimpleAdminWebApp.Models
    {
        public class ReportModel
        {
            public int Id { get; set; }
            public string Data { get; set; }
            public string Signature { get; set; }

            // Add the AssignedUserId property
            public string AssignedUserId { get; set; }
        }
    }

}



