using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayLangkiwaSystem.Controllers
{
    [AllowAnonymous]
    public class PortalController : Controller
    {
        public PortalController()
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var typesResp = await InMemoryDataStore.From<CertificationType>().Get();
            ViewBag.CertificateTypes = typesResp.Models;
            return View(new CertRequest());
        }

        [HttpGet]
        public async Task<IActionResult> SearchResidents(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new object[] { });
            }

            var residentsResp = await InMemoryDataStore.From<Resident>().Get();
            var matches = residentsResp.Models
                .Where(r => $"{r.FirstName} {r.LastName}".Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            r.ResidentId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(r => new
                {
                    id = r.ResidentId,
                    label = $"{r.LastName}, {r.FirstName} (ID: R-2025-00{r.ResidentId})",
                    value = $"{r.FirstName} {r.LastName}",
                    fullName = $"{r.FirstName} {r.LastName}",
                    firstName = r.FirstName,
                    lastName = r.LastName,
                    purok = r.Purok,
                    address = r.Address
                })
                .Take(10)
                .ToList();

            return Json(matches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRequest(CertRequest model)
        {
            // If ResidentId is provided, enrich Resident details
            Resident? resident = null;
            if (model.ResidentId > 0)
            {
                var residentResp = await InMemoryDataStore.From<Resident>().Where(r => r.ResidentId == model.ResidentId).Get();
                resident = residentResp.Models.FirstOrDefault();
                if (resident != null)
                {
                    model.FirstName = resident.FirstName;
                    model.LastName = resident.LastName;
                    model.ResidentName = $"{resident.FirstName} {resident.LastName}";
                }
            }

            if (model.ResidentId <= 0 || resident == null)
            {
                ModelState.AddModelError("ResidentId", "Please select a valid registered resident.");
            }

            if (string.IsNullOrWhiteSpace(model.CertificateType))
            {
                ModelState.AddModelError("CertificateType", "Please choose a certificate type.");
            }

            if (string.IsNullOrWhiteSpace(model.Purpose))
            {
                ModelState.AddModelError("Purpose", "Please state the purpose for this certification.");
            }

            if (!ModelState.IsValid)
            {
                var typesResp = await InMemoryDataStore.From<CertificationType>().Get();
                ViewBag.CertificateTypes = typesResp.Models;
                return View("Index", model);
            }

            var allReqs = await InMemoryDataStore.From<CertRequest>().Get();
            int nextId = allReqs.Models.Any() ? allReqs.Models.Max(r => r.RequestId) + 1 : 1;

            model.RequestId = nextId;
            model.RequestDate = DateTime.Now;
            model.Status = "Pending";

            await InMemoryDataStore.From<CertRequest>().Insert(model);

            // Also record a system activity for admin visibility
            try
            {
                var activity = new SystemActivity
                {
                    Action = $"Online request submitted: {model.CertificateType} for {model.ResidentName}",
                    UserRole = "Resident (Portal)",
                    Icon = "bi-envelope-paper text-primary",
                    Timestamp = DateTime.Now
                };
                await InMemoryDataStore.From<SystemActivity>().Insert(activity);
            }
            catch { /* non-critical */ }

            TempData["Success"] = $"Your request for {model.CertificateType} has been submitted successfully (Reference: REQ-{model.RequestId:D4})! You can follow up with the Barangay office.";
            return RedirectToAction(nameof(Confirmation), new { id = model.RequestId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var reqResp = await InMemoryDataStore.From<CertRequest>().Where(r => r.RequestId == id).Get();
            var request = reqResp.Models.FirstOrDefault();
            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }
    }
}
