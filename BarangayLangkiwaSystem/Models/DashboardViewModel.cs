using System.Collections.Generic;

namespace BarangayLangkiwaSystem.Models
{
    public class DashboardViewModel
    {
        // Top Cards
        public int TotalResidents { get; set; }
        public int CertificatesIssued { get; set; }
        public int PendingRequests { get; set; }
        public int ReportsGenerated { get; set; }

        // Population Charts
        public List<int> AgeDistributions { get; set; } = new();
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }

        // Activity Feed
        public List<ActivityLog> RecentActivities { get; set; } = new();
    }

    public class ActivityLog
    {
        public string Action { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-info-circle";
    }
}
