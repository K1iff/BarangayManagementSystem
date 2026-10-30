using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayLangkiwaSystem.Controllers
{
    public class HomeController : BaseController
    {
        private readonly Supabase.Client _client;

        public HomeController(Supabase.Client client)
        {
            _client = client;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Index(string filter = "Total")
        {
            DateTime now = DateTime.Now;
            DateTime? startDate = null;

            switch (filter)
            {
                case "Today":
                    startDate = now.Date;
                    break;
                case "Week":
                    startDate = now.AddDays(-(int)now.DayOfWeek).Date;
                    break;
                case "Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
                case "LastMonth":
                    var lastMonth = now.AddMonths(-1);
                    startDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    break;
                default:
                    startDate = null;
                    break;
            }

            var residentsResp = await _client.From<Resident>().Get();
            var certLogsResp = await _client.From<CertificationLog>().Get();
            var reportLogsResp = await _client.From<ReportLog>().Get();
            var activitiesResp = await _client.From<SystemActivity>().Get();

            IEnumerable<Resident> residentsQuery = residentsResp.Models;
            IEnumerable<CertificationLog> certLogQuery = certLogsResp.Models;
            IEnumerable<ReportLog> reportLogQuery = reportLogsResp.Models;

            if (startDate.HasValue)
            {
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

            var realActivities = activitiesResp.Models
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .Select(a => new ActivityLog
                {
                    Action = a.Action,
                    UserRole = a.UserRole,
                    Icon = a.Icon,
                    TimeAgo = GetTimeAgo(a.Timestamp)
                })
                .ToList();

            var model = new DashboardViewModel
            {
                TotalResidents = residents.Count,
                CertificatesIssued = filteredCertLogs.Count(c => c.status == "Approved"),
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
