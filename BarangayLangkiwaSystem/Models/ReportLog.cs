using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayLangkiwaSystem.Models
{
    public class ReportLog
    {
        [Key]
        public int Id { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty;
        public DateTime DateGenerated { get; set; } = DateTime.Now;
    }
}
