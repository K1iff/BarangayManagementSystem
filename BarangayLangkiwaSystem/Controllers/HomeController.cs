using Microsoft.AspNetCore.Mvc;

namespace BarangayLangkiwaSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Data taken from Project Report Figure 10: Staff Dashboard Statistics
            ViewBag.TotalResidents = "39,526";
            ViewBag.CertificatesIssued = "300";
            ViewBag.PendingRequests = "42";
            ViewBag.ReportsGenerated = "14";

            return View();
        }
    }
}