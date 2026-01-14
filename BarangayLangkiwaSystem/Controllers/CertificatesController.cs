using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using System.Collections.Generic;

namespace BarangayLangkiwaSystem.Controllers
{
    public class CertificatesController : Controller
    {
        // GET: Certificates/Index (Dashboard)
        public IActionResult Index()
        {
            ViewBag.Pending = 3;
            ViewBag.Approved = 2;
            ViewBag.Released = 3;
            ViewBag.Total = 8;

            var requests = new List<CertificateRequest>
            {
                new CertificateRequest { RequestId = "CR-001", ResidentName = "Juan Dela Cruz", CertificateType = "Certificate of Indigency", RequestDate = new DateTime(2025, 11, 1), Purpose = "Medical Assistance", Status = "Pending" },
                new CertificateRequest { RequestId = "CR-004", ResidentName = "Ana Garcia", CertificateType = "Certificate of Indigency", RequestDate = new DateTime(2025, 11, 2), Purpose = "School Requirements", Status = "Pending" },
                new CertificateRequest { RequestId = "CR-007", ResidentName = "Ricardo Flores", CertificateType = "Certificate of Indigency", RequestDate = new DateTime(2025, 11, 3), Purpose = "Medical Assistance", Status = "Pending" }
            };

            return View(requests);
        }

        // GET: Certificates/Create (Issue Form)
        public IActionResult Create()
        {
            return View();
        }

        // GET: Certificates/Details (Certificate View Page)
        public IActionResult Details(string id)
        {
            return View();
        }
    }
}