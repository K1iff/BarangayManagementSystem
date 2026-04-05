using Microsoft.EntityFrameworkCore;
using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.Data
{
    public class BarangayDBContext : DbContext
    {
        public BarangayDBContext(DbContextOptions<BarangayDBContext> options) 
            : base(options) 
        { 

        }
        
        public DbSet<Resident> Residents { get; set; }
        public DbSet<CertRequest> CertificateRequests { get; set; }
        public DbSet<SystemActivity> SystemActivities { get; set; }
        public DbSet<ReportLog> ReportLogs { get; set; }
        public DbSet<CertificationType> CertificationTypes { get; set; }
        public DbSet<CertificationLog> CertificationLogs { get; set; }
        public DbSet<UserAccounts> UserAccounts { get; set; }
    }
}
