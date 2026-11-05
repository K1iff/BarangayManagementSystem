using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.Data
{
    public static class MockEntityExtensions
    {
        public static Task Update<T>(this T entity) where T : class
        {
            // For in-memory objects, properties are already modified on reference
            return Task.CompletedTask;
        }
    }

    public class MockQuery<T> where T : class
    {
        private readonly List<T> _source;
        private readonly Func<T, bool>? _predicate;

        public MockQuery(List<T> source, Func<T, bool>? predicate = null)
        {
            _source = source;
            _predicate = predicate;
        }

        public MockQuery<T> Where(Func<T, bool> predicate)
        {
            if (_predicate == null)
            {
                return new MockQuery<T>(_source, predicate);
            }
            var prev = _predicate;
            return new MockQuery<T>(_source, x => prev(x) && predicate(x));
        }

        public Task<MockQueryResponse<T>> Get()
        {
            List<T> list;
            lock (_source)
            {
                list = _predicate != null ? _source.Where(_predicate).ToList() : _source.ToList();
            }
            return Task.FromResult(new MockQueryResponse<T>(list));
        }

        public Task Insert(T item)
        {
            lock (_source)
            {
                _source.Add(item);
            }
            return Task.CompletedTask;
        }

        public Task Delete()
        {
            if (_predicate != null)
            {
                lock (_source)
                {
                    _source.RemoveAll(x => _predicate(x));
                }
            }
            return Task.CompletedTask;
        }
    }

    public class MockQueryResponse<T>
    {
        public List<T> Models { get; }

        public MockQueryResponse(List<T> models)
        {
            Models = models;
        }
    }

    public static class InMemoryDataStore
    {
        public static readonly List<Resident> Residents = new();
        public static readonly List<CertificationLog> CertificationLogs = new();
        public static readonly List<CertificationType> CertificationTypes = new();
        public static readonly List<ReportLog> ReportLogs = new();
        public static readonly List<SystemActivity> SystemActivities = new();
        public static readonly List<UserAccounts> UserAccounts = new();
        public static readonly List<CertRequest> CertificateRequests = new();

        static InMemoryDataStore()
        {
            var now = DateTime.Now;

            // ==========================================
            // 1. Seed User Accounts
            // ==========================================
            UserAccounts.AddRange(new[]
            {
                new UserAccounts
                {
                    user_id = 1,
                    username = "admin@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.", // Precomputed or BCrypt
                    staff_name = "Maria Elena Santos",
                    role = "Barangay Secretary",
                    is_active = true
                },
                new UserAccounts
                {
                    user_id = 2,
                    username = "captain@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Hon. Roberto D. Alcantara (Captain)",
                    role = "Captain",
                    is_active = true
                },
                new UserAccounts
                {
                    user_id = 3,
                    username = "encoder1@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Juan Carlo Reyes",
                    role = "Barangay Staff",
                    is_active = true
                },
                new UserAccounts
                {
                    user_id = 4,
                    username = "encoder2@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Angelica Mae Flores",
                    role = "Barangay Staff",
                    is_active = true
                },
                new UserAccounts
                {
                    user_id = 5,
                    username = "inactive_staff@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Ricardo P. Gomez",
                    role = "Barangay Staff",
                    is_active = false
                },
                new UserAccounts
                {
                    user_id = 101,
                    username = "clerk@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Juan Dela Cruz (Clerk)",
                    role = "Clerk",
                    is_active = true
                },
                new UserAccounts
                {
                    user_id = 102,
                    username = "secretary@langkiwa.gov.ph",
                    password = "$2a$11$X0UqQWcZ6w47cK5fM4tJge0jP1kQ2lR3mS4nT5oU6pV7qW8rS9tU.",
                    staff_name = "Maria Elena Santos (Secretary)",
                    role = "Secretary",
                    is_active = true
                }
            });

            // Set passwords using BCrypt.Net.BCrypt.HashPassword so verification matches exactly
            foreach (var user in UserAccounts)
            {
                user.password = BCrypt.Net.BCrypt.HashPassword("admin123");
            }

            // ==========================================
            // 2. Seed Certification Types
            // ==========================================
            CertificationTypes.AddRange(new[]
            {
                new CertificationType { TypeId = 1, TypeName = "Barangay Clearance" },
                new CertificationType { TypeId = 2, TypeName = "Certificate of Indigency" },
                new CertificationType { TypeId = 3, TypeName = "Certificate of Residency" },
                new CertificationType { TypeId = 4, TypeName = "Barangay Business Clearance" },
                new CertificationType { TypeId = 5, TypeName = "Certificate of Good Moral Character" }
            });

            // ==========================================
            // 3. Seed Comprehensive Residents
            // ==========================================
            Residents.AddRange(new[]
            {
                new Resident
                {
                    ResidentId = 1,
                    FirstName = "Juan",
                    MiddleName = "Dela",
                    LastName = "Cruz",
                    Birthdate = new DateTime(1988, 5, 14),
                    Gender = "Male",
                    CivilStatus = "Married",
                    Address = "Block 12 Lot 4, Mabuhay St., Purok 1",
                    Purok = "Purok 1",
                    ContactNo = "09171234567",
                    Email = "juan.delacruz@gmail.com",
                    IsMigrant = false,
                    Occupation = "Civil Engineer",
                    EmergencyContactName = "Juana Dela Cruz",
                    EmergencyContactNo = "09177654321",
                    DateRegistered = now.AddMonths(-14)
                },
                new Resident
                {
                    ResidentId = 2,
                    FirstName = "Maria",
                    MiddleName = "Clara",
                    LastName = "Santos",
                    Birthdate = new DateTime(1995, 8, 22),
                    Gender = "Female",
                    CivilStatus = "Single",
                    Address = "Unit 3B Sunshine Compound, Purok 2",
                    Purok = "Purok 2",
                    ContactNo = "09209876543",
                    Email = "maria.santos@outlook.com",
                    IsMigrant = false,
                    Occupation = "Public School Teacher",
                    EmergencyContactName = "Clara Santos",
                    EmergencyContactNo = "09201112233",
                    DateRegistered = now.AddMonths(-8)
                },
                new Resident
                {
                    ResidentId = 3,
                    FirstName = "Antonio",
                    MiddleName = "Luna",
                    LastName = "Reyes",
                    Birthdate = new DateTime(1958, 10, 29), // Senior citizen (65+)
                    Gender = "Male",
                    CivilStatus = "Widowed",
                    Address = "104 Sampaguita St., Purok 3",
                    Purok = "Purok 3",
                    ContactNo = "09183456789",
                    Email = "antonio.reyes@yahoo.com",
                    IsMigrant = false,
                    Occupation = "Retired Government Employee",
                    EmergencyContactName = "Joaquin Reyes",
                    EmergencyContactNo = "09189998877",
                    DateRegistered = now.AddYears(-3)
                },
                new Resident
                {
                    ResidentId = 4,
                    FirstName = "Carmela",
                    MiddleName = "Bautista",
                    LastName = "Mendoza",
                    Birthdate = new DateTime(2001, 3, 17),
                    Gender = "Female",
                    CivilStatus = "Single",
                    Address = "Lot 7 Phase 2, Villa Langkiwa, Purok 4",
                    Purok = "Purok 4",
                    ContactNo = "09327891234",
                    Email = "carmela.mendoza@gmail.com",
                    IsMigrant = true,
                    Occupation = "Call Center Agent",
                    EmergencyContactName = "Ernesto Mendoza",
                    EmergencyContactNo = "09325556677",
                    DateRegistered = now.AddDays(-15) // Recent (This month)
                },
                new Resident
                {
                    ResidentId = 5,
                    FirstName = "Rodrigo",
                    MiddleName = "Perez",
                    LastName = "Aquino",
                    Birthdate = new DateTime(1974, 12, 5),
                    Gender = "Male",
                    CivilStatus = "Married",
                    Address = "Block 5 Lot 18, Rosal St., Purok 1",
                    Purok = "Purok 1",
                    ContactNo = "09456781234",
                    Email = "rodrigo.aquino@gmail.com",
                    IsMigrant = false,
                    Occupation = "Jeepney Driver / Operator",
                    EmergencyContactName = "Corazon Aquino",
                    EmergencyContactNo = "09458887766",
                    DateRegistered = now.AddMonths(-18)
                },
                new Resident
                {
                    ResidentId = 6,
                    FirstName = "Lourdes",
                    MiddleName = "Garcia",
                    LastName = "Castillo",
                    Birthdate = new DateTime(1960, 4, 11), // Senior citizen (60+)
                    Gender = "Female",
                    CivilStatus = "Married",
                    Address = "21 Ilang-Ilang St., Purok 5",
                    Purok = "Purok 5",
                    ContactNo = "09228901234",
                    Email = "lourdes.castillo@gmail.com",
                    IsMigrant = false,
                    Occupation = "Sari-Sari Store Owner",
                    EmergencyContactName = "Manuel Castillo",
                    EmergencyContactNo = "09223334455",
                    DateRegistered = now.AddMonths(-5)
                },
                new Resident
                {
                    ResidentId = 7,
                    FirstName = "Kenneth",
                    MiddleName = "Velasco",
                    LastName = "Ramos",
                    Birthdate = new DateTime(2009, 9, 15), // Minor (Age < 18)
                    Gender = "Male",
                    CivilStatus = "Single",
                    Address = "Block 12 Lot 4, Mabuhay St., Purok 1",
                    Purok = "Purok 1",
                    ContactNo = "09171234567",
                    Email = "",
                    IsMigrant = false,
                    Occupation = "High School Student",
                    EmergencyContactName = "Juan Dela Cruz",
                    EmergencyContactNo = "09171234567",
                    DateRegistered = now.AddMonths(-14)
                },
                new Resident
                {
                    ResidentId = 8,
                    FirstName = "Aileen",
                    MiddleName = "Morales",
                    LastName = "Domingo",
                    Birthdate = new DateTime(1992, 1, 30),
                    Gender = "Female",
                    CivilStatus = "Married",
                    Address = "45 Narra Extension, Purok 6",
                    Purok = "Purok 6",
                    ContactNo = "09561239874",
                    Email = "aileen.domingo@hotmail.com",
                    IsMigrant = true,
                    Occupation = "Registered Nurse",
                    EmergencyContactName = "Danilo Domingo",
                    EmergencyContactNo = "09568884422",
                    DateRegistered = now.AddDays(-2) // This week
                },
                new Resident
                {
                    ResidentId = 9,
                    FirstName = "Danilo",
                    MiddleName = "Soriano",
                    LastName = "Domingo",
                    Birthdate = new DateTime(1990, 6, 18),
                    Gender = "Male",
                    CivilStatus = "Married",
                    Address = "45 Narra Extension, Purok 6",
                    Purok = "Purok 6",
                    ContactNo = "09568884422",
                    Email = "danilo.domingo@gmail.com",
                    IsMigrant = true,
                    Occupation = "Software Developer",
                    EmergencyContactName = "Aileen Domingo",
                    EmergencyContactNo = "09561239874",
                    DateRegistered = now.AddDays(-2) // This week
                },
                new Resident
                {
                    ResidentId = 10,
                    FirstName = "Beatrice",
                    MiddleName = "Lopez",
                    LastName = "Villanueva",
                    Birthdate = new DateTime(1998, 11, 8),
                    Gender = "Female",
                    CivilStatus = "Single",
                    Address = "18 Orchid Lane, Purok 2",
                    Purok = "Purok 2",
                    ContactNo = "09276543210",
                    Email = "bea.villanueva@gmail.com",
                    IsMigrant = false,
                    Occupation = "Bank Teller",
                    EmergencyContactName = "Teresa Villanueva",
                    EmergencyContactNo = "09279991122",
                    DateRegistered = now.AddMonths(-1) // Last month
                },
                new Resident
                {
                    ResidentId = 11,
                    FirstName = "Francisco",
                    MiddleName = "Baltazar",
                    LastName = "Tolentino",
                    Suffix = "Jr.",
                    Birthdate = new DateTime(1963, 7, 24), // Senior (60+)
                    Gender = "Male",
                    CivilStatus = "Married",
                    Address = "72 Kamagong St., Purok 3",
                    Purok = "Purok 3",
                    ContactNo = "09194567890",
                    Email = "francisco.tolentino@gmail.com",
                    IsMigrant = false,
                    Occupation = "Barangay Tanod Officer",
                    EmergencyContactName = "Gloria Tolentino",
                    EmergencyContactNo = "09198882233",
                    DateRegistered = now.AddYears(-2)
                },
                new Resident
                {
                    ResidentId = 12,
                    FirstName = "Princess",
                    MiddleName = "Mae",
                    LastName = "Navarro",
                    Birthdate = new DateTime(2003, 2, 14),
                    Gender = "Female",
                    CivilStatus = "Single",
                    Address = "Lot 15 Block 3, Diamond St., Purok 5",
                    Purok = "Purok 5",
                    ContactNo = "09391122334",
                    Email = "princess.navarro@gmail.com",
                    IsMigrant = false,
                    Occupation = "College Student (Scholar)",
                    EmergencyContactName = "Rowena Navarro",
                    EmergencyContactNo = "09397776655",
                    DateRegistered = now.AddDays(-25) // This month
                },
                new Resident
                {
                    ResidentId = 13,
                    FirstName = "Gerald",
                    MiddleName = "Lim",
                    LastName = "Tan",
                    Birthdate = new DateTime(1982, 10, 10),
                    Gender = "Male",
                    CivilStatus = "Married",
                    Address = "88 Commercial Row, Purok 4",
                    Purok = "Purok 4",
                    ContactNo = "09175551234",
                    Email = "gerald.tan@bizph.com",
                    IsMigrant = false,
                    Occupation = "Hardware Merchant",
                    EmergencyContactName = "Grace Tan",
                    EmergencyContactNo = "09175558899",
                    DateRegistered = now.AddMonths(-11)
                },
                new Resident
                {
                    ResidentId = 14,
                    FirstName = "Elena",
                    MiddleName = "Rosales",
                    LastName = "Enriquez",
                    Birthdate = new DateTime(1952, 1, 19), // Senior (70+)
                    Gender = "Female",
                    CivilStatus = "Widowed",
                    Address = "12 Acacia Way, Purok 6",
                    Purok = "Purok 6",
                    ContactNo = "09214445566",
                    Email = "",
                    IsMigrant = false,
                    Occupation = "Pensioner",
                    EmergencyContactName = "Salvador Enriquez",
                    EmergencyContactNo = "09219994433",
                    DateRegistered = now.AddYears(-4)
                },
                new Resident
                {
                    ResidentId = 15,
                    FirstName = "Marc",
                    MiddleName = "Julian",
                    LastName = "Salazar",
                    Birthdate = new DateTime(1996, 12, 3),
                    Gender = "Male",
                    CivilStatus = "Single",
                    Address = "Block 8 Lot 22, Greenview Subd., Purok 2",
                    Purok = "Purok 2",
                    ContactNo = "09167778899",
                    Email = "marc.salazar@techcorp.io",
                    IsMigrant = true,
                    Occupation = "Graphic Designer",
                    EmergencyContactName = "Lucia Salazar",
                    EmergencyContactNo = "09163332211",
                    DateRegistered = now.Date // Registered Today
                },
                new Resident
                {
                    ResidentId = 16,
                    FirstName = "Jasmine",
                    MiddleName = "Kay",
                    LastName = "Fernandez",
                    Birthdate = new DateTime(2000, 7, 7),
                    Gender = "Female",
                    CivilStatus = "Single",
                    Address = "Lot 9 Golden Hills, Purok 3",
                    Purok = "Purok 3",
                    ContactNo = "09285559911",
                    Email = "jasmine.fernandez@gmail.com",
                    IsMigrant = false,
                    Occupation = "Customer Support Specialist",
                    EmergencyContactName = "Rosa Fernandez",
                    EmergencyContactNo = "09282223344",
                    DateRegistered = now.AddMonths(-2)
                }
            });

            // ==========================================
            // 4. Seed Certification Logs
            // ==========================================
            CertificationLogs.AddRange(new[]
            {
                new CertificationLog
                {
                    CertId = 101,
                    resident_id = 1,
                    type_id = 1, // Barangay Clearance
                    purpose = "Local Employment Application",
                    date_issued = now.Date.AddHours(9), // Today
                    status = "Approved",
                    issued_by = 1,
                    reference_code = "BC-2026-00101",
                    IssuedBy = "Juan Dela Cruz",
                    ReferenceCode = "Barangay Clearance"
                },
                new CertificationLog
                {
                    CertId = 102,
                    resident_id = 2,
                    type_id = 3, // Certificate of Residency
                    purpose = "Bank Account Opening Requirement",
                    date_issued = now.Date.AddHours(10), // Today
                    status = "Approved",
                    issued_by = 3,
                    reference_code = "CR-2026-00102",
                    IssuedBy = "Maria Clara Santos",
                    ReferenceCode = "Certificate of Residency"
                },
                new CertificationLog
                {
                    CertId = 103,
                    resident_id = 4,
                    type_id = 2, // Certificate of Indigency
                    purpose = "Medical Assistance - Biñan City Hospital",
                    date_issued = now.AddDays(-1).AddHours(14), // This week
                    status = "Approved",
                    issued_by = 4,
                    reference_code = "CI-2026-00103",
                    IssuedBy = "Carmela Mendoza",
                    ReferenceCode = "Certificate of Indigency"
                },
                new CertificationLog
                {
                    CertId = 104,
                    resident_id = 12,
                    type_id = 2, // Certificate of Indigency
                    purpose = "Educational Assistance & Scholarship Grant",
                    date_issued = now.AddDays(-3).AddHours(11), // This week
                    status = "Approved",
                    issued_by = 3,
                    reference_code = "CI-2026-00104",
                    IssuedBy = "Princess Navarro",
                    ReferenceCode = "Certificate of Indigency"
                },
                new CertificationLog
                {
                    CertId = 105,
                    resident_id = 13,
                    type_id = 4, // Barangay Business Clearance
                    purpose = "Annual Business Permit Renewal",
                    date_issued = now.AddDays(-5).AddHours(15), // This week
                    status = "Approved",
                    issued_by = 1,
                    reference_code = "BBC-2026-00105",
                    IssuedBy = "Gerald Tan",
                    ReferenceCode = "Barangay Business Clearance"
                },
                new CertificationLog
                {
                    CertId = 106,
                    resident_id = 8,
                    type_id = 1, // Barangay Clearance
                    purpose = "PRC License Renewal Requirement",
                    date_issued = now.AddDays(-10).AddHours(10), // This month
                    status = "Approved",
                    issued_by = 3,
                    reference_code = "BC-2026-00106",
                    IssuedBy = "Aileen Domingo",
                    ReferenceCode = "Barangay Clearance"
                },
                new CertificationLog
                {
                    CertId = 107,
                    resident_id = 9,
                    type_id = 3, // Certificate of Residency
                    purpose = "Postal ID Application Requirement",
                    date_issued = now.AddDays(-12).AddHours(16), // This month
                    status = "Approved",
                    issued_by = 4,
                    reference_code = "CR-2026-00107",
                    IssuedBy = "Danilo Domingo",
                    ReferenceCode = "Certificate of Residency"
                },
                new CertificationLog
                {
                    CertId = 108,
                    resident_id = 5,
                    type_id = 1, // Barangay Clearance
                    purpose = "Tricycle / PUV Franchise Renewal",
                    date_issued = now.AddDays(-16).AddHours(13), // This month
                    status = "Disapproved",
                    issued_by = 1,
                    reference_code = "BC-2026-00108",
                    IssuedBy = "Rodrigo Aquino",
                    ReferenceCode = "Barangay Clearance"
                },
                new CertificationLog
                {
                    CertId = 109,
                    resident_id = 6,
                    type_id = 4, // Barangay Business Clearance
                    purpose = "New Sari-Sari Store Registration",
                    date_issued = now.AddDays(-20).AddHours(9), // This month
                    status = "Approved",
                    issued_by = 3,
                    reference_code = "BBC-2026-00109",
                    IssuedBy = "Lourdes Castillo",
                    ReferenceCode = "Barangay Business Clearance"
                },
                new CertificationLog
                {
                    CertId = 110,
                    resident_id = 3,
                    type_id = 2, // Certificate of Indigency
                    purpose = "Senior Citizen Subsidy Program",
                    date_issued = now.AddMonths(-1).AddDays(5), // Last month
                    status = "Approved",
                    issued_by = 1,
                    reference_code = "CI-2026-00110",
                    IssuedBy = "Antonio Reyes",
                    ReferenceCode = "Certificate of Indigency"
                },
                new CertificationLog
                {
                    CertId = 111,
                    resident_id = 10,
                    type_id = 5, // Certificate of Good Moral Character
                    purpose = "Overseas Employment (POEA)",
                    date_issued = now.AddMonths(-1).AddDays(10), // Last month
                    status = "Approved",
                    issued_by = 1,
                    reference_code = "CGM-2026-00111",
                    IssuedBy = "Beatrice Villanueva",
                    ReferenceCode = "Certificate of Good Moral Character"
                },
                new CertificationLog
                {
                    CertId = 112,
                    resident_id = 11,
                    type_id = 1, // Barangay Clearance
                    purpose = "Firearm License Renewal Support",
                    date_issued = now.AddMonths(-1).AddDays(15), // Last month
                    status = "Approved",
                    issued_by = 4,
                    reference_code = "BC-2026-00112",
                    IssuedBy = "Francisco Tolentino Jr.",
                    ReferenceCode = "Barangay Clearance"
                },
                new CertificationLog
                {
                    CertId = 113,
                    resident_id = 14,
                    type_id = 3, // Certificate of Residency
                    purpose = "GSIS Pension Verification Requirement",
                    date_issued = now.AddMonths(-2).AddDays(4), // Older
                    status = "Approved",
                    issued_by = 1,
                    reference_code = "CR-2026-00113",
                    IssuedBy = "Elena Enriquez",
                    ReferenceCode = "Certificate of Residency"
                },
                new CertificationLog
                {
                    CertId = 114,
                    resident_id = 15,
                    type_id = 1, // Barangay Clearance
                    purpose = "Passport / DFA Application Requirement",
                    date_issued = now.Date.AddHours(11), // Today
                    status = "Pending",
                    issued_by = 3,
                    reference_code = "BC-2026-00114",
                    IssuedBy = "Marc Salazar",
                    ReferenceCode = "Barangay Clearance"
                },
                new CertificationLog
                {
                    CertId = 115,
                    resident_id = 16,
                    type_id = 1, // Barangay Clearance
                    purpose = "NBI Clearance Requirement",
                    date_issued = now.AddDays(-4).AddHours(14), // This week
                    status = "Approved",
                    issued_by = 3,
                    reference_code = "BC-2026-00115",
                    IssuedBy = "Jasmine Fernandez",
                    ReferenceCode = "Barangay Clearance"
                }
            });

            // ==========================================
            // 5. Seed Certificate Requests (Powers Reports Recent Submissions & Trends)
            // ==========================================
            CertificateRequests.AddRange(new[]
            {
                new CertRequest
                {
                    RequestId = 1,
                    ResidentId = 15,
                    ResidentName = "Marc Salazar",
                    FirstName = "Marc",
                    LastName = "Salazar",
                    CertificateType = "Barangay Clearance",
                    Purpose = "Passport / DFA Application Requirement",
                    RequestDate = now.Date.AddHours(8),
                    Status = "Pending"
                },
                new CertRequest
                {
                    RequestId = 2,
                    ResidentId = 1,
                    ResidentName = "Juan Cruz",
                    FirstName = "Juan",
                    LastName = "Cruz",
                    CertificateType = "Barangay Clearance",
                    Purpose = "Local Employment Application",
                    RequestDate = now.Date.AddHours(7),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 3,
                    ResidentId = 2,
                    ResidentName = "Maria Clara Santos",
                    FirstName = "Maria",
                    LastName = "Santos",
                    CertificateType = "Certificate of Residency",
                    Purpose = "Bank Account Opening Requirement",
                    RequestDate = now.AddDays(-1).AddHours(9),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 4,
                    ResidentId = 4,
                    ResidentName = "Carmela Mendoza",
                    FirstName = "Carmela",
                    LastName = "Mendoza",
                    CertificateType = "Certificate of Indigency",
                    Purpose = "Medical Assistance - Biñan City Hospital",
                    RequestDate = now.AddDays(-2).AddHours(10),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 5,
                    ResidentId = 8,
                    ResidentName = "Aileen Domingo",
                    FirstName = "Aileen",
                    LastName = "Domingo",
                    CertificateType = "Barangay Clearance",
                    Purpose = "PRC License Renewal Requirement",
                    RequestDate = now.AddDays(-3).AddHours(11),
                    Status = "Pending"
                },
                new CertRequest
                {
                    RequestId = 6,
                    ResidentId = 12,
                    ResidentName = "Princess Navarro",
                    FirstName = "Princess",
                    LastName = "Navarro",
                    CertificateType = "Certificate of Indigency",
                    Purpose = "Educational Assistance & Scholarship Grant",
                    RequestDate = now.AddDays(-4).AddHours(13),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 7,
                    ResidentId = 13,
                    ResidentName = "Gerald Tan",
                    FirstName = "Gerald",
                    LastName = "Tan",
                    CertificateType = "Barangay Business Clearance",
                    Purpose = "Annual Business Permit Renewal",
                    RequestDate = now.AddDays(-6).AddHours(15),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 8,
                    ResidentId = 5,
                    ResidentName = "Rodrigo Aquino",
                    FirstName = "Rodrigo",
                    LastName = "Aquino",
                    CertificateType = "Barangay Clearance",
                    Purpose = "Tricycle / PUV Franchise Renewal",
                    RequestDate = now.AddDays(-15).AddHours(10),
                    Status = "Pending"
                },
                new CertRequest
                {
                    RequestId = 9,
                    ResidentId = 10,
                    ResidentName = "Beatrice Villanueva",
                    FirstName = "Beatrice",
                    LastName = "Villanueva",
                    CertificateType = "Certificate of Good Moral Character",
                    Purpose = "Overseas Employment (POEA)",
                    RequestDate = now.AddMonths(-1).AddDays(8),
                    Status = "Completed"
                },
                new CertRequest
                {
                    RequestId = 10,
                    ResidentId = 3,
                    ResidentName = "Antonio Reyes",
                    FirstName = "Antonio",
                    LastName = "Reyes",
                    CertificateType = "Certificate of Indigency",
                    Purpose = "Senior Citizen Subsidy Program",
                    RequestDate = now.AddMonths(-1).AddDays(12),
                    Status = "Completed"
                }
            });

            // ==========================================
            // 6. Seed System Activities (Powers Dashboard Activity Log & Activity Report)
            // ==========================================
            SystemActivities.AddRange(new[]
            {
                new SystemActivity
                {
                    Id = 1,
                    Action = "Issued Barangay Clearance for Juan Cruz",
                    UserRole = "Barangay Secretary",
                    Icon = "bi-file-earmark-check text-success",
                    Timestamp = now.AddMinutes(-15)
                },
                new SystemActivity
                {
                    Id = 2,
                    Action = "New resident registered: Marc Salazar",
                    UserRole = "Barangay Staff",
                    Icon = "bi-person-plus text-primary",
                    Timestamp = now.AddHours(-1)
                },
                new SystemActivity
                {
                    Id = 3,
                    Action = "Approved Certificate of Residency for Maria Clara Santos",
                    UserRole = "Barangay Staff",
                    Icon = "bi-check2-circle text-success",
                    Timestamp = now.AddHours(-3)
                },
                new SystemActivity
                {
                    Id = 4,
                    Action = "Generated Resident Summary Report (Purok Statistics)",
                    UserRole = "Barangay Secretary",
                    Icon = "bi-printer text-info",
                    Timestamp = now.AddHours(-6)
                },
                new SystemActivity
                {
                    Id = 5,
                    Action = "Updated resident profile: Carmela Mendoza",
                    UserRole = "Barangay Staff",
                    Icon = "bi-pencil-square text-warning",
                    Timestamp = now.AddDays(-1)
                },
                new SystemActivity
                {
                    Id = 6,
                    Action = "Exported System Activity Report to Excel",
                    UserRole = "Barangay Secretary",
                    Icon = "bi-file-earmark-excel text-success",
                    Timestamp = now.AddDays(-2)
                },
                new SystemActivity
                {
                    Id = 7,
                    Action = "New resident registered: Aileen Domingo",
                    UserRole = "Barangay Staff",
                    Icon = "bi-person-plus text-primary",
                    Timestamp = now.AddDays(-2).AddHours(-4)
                },
                new SystemActivity
                {
                    Id = 8,
                    Action = "Deactivated user account: Ricardo P. Gomez",
                    UserRole = "Barangay Captain",
                    Icon = "bi-shield-lock text-danger",
                    Timestamp = now.AddDays(-5)
                }
            });

            // ==========================================
            // 7. Seed Report Logs (Powers Reports Summary Cards & Export History)
            // ==========================================
            ReportLogs.AddRange(new[]
            {
                new ReportLog
                {
                    Id = 1,
                    ReportType = "Resident Summary Report",
                    ExportFormat = "Print/PDF",
                    DateGenerated = now.Date.AddHours(10)
                },
                new ReportLog
                {
                    Id = 2,
                    ReportType = "System Activity Report",
                    ExportFormat = "Excel",
                    DateGenerated = now.AddDays(-1).AddHours(14)
                },
                new ReportLog
                {
                    Id = 3,
                    ReportType = "Certificate Issuance Report",
                    ExportFormat = "Excel",
                    DateGenerated = now.AddDays(-2).AddHours(9)
                },
                new ReportLog
                {
                    Id = 4,
                    ReportType = "Resident Summary Report",
                    ExportFormat = "Excel",
                    DateGenerated = now.AddDays(-4).AddHours(16)
                },
                new ReportLog
                {
                    Id = 5,
                    ReportType = "Certificate Issuance Report",
                    ExportFormat = "Print/PDF",
                    DateGenerated = now.AddDays(-6).AddHours(11)
                },
                new ReportLog
                {
                    Id = 6,
                    ReportType = "Resident Summary Report",
                    ExportFormat = "Print/PDF",
                    DateGenerated = now.AddMonths(-1).AddDays(5)
                },
                new ReportLog
                {
                    Id = 7,
                    ReportType = "System Activity Report",
                    ExportFormat = "Excel",
                    DateGenerated = now.AddMonths(-1).AddDays(18)
                },
                new ReportLog
                {
                    Id = 8,
                    ReportType = "Certificate Issuance Report",
                    ExportFormat = "Excel",
                    DateGenerated = now.AddMonths(-2).AddDays(10)
                }
            });
        }

        public static MockQuery<T> From<T>() where T : class
        {
            if (typeof(T) == typeof(Resident)) return new MockQuery<T>((List<T>)(object)Residents);
            if (typeof(T) == typeof(CertificationLog)) return new MockQuery<T>((List<T>)(object)CertificationLogs);
            if (typeof(T) == typeof(CertificationType)) return new MockQuery<T>((List<T>)(object)CertificationTypes);
            if (typeof(T) == typeof(ReportLog)) return new MockQuery<T>((List<T>)(object)ReportLogs);
            if (typeof(T) == typeof(SystemActivity)) return new MockQuery<T>((List<T>)(object)SystemActivities);
            if (typeof(T) == typeof(UserAccounts)) return new MockQuery<T>((List<T>)(object)UserAccounts);
            if (typeof(T) == typeof(CertRequest)) return new MockQuery<T>((List<T>)(object)CertificateRequests);

            return new MockQuery<T>(new List<T>());
        }
    }
}
