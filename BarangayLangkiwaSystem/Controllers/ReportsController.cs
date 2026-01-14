using Microsoft.AspNetCore.Mvc;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Reports/Index (Statistics Dashboard)
        public IActionResult Index()
        {
            ViewBag.TotalReports = 120;
            ViewBag.PendingReview = 26;
            ViewBag.Completed = 94;
            ViewBag.ThisMonth = 20;
            return View();
        }

        // GET: Reports/Generate (Report Page Form)
        public IActionResult Generate()
        {
            return View();
        }

        // GET: Reports/ViewReport (Generated Report Page)
        public IActionResult ViewReport()
        {
            return View();
        }
    }
}