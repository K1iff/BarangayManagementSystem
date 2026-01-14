using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayLangkiwaSystem.Models
{
    public class Resident
    {
        [Key]
        public int ResidentId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime Birthdate { get; set; } = DateTime.Now;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        public string CivilStatus { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public string Purok { get; set; } = string.Empty;

        public string ContactNo { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsMigrant { get; set; }

        public string VoterStatus { get; set; } = string.Empty;

        public string Occupation { get; set; } = string.Empty;

        public string EmergencyContactName { get; set; } = string.Empty;

        public string EmergencyContactNo { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; } = DateTime.Now;
    }
}