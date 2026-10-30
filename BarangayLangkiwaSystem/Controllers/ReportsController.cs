using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly Supabase.Client _client;

        public ReportsController(Supabase.Client client)
        {
            _client = client;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");

            return role == "Barangay Captain"
                || role == "Secretary";
        }

        private int GetLoggedInUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (!IsAdmin())
            {
                context.Result = RedirectToAction("AccessDenied", "Home");
                return;
            }

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var reportsResp = await _client.From<ReportLog>().Get();
            var certRequestsResp = await _client.From<CertRequest>().Get();

            var reportLogs = reportsResp.Models;
            var certRequests = certRequestsResp.Models;

            ViewBag.TotalReports = reportLogs.Count;
            ViewBag.Completed = certRequests.Count(r => r.Status == "Completed");
            ViewBag.ThisMonth = reportLogs.Count(r => r.DateGenerated.Month == DateTime.Now.Month && r.DateGenerated.Year == DateTime.Now.Year);

            return View();
        }

        public IActionResult Generate()
        {
            return View();
        }

        public async Task<IActionResult> ViewReport()
        {
            var response = await _client.From<Resident>().Get();
            var residents = response.Models;

            var reportData = residents
                .GroupBy(r => r.Purok)
                .Select(g => new PurokReportViewModel
                {
                    PurokName = string.IsNullOrEmpty(g.Key) ? "General/Unassigned" : g.Key,
                    TotalResidents = g.Count(),
                    MaleCount = g.Count(r => r.Gender == "Male"),
                    FemaleCount = g.Count(r => r.Gender == "Female"),
                    TotalHousehold = g.Select(r => r.Address).Where(a => !string.IsNullOrEmpty(a)).Distinct().Count(),
                    SeniorCount = g.Count(r => r.Age >= 60),
                    RegisteredVoters = g.Count(r => r.VoterStatus == "Active"),
                    NewResidents = g.Count(r => r.DateRegistered >= DateTime.Now.AddDays(-30))
                })
                .OrderBy(r => r.PurokName)
                .ToList();

            return View(reportData);
        }

        [HttpPost]
        public async Task<IActionResult> LogExport(string type, string format)
        {
            var log = new ReportLog
            {
                ReportType = type,
                ExportFormat = format,
                DateGenerated = DateTime.Now
            };
            await _client.From<ReportLog>().Insert(log);
            return Ok();
        }

        public async Task<IActionResult> ActivityReport(DateTime? startDate, DateTime? endDate)
        {
            var response = await _client.From<SystemActivity>().Get();
            IEnumerable<SystemActivity> query = response.Models;

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp.Date <= endDate.Value.Date);

            var activities = query.OrderByDescending(a => a.Timestamp).ToList();

            return View(activities);
        }

        public async Task<IActionResult> CertificateIssuanceReport(DateTime? startDate, DateTime? endDate, string certType)
        {
            var certLogsResp = await _client.From<CertificationLog>().Get();
            var residentsResp = await _client.From<Resident>().Get();
            var certTypesResp = await _client.From<CertificationType>().Get();

            var certLogs = certLogsResp.Models;
            var residents = residentsResp.Models;
            var certTypes = certTypesResp.Models;

            var query = from log in certLogs
                        join res in residents on log.resident_id equals res.ResidentId
                        join type in certTypes on log.type_id equals type.TypeId
                        select new CertificationLog
                        {
                            date_issued = log.date_issued,
                            purpose = log.purpose,
                            status = log.status,
                            IssuedBy = res.FirstName + " " + res.LastName,
                            ReferenceCode = type.TypeName
                        };

            if (startDate.HasValue)
                query = query.Where(c => c.date_issued.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(c => c.date_issued.Date <= endDate.Value.Date);

            if (!string.IsNullOrEmpty(certType) && certType != "All")
                query = query.Where(c => c.ReferenceCode == certType);

            var reportData = query.OrderByDescending(r => r.date_issued).ToList();
            return View(reportData);
        }

        #region Chart Data Helpers
        [HttpGet]
        public async Task<IActionResult> GetChartData(string filter)
        {
            var period = string.IsNullOrEmpty(filter) ? "weekly" : filter.ToLower();

            object chartData = period switch
            {
                "monthly" => await GetMonthlyData(),
                "quarterly" => await GetQuarterlyData(),
                "annually" => await GetAnnualData(),
                _ => await GetWeeklyData()
            };

            return Json(chartData);
        }

        #region Unified Chart Data Logic

        private async Task<object> GetWeeklyData()
        {
            var reportsResp = await _client.From<ReportLog>().Get();
            var certReqResp = await _client.From<CertRequest>().Get();

            var reports = reportsResp.Models;
            var certRequests = certReqResp.Models;

            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Now.Date.AddDays(-i)).Reverse().ToList();
            return new
            {
                labels = last7Days.Select(d => d.ToString("ddd")),
                total = last7Days.Select(d => reports.Count(r => r.DateGenerated.Date == d)),
                pending = last7Days.Select(d => certRequests.Count(r => r.RequestDate.Date == d && r.Status == "Pending")),
                recentItems = GetRecentItems(certRequests, DateTime.Now.AddDays(-7))
            };
        }

        private async Task<object> GetMonthlyData()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var reportsResp = await _client.From<ReportLog>().Get();
            var certReqResp = await _client.From<CertRequest>().Get();

            var reports = reportsResp.Models;
            var certRequests = certReqResp.Models;

            var weeks = Enumerable.Range(0, 4).Select(i => new {
                Start = startOfMonth.AddDays(i * 7),
                End = i == 3 ? startOfMonth.AddMonths(1).AddDays(-1) : startOfMonth.AddDays((i * 7) + 6)
            }).ToList();

            return new
            {
                labels = new[] { "Week 1", "Week 2", "Week 3", "Week 4" },
                total = weeks.Select(w => reports.Count(r => r.DateGenerated >= w.Start && r.DateGenerated <= w.End)).ToArray(),
                pending = weeks.Select(w => certRequests.Count(r => r.RequestDate >= w.Start && r.RequestDate <= w.End && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(certRequests, startOfMonth)
            };
        }

        private async Task<object> GetQuarterlyData()
        {
            var year = DateTime.Now.Year;
            var quarters = new[] {
                new { Name = "Q1", Start = new DateTime(year, 1, 1), End = new DateTime(year, 3, 31) },
                new { Name = "Q2", Start = new DateTime(year, 4, 1), End = new DateTime(year, 6, 30) },
                new { Name = "Q3", Start = new DateTime(year, 7, 1), End = new DateTime(year, 9, 30) },
                new { Name = "Q4", Start = new DateTime(year, 10, 1), End = new DateTime(year, 12, 31) }
            };

            var reportsResp = await _client.From<ReportLog>().Get();
            var certReqResp = await _client.From<CertRequest>().Get();

            var reports = reportsResp.Models;
            var certRequests = certReqResp.Models;

            return new
            {
                labels = quarters.Select(q => q.Name).ToArray(),
                total = quarters.Select(q => reports.Count(r => r.DateGenerated >= q.Start && r.DateGenerated <= q.End)).ToArray(),
                pending = quarters.Select(q => certRequests.Count(r => r.RequestDate >= q.Start && r.RequestDate <= q.End && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(certRequests, DateTime.Now.AddMonths(-3))
            };
        }

        private async Task<object> GetAnnualData()
        {
            var currentYear = DateTime.Now.Year;
            var years = new[] { currentYear - 2, currentYear - 1, currentYear };

            var reportsResp = await _client.From<ReportLog>().Get();
            var certReqResp = await _client.From<CertRequest>().Get();

            var reports = reportsResp.Models;
            var certRequests = certReqResp.Models;

            return new
            {
                labels = years.Select(y => y.ToString()).ToArray(),
                total = years.Select(y => reports.Count(r => r.DateGenerated.Year == y)).ToArray(),
                pending = years.Select(y => certRequests.Count(r => r.RequestDate.Year == y && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(certRequests, DateTime.Now.AddYears(-1))
            };
        }

        #endregion

        private object GetRecentItems(List<CertRequest> requests, DateTime since)
        {
            return requests
                .Where(r => r.RequestDate >= since)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .Select(r => new { type = r.CertificateType, residentName = r.FirstName + " " + r.LastName, status = r.Status })
                .ToList();
        }
        #endregion
    }
}
