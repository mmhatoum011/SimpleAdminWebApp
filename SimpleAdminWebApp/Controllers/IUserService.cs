using SimpleAdminWebApp.Models;
using SimpleAdminWebApp.Models.SimpleAdminWebApp.Models;

namespace SimpleAdminWebApp.Services
{
    public interface IUserService
    {
        List<ReportModel> GetReportsAssignedToUser(string userId);  // Ensure you're using ReportModel here
    }
}

