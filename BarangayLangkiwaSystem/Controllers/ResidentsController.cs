using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ResidentsController : BaseController
    {
        private readonly Supabase.Client _client;

        public ResidentsController(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<IActionResult> Index(string searchString, string filter = "All")
        {
            DateTime now = DateTime.Now;
            var response = await _client.From<Resident>().Get();
            var residentsList = response.Models;

            IEnumerable<Resident> residentsQuery = residentsList;

            // 1. Search Logic
            if (!string.IsNullOrEmpty(searchString))
            {
                residentsQuery = residentsQuery.Where(r =>
                    (r.FirstName != null && r.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (r.LastName != null && r.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            // 2. Date Filter Logic
            switch (filter)
            {
                case "Today":
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered.Date == now.Date);
                    break;
                case "Week":
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered >= startOfWeek);
                    break;
                case "Month":
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered.Month == now.Month && r.DateRegistered.Year == now.Year);
                    break;
                case "LastMonth":
                    var lastMonth = now.AddMonths(-1);
                    residentsQuery = residentsQuery.Where(r => r.DateRegistered.Month == lastMonth.Month && r.DateRegistered.Year == lastMonth.Year);
                    break;
            }

            ViewBag.Filter = filter;
            ViewBag.SearchString = searchString;

            var residents = residentsQuery.ToList();

            ViewBag.TotalRegistered = residents.Count;
            ViewBag.ActiveVoters = residents.Count(r => r.Age >= 18);
            ViewBag.NewResidents = residents.Count(r => r.DateRegistered >= DateTime.Today.AddMonths(-1));
            ViewBag.TotalHouseholds = residents.Select(r => r.Address).Where(a => !string.IsNullOrEmpty(a)).Distinct().Count();

            return View(residents);
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Resident resident)
        {
            if (ModelState.IsValid)
            {
                resident.DateRegistered = DateTime.Now;
                await _client.From<Resident>().Insert(resident);

                await LogActivity($"New resident registered: {resident.FirstName} {resident.LastName}", "bi-person-plus text-success");

                return RedirectToAction(nameof(Index));
            }
            return View(resident);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
            var resident = response.Models.FirstOrDefault();
            if (resident == null) return NotFound();
            return View(resident);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
            var resident = response.Models.FirstOrDefault();
            if (resident != null)
            {
                string fullName = $"{resident.FirstName} {resident.LastName}";
                await _client.From<Resident>().Where(r => r.ResidentId == id).Delete();

                await LogActivity($"Resident record deleted: {fullName}", "bi-trash text-danger");
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> View(int id)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
            var resident = response.Models.FirstOrDefault();
            if (resident == null) return NotFound();
            return View("ViewResident", resident);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
            var resident = response.Models.FirstOrDefault();
            if (resident == null) return NotFound();
            return View("EditResident", resident);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resident resident)
        {
            if (id != resident.ResidentId) 
                return BadRequest();

            if (ModelState.IsValid)
            {
                var response = await _client.From<Resident>().Where(r => r.ResidentId == id).Get();
                var existingResident = response.Models.FirstOrDefault();

                if (existingResident == null)
                    return NotFound();

                existingResident.FirstName = resident.FirstName;
                existingResident.MiddleName = resident.MiddleName;
                existingResident.LastName = resident.LastName;
                existingResident.Birthdate = resident.Birthdate;
                existingResident.Gender = resident.Gender;
                existingResident.CivilStatus = resident.CivilStatus;
                existingResident.Address = resident.Address;
                existingResident.ContactNo = resident.ContactNo;
                existingResident.IsMigrant = resident.IsMigrant;
                existingResident.Email = resident.Email;
                existingResident.Purok = resident.Purok;
                existingResident.Suffix = resident.Suffix;
                existingResident.Occupation = resident.Occupation;
                existingResident.EmergencyContactName = resident.EmergencyContactName;
                existingResident.EmergencyContactNo = resident.EmergencyContactNo;

                await existingResident.Update<Resident>();

                await LogActivity($"Resident record updated: {resident.FirstName} {resident.LastName}", "bi-pencil-square text-primary");

                return RedirectToAction(nameof(Index));
            }
            return View("EditResident", resident);
        }

        private async Task LogActivity(string action, string icon)
        {
            try
            {
                var role = HttpContext.Session.GetString("Role") ?? "Unknown";  

                var activity = new SystemActivity
                {
                    Action = action,
                    UserRole = role,
                    Icon = icon,
                    Timestamp = DateTime.Now
                };
                await _client.From<SystemActivity>().Insert(activity);
            }
            catch (Exception) { /* Fail silently to prevent app crash */ }
        }
    }
}
