using System;
using System.Collections.Generic;
using System.Linq;
using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly BarangayDBContext _context;

        public ReportsController(BarangayDBContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            // Check if the session Role is BarangayCaptain or Secretary
            var role = HttpContext.Session.GetString("Role");

            return role == "Barangay Captain"
                || role == "Secretary";
        }

        private int GetLoggedInUserId()
        {
            // Returns logged-in user's ID from session
            return HttpContext.Session.GetInt32("UserId") ?? 0; // 0 if not logged in
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

        public IActionResult Index()
        {

            ViewBag.TotalReports = _context.ReportLogs.Count();
            //ViewBag.PendingReview = _context.CertificateRequests.Count(r => r.Status == "Pending");
            ViewBag.Completed = _context.CertificateRequests.Count(r => r.Status == "Completed");
            ViewBag.ThisMonth = _context.ReportLogs.Count(r => r.DateGenerated.Month == DateTime.Now.Month && r.DateGenerated.Year == DateTime.Now.Year);

            return View();
        }

        public IActionResult Generate()
        {
            return View();
        }

        public IActionResult ViewReport()
        {
            // We fetch all residents and group them by Purok
            var residents = _context.Residents.ToList();

            var reportData = residents
                .GroupBy(r => r.Purok)
                .Select(g => new PurokReportViewModel
                {
                    PurokName = g.Key ?? "General/Unassigned",
                    TotalResidents = g.Count(),
                    MaleCount = g.Count(r => r.Gender == "Male"),
                    FemaleCount = g.Count(r => r.Gender == "Female"),
                    TotalHousehold = g.Select(r => r.Address).Distinct().Count(),
                    SeniorCount = g.Count(r => r.Age >= 60),
                    RegisteredVoters = g.Count(r => r.VoterStatus == "Active"),
                    NewResidents = g.Count(r => r.DateRegistered >= DateTime.Now.AddDays(-30))
                })
                .OrderBy(r => r.PurokName)
                .ToList();

            return View(reportData);
        }



        [HttpPost]
        public IActionResult LogExport(string type, string format)
        {
            // FIX: Use the singular 'ReportLog' model name
            var log = new ReportLog
            {
                ReportType = type,
                ExportFormat = format,
                DateGenerated = DateTime.Now
            };
            _context.ReportLogs.Add(log);
            _context.SaveChanges();
            return Ok();
        }

        public IActionResult ActivityReport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.SystemActivities.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp.Date <= endDate.Value.Date);

            var activities = query.OrderByDescending(a => a.Timestamp).ToList();

            return View(activities);
        }

        public IActionResult CertificateIssuanceReport(DateTime? startDate, DateTime? endDate, string certType)
        {
            var query = from log in _context.CertificationLogs
                            // Connect to Resident table for the real name
                        join res in _context.Residents on log.resident_id equals res.ResidentId
                        // Connect to Type table for the category name
                        join type in _context.CertificationTypes on log.type_id equals type.TypeId
                        select new CertificationLog
                        {
                            date_issued = log.date_issued,
                            purpose = log.purpose,
                            status = log.status,
                            // Store the real names into the existing fields for display
                            // This avoids creating a new ViewModel
                            IssuedBy = res.FirstName + " " + res.LastName, // Temporarily use an available string field
                            ReferenceCode = type.TypeName // Temporarily use an available string field
                        };

            // Filter logic remains the same...
            var reportData = query.OrderByDescending(r => r.date_issued).ToList();
            return View(reportData);
        }

        // Inside ReportsController.cs
        #region Chart Data Helpers
        [HttpGet]
        public IActionResult GetChartData(string filter)
        {
            var period = string.IsNullOrEmpty(filter) ? "weekly" : filter.ToLower();

            // These calls are what triggered the CS0103 error
            object chartData = period switch
            {
                "monthly" => GetMonthlyData(),
                "quarterly" => GetQuarterlyData(),
                "annually" => GetAnnualData(),
                _ => GetWeeklyData()
            };

            return Json(chartData);
        }

        #region Unified Chart Data Logic

        private object GetWeeklyData()
        {
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Now.Date.AddDays(-i)).Reverse().ToList();
            return new
            {
                labels = last7Days.Select(d => d.ToString("ddd")),
                total = last7Days.Select(d => _context.ReportLogs.Count(r => r.DateGenerated.Date == d)),
                pending = last7Days.Select(d => _context.CertificateRequests.Count(r => r.RequestDate.Date == d && r.Status == "Pending")),
                recentItems = GetRecentItems(DateTime.Now.AddDays(-7))
            };
        }

        private object GetMonthlyData()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Create 4 week-long ranges for the current month
            var weeks = Enumerable.Range(0, 4).Select(i => new {
                Start = startOfMonth.AddDays(i * 7),
                End = i == 3 ? startOfMonth.AddMonths(1).AddDays(-1) : startOfMonth.AddDays((i * 7) + 6)
            }).ToList();

            return new
            {
                labels = new[] { "Week 1", "Week 2", "Week 3", "Week 4" },
                // Updated: Using ReportLogs for Total and CertificateRequests for Pending to match your Weekly logic
                total = weeks.Select(w => _context.ReportLogs.Count(r => r.DateGenerated >= w.Start && r.DateGenerated <= w.End)).ToArray(),
                pending = weeks.Select(w => _context.CertificateRequests.Count(r => r.RequestDate >= w.Start && r.RequestDate <= w.End && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(startOfMonth)
            };
        }

        private object GetQuarterlyData()
        {
            var year = DateTime.Now.Year;
            var quarters = new[] {
        new { Name = "Q1", Start = new DateTime(year, 1, 1), End = new DateTime(year, 3, 31) },
        new { Name = "Q2", Start = new DateTime(year, 4, 1), End = new DateTime(year, 6, 30) },
        new { Name = "Q3", Start = new DateTime(year, 7, 1), End = new DateTime(year, 9, 30) },
        new { Name = "Q4", Start = new DateTime(year, 10, 1), End = new DateTime(year, 12, 31) }
    };

            return new
            {
                labels = quarters.Select(q => q.Name).ToArray(),
                total = quarters.Select(q => _context.ReportLogs.Count(r => r.DateGenerated >= q.Start && r.DateGenerated <= q.End)).ToArray(),
                pending = quarters.Select(q => _context.CertificateRequests.Count(r => r.RequestDate >= q.Start && r.RequestDate <= q.End && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(DateTime.Now.AddMonths(-3))
            };
        }

        private object GetAnnualData()
        {
            var currentYear = DateTime.Now.Year;
            var years = new[] { currentYear - 2, currentYear - 1, currentYear };

            return new
            {
                labels = years.Select(y => y.ToString()).ToArray(),
                total = years.Select(y => _context.ReportLogs.Count(r => r.DateGenerated.Year == y)).ToArray(),
                pending = years.Select(y => _context.CertificateRequests.Count(r => r.RequestDate.Year == y && r.Status == "Pending")).ToArray(),
                recentItems = GetRecentItems(DateTime.Now.AddYears(-1))
            };
        }

        #endregion
        private object GetRecentItems(DateTime since)
        {
            return _context.CertificateRequests
                .Where(r => r.RequestDate >= since)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .Select(r => new { type = r.CertificateType, residentName = r.FirstName + " " + r.LastName, status = r.Status })
                .ToList();
        }
        #endregion
    }
}
