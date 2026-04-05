using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;
using BarangayLangkiwaSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Template;

namespace BarangayLangkiwaSystem.Controllers
{
    public class CertificatesController : BaseController
    {
        private readonly BarangayDBContext _context;

        public CertificatesController(BarangayDBContext context)
        {
            _context = context;
        }

        private int GetLoggedInUserId()
        {
            // Returns logged-in user's ID from session
            return HttpContext.Session.GetInt32("UserId") ?? 0; // 0 if not logged in
        }

        public IActionResult Index(string filter = "All")
        {
            DateTime now = DateTime.Now;
            var certQuery = _context.CertificationLogs.AsQueryable();

            // Date Filter Logic
            switch (filter)
            {
                case "Today":
                    certQuery = certQuery.Where(r => r.date_issued.Date == now.Date);
                    break;
                case "Week":
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    certQuery = certQuery.Where(r => r.date_issued >= startOfWeek);
                    break;
                case "Month":
                    certQuery = certQuery.Where(r => r.date_issued.Month == now.Month && r.date_issued.Year == now.Year);
                    break;
                case "LastMonth":
                    var lastMonth = now.AddMonths(-1);
                    certQuery = certQuery.Where(r => r.date_issued.Month == lastMonth.Month && r.date_issued.Year == lastMonth.Year);
                    break;
            }

            var chartData = certQuery
                .Join(_context.CertificationTypes,
                      log => log.type_id,
                      type => type.TypeId,
                      (log, type) => new { log, type })
                .GroupBy(x => x.type.TypeName)
                .Select(g => new {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.ChartLabels = chartData.Select(x => x.TypeName).ToArray();
            ViewBag.ChartValues = chartData.Select(x => x.Count).ToArray();

            ViewBag.Filter = filter;

            ViewBag.Approved = certQuery.Count(r => r.status == "Approved");
            ViewBag.Released = certQuery.Count(r => r.status == "Approved");
            ViewBag.Total = certQuery.Count();
                
            return View(certQuery.OrderByDescending(r => r.date_issued).ToList());
        }

        public IActionResult ViewLog(string filter ="All", int? typeId = null, string searchString = "")
        {
            DateTime now = DateTime.Now;
            var logQuery = _context.CertificationLogs.AsQueryable();

            // 1. Date Filter Logic
            switch (filter)
            {
                case "Today": logQuery = logQuery.Where(r => r.date_issued.Date == now.Date); break;
                case "Week":
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    logQuery = logQuery.Where(r => r.date_issued >= startOfWeek);
                    break;
                case "Month": logQuery = logQuery.Where(r => r.date_issued.Month == now.Month && r.date_issued.Year == now.Year); break;
                case "LastMonth":
                    var lastMonth = now.AddMonths(-1);
                    logQuery = logQuery.Where(r => r.date_issued.Month == lastMonth.Month && r.date_issued.Year == lastMonth.Year); break;
            }

            // 2. Type Filter
            if (typeId.HasValue) logQuery = logQuery.Where(l => l.type_id == typeId);

            // 3. Join and Search Logic
            var viewModelQuery = logQuery
                .Join(_context.Residents, l => l.resident_id, r => r.ResidentId, (l, r) => new { l, r })
                .Join(_context.UserAccounts, combined => combined.l.issued_by, u => u.user_id, (combined, u) => new { combined, u })
                .Join(_context.CertificationTypes, c => c.combined.l.type_id, t => t.TypeId, (c, t) => new { c, t })
                .Select(x => new CertificateLogVM
                {
                    Log = x.c.combined.l,
                    ResidentName = x.c.combined.r.FirstName + " " + x.c.combined.r.LastName,
                    IssuedByName = x.c.u.staff_name,
                    TypeName = x.t.TypeName
                });

            // 4. Name Search (Resident or Staff)
            if (!string.IsNullOrEmpty(searchString))
            {
                viewModelQuery = viewModelQuery.Where(x =>
                    x.ResidentName.Contains(searchString) ||
                    x.IssuedByName.Contains(searchString));
            }

            ViewBag.Filter = filter;
            ViewBag.TypeId = typeId;
            ViewBag.SearchString = searchString;
            ViewBag.Types = _context.CertificationTypes.ToList();

            return View(viewModelQuery.OrderByDescending(x => x.Log.date_issued).ToList());
        }

        public IActionResult Create()
        {
            var certificateTypes = _context.CertificationTypes.ToList();

            // 2. Condition: If the table is empty, do not allow issuance
            if (certificateTypes == null || !certificateTypes.Any())
            {
                // Add a message to show the user why they were redirected
                TempData["ErrorMessage"] = "You cannot issue a certificate because no Certificate Types have been defined yet. Please add a Certificate Type first.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CertificateTypes = _context.CertificationTypes.ToList();

            AllData vmAllData = new AllData
            {
                Residents = _context.Residents.ToList(),
                CertificateRequests = _context.CertificateRequests.ToList(),
                
            };

            return View(vmAllData);
        }

        [HttpGet]
        public IActionResult SearchResidents(string term)
        {
            var residents = _context.Residents
                .Where(r => (r.FirstName + " " + r.LastName).Contains(term))
                .Select(r => new
                {
                    id = r.ResidentId,
                    label = r.LastName + ", " + r.FirstName,
                    value = r.FirstName + " " + r.LastName
                })
                .Take(10)
                .ToList();

            return Json(residents);
        }

        // --- UPDATED API FOR AUTOFILL ---
        [HttpGet]
        public IActionResult GetResidentDetails(int id)
        {
            // Fetches the resident from the database context
            var resident = _context.Residents.FirstOrDefault(r => r.ResidentId == id);

            if (resident == null) return NotFound();

            // Logic to calculate age accurately based on the Birthdate column
            int age = DateTime.Now.Year - resident.Birthdate.Year;
            if (resident.Birthdate > DateTime.Now.AddYears(-age)) age--;

            int daysofBeingaResident = (DateTime.Now - resident.DateRegistered).Days;

            // This JSON object matches what your JavaScript is looking for
            return Json(new
            {
                fullName = resident.FirstName + " " + resident.LastName,
                age = age,
                civilStatus = resident.CivilStatus,
                address = resident.Address,
                daysBeingResident = daysofBeingaResident
            });
        }
        // 1. Display the form
        [HttpGet]
        public IActionResult AddType()
        {
            return View();
        }

        // 2. Process the form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddType(CertificationType model)
        {
            if (ModelState.IsValid)
            {
                _context.CertificationTypes.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        public IActionResult Details(int ResidentId)
        {
            var resident = _context.Residents.FirstOrDefault(r => r.ResidentId == ResidentId);
            var vm = new Resident() { ResidentId = ResidentId };
            return View(vm);
        }


        [HttpGet]
        public IActionResult Register(CertificateIssueVM vm)
        {
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AllData vm)
        {
            var resident = _context.Residents.FirstOrDefault(r=>r.ResidentId == vm.ResidentId);
            if (resident == null) 
                return NotFound();

            // Calculate residency duration
            int residencyDuration = (DateTime.Now - resident.DateRegistered).Days;

            var preview = new CertificateIssueVM
            {
                ResidentId = vm.ResidentId,
                ResidentFullName = resident.FirstName + " " + resident.LastName,
                Age = resident.Age,
                CivilStatus = resident.CivilStatus,
                Address = resident.Address,
                CertificateTypeName = vm.CertificateTypeName,
                Purpose = vm.Purpose,
                DateIssued = DateTime.Now,
                IssuedById = GetLoggedInUserId(),
                IssuedByName = "Barangay Staff",
                ReferenceCode = Guid.NewGuid().ToString(),
                DaysofBeingaResident = residencyDuration
                
            };

            return View("Register", preview); // VERY IMPORTANT

        }

        [HttpPost]
        public IActionResult Approve(CertificateIssueVM vm)
        {
            var resident = _context.Residents
                .FirstOrDefault(r => r.ResidentId == vm.ResidentId);

            if (resident == null)
                return NotFound();

            var type = _context.CertificationTypes
                .First(t => t.TypeName == vm.CertificateTypeName);

            var log = new CertificationLog
            {
                resident_id = vm.ResidentId,
                type_id = _context.CertificationTypes
                    .First(t => t.TypeName == vm.CertificateTypeName).TypeId,
                purpose = vm.Purpose,
                date_issued = DateTime.Now,
                status = "Approved",
                issued_by = GetLoggedInUserId(),
                reference_code = Guid.NewGuid().ToString(),
                //approval_status = "Approved"
            };

            _context.CertificationLogs.Add(log);
            _context.SaveChanges();

            int daysofBeingaResident =
        (DateTime.Now - resident.DateRegistered).Days;

            string templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "templates",
                vm.CertificateTypeName + "Template.docx"
            );

            string generatedFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "generated"
            );

            if (!Directory.Exists(generatedFolder))
                Directory.CreateDirectory(generatedFolder);

            string fileName = $"{vm.CertificateTypeName}_{log.reference_code}.docx";
            string newFilePath = Path.Combine(generatedFolder, fileName);

            System.IO.File.Copy(templatePath, newFilePath, true);

            string staffName = HttpContext.Session.GetString("Username") ?? "Barangay Staff";
            string staffRole = HttpContext.Session.GetString("Role") ?? "Staff";

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(newFilePath, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;

                foreach (var para in body.Descendants<Paragraph>())
                {
                    var runs = para.Elements<Run>().ToList();
                    if (!runs.Any()) continue;

                    // Combine all text in the paragraph
                    string combinedText = string.Concat(runs.Select(r => string.Concat(r.Elements<Text>().Select(t => t.Text))));

                    // Check if paragraph contains any placeholder
                    if (combinedText.Contains("[Name]") || combinedText.Contains("[Purpose]") ||
                        combinedText.Contains("[Date]") || combinedText.Contains("[Address]") ||
                        combinedText.Contains("[StaffName]") || combinedText.Contains("[role]") ||
                        combinedText.Contains("[DaysofBeingaResident]"))
                    {
                        // Replace placeholders
                        combinedText = combinedText.Replace("[Name]", resident.FirstName + " " + resident.LastName)
                                                   .Replace("[Purpose]", vm.Purpose)
                                                   .Replace("[Date]", DateTime.Now.ToString("MMMM dd, yyyy"))
                                                   .Replace("[Address]", resident.Address)
                                                   .Replace("[StaffName]", staffName)
                                                   .Replace("[role]", staffRole)
                                                   .Replace("[DaysofBeingaResident]", 
                                                        daysofBeingaResident >= 365 ? $"{daysofBeingaResident / 365} year(s)" :
                                                        daysofBeingaResident >= 30 ? $"{daysofBeingaResident / 30} month(s)" :
                                                        daysofBeingaResident >= 7 ? $"{daysofBeingaResident / 7} week(s)" :
                                                        $"{daysofBeingaResident} day(s)");
                                            

                        // Remove old runs
                        para.RemoveAllChildren<Run>();

                        // Add a single new run with replaced text
                        Run newRun = new Run(new Text(combinedText));
                        para.Append(newRun);
                    }
                }

                wordDoc.MainDocumentPart.Document.Save();
            }

            // 3️⃣ Return File Immediately (Download)
            return File(
                System.IO.File.ReadAllBytes(newFilePath),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName
            );


        }


        [HttpPost]
        public IActionResult Disapprove(CertificateIssueVM vm)
        {
            var log = new CertificationLog
            {
                resident_id = vm.ResidentId,
                type_id = _context.CertificationTypes
                    .First(t => t.TypeName == vm.CertificateTypeName).TypeId,
                purpose = vm.Purpose,
                date_issued = DateTime.Now,
                status = "Disapproved",
                issued_by = GetLoggedInUserId(),
                reference_code = Guid.NewGuid().ToString(),
                //approval_status = "Disapproved"
            };

            _context.CertificationLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}