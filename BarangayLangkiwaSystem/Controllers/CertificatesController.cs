using BarangayLangkiwaSystem.Models;
using BarangayLangkiwaSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayLangkiwaSystem.Controllers
{
    public class CertificatesController : BaseController
    {
        private readonly Supabase.Client _client;

        public CertificatesController(Supabase.Client client)
        {
            _client = client;
        }

        private int GetLoggedInUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public async Task<IActionResult> Index(string filter = "All")
        {
            DateTime now = DateTime.Now;
            var certLogsResp = await _client.From<CertificationLog>().Get();
            var certTypesResp = await _client.From<CertificationType>().Get();

            var certLogs = certLogsResp.Models;
            var certTypes = certTypesResp.Models;

            IEnumerable<CertificationLog> certQuery = certLogs;

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

            var filteredLogs = certQuery.ToList();

            var chartData = filteredLogs
                .Join(certTypes,
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

            ViewBag.Approved = filteredLogs.Count(r => r.status == "Approved");
            ViewBag.Released = filteredLogs.Count(r => r.status == "Approved");
            ViewBag.Total = filteredLogs.Count;
                
            return View(filteredLogs.OrderByDescending(r => r.date_issued).ToList());
        }

        public async Task<IActionResult> ViewLog(string filter = "All", int? typeId = null, string searchString = "")
        {
            DateTime now = DateTime.Now;

            var certLogsResp = await _client.From<CertificationLog>().Get();
            var residentsResp = await _client.From<Resident>().Get();
            var usersResp = await _client.From<UserAccounts>().Get();
            var certTypesResp = await _client.From<CertificationType>().Get();

            var certLogs = certLogsResp.Models;
            var residents = residentsResp.Models;
            var users = usersResp.Models;
            var certTypes = certTypesResp.Models;

            IEnumerable<CertificationLog> logQuery = certLogs;

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

            if (typeId.HasValue) logQuery = logQuery.Where(l => l.type_id == typeId);

            var viewModelQuery = logQuery
                .Join(residents, l => l.resident_id, r => r.ResidentId, (l, r) => new { l, r })
                .Join(users, combined => combined.l.issued_by, u => u.user_id, (combined, u) => new { combined, u })
                .Join(certTypes, c => c.combined.l.type_id, t => t.TypeId, (c, t) => new { c, t })
                .Select(x => new CertificateLogVM
                {
                    Log = x.c.combined.l,
                    ResidentName = x.c.combined.r.FirstName + " " + x.c.combined.r.LastName,
                    IssuedByName = x.c.u.staff_name,
                    TypeName = x.t.TypeName
                });

            if (!string.IsNullOrEmpty(searchString))
            {
                viewModelQuery = viewModelQuery.Where(x =>
                    (x.ResidentName != null && x.ResidentName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (x.IssuedByName != null && x.IssuedByName.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            ViewBag.Filter = filter;
            ViewBag.TypeId = typeId;
            ViewBag.SearchString = searchString;
            ViewBag.Types = certTypes;

            return View(viewModelQuery.OrderByDescending(x => x.Log.date_issued).ToList());
        }

        public async Task<IActionResult> Create()
        {
            var certTypesResp = await _client.From<CertificationType>().Get();
            var certificateTypes = certTypesResp.Models;

            if (certificateTypes == null || !certificateTypes.Any())
            {
                TempData["ErrorMessage"] = "You cannot issue a certificate because no Certificate Types have been defined yet. Please add a Certificate Type first.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CertificateTypes = certificateTypes;

            var residentsResp = await _client.From<Resident>().Get();
            var certRequestsResp = await _client.From<CertRequest>().Get();

            AllData vmAllData = new AllData
            {
                Residents = residentsResp.Models,
                CertificateRequests = certRequestsResp.Models,
            };

            return View(vmAllData);
        }

        [HttpGet]
        public async Task<IActionResult> SearchResidents(string term)
        {
            var residentsResp = await _client.From<Resident>().Get();
            var residents = residentsResp.Models
                .Where(r => (r.FirstName + " " + r.LastName).Contains(term, StringComparison.OrdinalIgnoreCase))
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

        [HttpGet]
        public async Task<IActionResult> GetResidentDetails(int id)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
            var resident = response.Models.FirstOrDefault();

            if (resident == null) return NotFound();

            int age = DateTime.Now.Year - resident.Birthdate.Year;
            if (resident.Birthdate > DateTime.Now.AddYears(-age)) age--;

            int daysofBeingaResident = (DateTime.Now - resident.DateRegistered).Days;

            return Json(new
            {
                fullName = resident.FirstName + " " + resident.LastName,
                age = age,
                civilStatus = resident.CivilStatus,
                address = resident.Address,
                daysBeingResident = daysofBeingaResident
            });
        }

        [HttpGet]
        public IActionResult AddType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddType(CertificationType model)
        {
            if (ModelState.IsValid)
            {
                await _client.From<CertificationType>().Insert(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int ResidentId)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == ResidentId).Get();
            var resident = response.Models.FirstOrDefault();
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
        public async Task<IActionResult> Register(AllData vm)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == vm.ResidentId).Get();
            var resident = response.Models.FirstOrDefault();
            if (resident == null) 
                return NotFound();

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

            return View("Register", preview);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(CertificateIssueVM vm)
        {
            var resResp = await _client.From<Resident>().Where(r => r.ResidentId == vm.ResidentId).Get();
            var resident = resResp.Models.FirstOrDefault();

            if (resident == null)
                return NotFound();

            var typeResp = await _client.From<CertificationType>().Where(t => t.TypeName == vm.CertificateTypeName).Get();
            var type = typeResp.Models.FirstOrDefault();

            if (type == null)
                return NotFound();

            var log = new CertificationLog
            {
                resident_id = vm.ResidentId,
                type_id = type.TypeId,
                purpose = vm.Purpose,
                date_issued = DateTime.Now,
                status = "Approved",
                issued_by = GetLoggedInUserId(),
                reference_code = Guid.NewGuid().ToString()
            };

            await _client.From<CertificationLog>().Insert(log);

            int daysofBeingaResident = (DateTime.Now - resident.DateRegistered).Days;

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
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var para in body.Descendants<Paragraph>())
                    {
                        var runs = para.Elements<Run>().ToList();
                        if (!runs.Any()) continue;

                        string combinedText = string.Concat(runs.Select(r => string.Concat(r.Elements<Text>().Select(t => t.Text))));

                        if (combinedText.Contains("[Name]") || combinedText.Contains("[Purpose]") ||
                            combinedText.Contains("[Date]") || combinedText.Contains("[Address]") ||
                            combinedText.Contains("[StaffName]") || combinedText.Contains("[role]") ||
                            combinedText.Contains("[DaysofBeingaResident]"))
                        {
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

                            para.RemoveAllChildren<Run>();

                            Run newRun = new Run(new Text(combinedText));
                            para.Append(newRun);
                        }
                    }

                    wordDoc.MainDocumentPart.Document.Save();
                }
            }

            return File(
                System.IO.File.ReadAllBytes(newFilePath),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName
            );
        }

        [HttpPost]
        public async Task<IActionResult> Disapprove(CertificateIssueVM vm)
        {
            var typeResp = await _client.From<CertificationType>().Where(t => t.TypeName == vm.CertificateTypeName).Get();
            var type = typeResp.Models.FirstOrDefault();

            if (type == null)
                return NotFound();

            var log = new CertificationLog
            {
                resident_id = vm.ResidentId,
                type_id = type.TypeId,
                purpose = vm.Purpose,
                date_issued = DateTime.Now,
                status = "Disapproved",
                issued_by = GetLoggedInUserId(),
                reference_code = Guid.NewGuid().ToString()
            };

            await _client.From<CertificationLog>().Insert(log);

            return RedirectToAction("Index");
        }
    }
}
