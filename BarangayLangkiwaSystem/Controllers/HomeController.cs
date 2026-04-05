using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarangayLangkiwaSystem.Controllers
{
    public class HomeController : BaseController
    {
        private readonly BarangayDBContext _context;

        public HomeController(BarangayDBContext context)
        {
            _context = context;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Index(string filter = "Total")
        {
            DateTime now = DateTime.Now;
            DateTime? startDate = null;

            switch (filter)
            {
                case "Today":
                    startDate = now.Date;
                    break;
                case "Week":
                    // Get the start of the current week (Sunday)
                    startDate = now.AddDays(-(int)now.DayOfWeek).Date;
                    break;
                case "Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
                case "LastMonth":
                    var lastMonth = now.AddMonths(-1);
                    startDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    var endDate = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                    // Specific logic for LastMonth might require an end date, 
                    // but for simplicity, we'll filter from start of last month to now 
                    // OR you can add an EndDate check in the LINQ queries below.
                    break;
                default: // "Total" or any other value
                    startDate = null;
                    break;
            }

            var residentsQuery = _context.Residents.AsQueryable();
            var certLogQuery = _context.CertificationLogs.AsQueryable();
            var reportLogQuery = _context.ReportLogs.AsQueryable();

            if (startDate.HasValue)
            {
                // For LastMonth, we specifically want to stop at the start of this month
                if (filter == "LastMonth")
                {
                    var endOfLastMonth = new DateTime(now.Year, now.Month, 1);
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered >= startDate && r.DateRegistered < endOfLastMonth);
                    certLogQuery = certLogQuery.Where(c => c.date_issued >= startDate && c.date_issued < endOfLastMonth);
                    reportLogQuery = reportLogQuery.Where(r => r.DateGenerated >= startDate && r.DateGenerated < endOfLastMonth);
                }
                else
                {
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered >= startDate);
                    certLogQuery = certLogQuery.Where(c => c.date_issued >= startDate);
                    reportLogQuery = reportLogQuery.Where(r => r.DateGenerated >= startDate);
                }
            }

            var residents = residentsQuery.ToList();
            var filteredCertLogs = certLogQuery.ToList();

            //var residents = _context.Residents.ToList();
            //var certRequests = _context.CertificateRequests.ToList();
            //var certLog = _context.CertificationLogs.ToList();

            // 2. Fetch REAL Activities from the Database Table
            var realActivities = _context.SystemActivities
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .ToList()
                .Select(a => new ActivityLog
                {
                    Action = a.Action,
                    UserRole = a.UserRole,
                    Icon = a.Icon,
                    TimeAgo = GetTimeAgo(a.Timestamp)
                })
                .ToList();

            // 3. Prepare the ViewModel and integrate the ReportLogs count
            var model = new DashboardViewModel
            {
                TotalResidents = residents.Count,
                CertificatesIssued = filteredCertLogs.Count(c => c.status == "Approved"),
                //PendingRequests = certRequests.Count(c => c.Status == "Pending"),

                // FIX: Instead of hardcoded 14, we count from the new ReportLogs table
                ReportsGenerated = reportLogQuery.Count(),

                MaleCount = residents.Count(r => r.Gender == "Male"),
                FemaleCount = residents.Count(r => r.Gender == "Female"),

                AgeDistributions = new List<int>
                {
                    residents.Count(r => r.Age <= 17),
                    residents.Count(r => r.Age >= 18 && r.Age <= 35),
                    residents.Count(r => r.Age >= 36 && r.Age <= 50),
                    residents.Count(r => r.Age >= 51 && r.Age <= 65),
                    residents.Count(r => r.Age >= 66)
                },

                RecentActivities = realActivities
            };

            ViewBag.CurrentFilter = filter;
            return View(model);
        }

        // HELPER METHOD: Converts a DateTime into a friendly "Time Ago" string
        private string GetTimeAgo(DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} mins ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";

            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}