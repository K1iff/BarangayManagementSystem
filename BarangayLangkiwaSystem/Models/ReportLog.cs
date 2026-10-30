using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("ReportLog")]
    public class ReportLog : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("report_type")]
        public string ReportType { get; set; } = string.Empty;

        [Column("export_format")]
        public string ExportFormat { get; set; } = string.Empty;

        [Column("date_generated")]
        public DateTime DateGenerated { get; set; } = DateTime.Now;
    }
}
