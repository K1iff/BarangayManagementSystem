using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ResidentsController : BaseController
    {
        private readonly BarangayDBContext _context;

        public ResidentsController(BarangayDBContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, string filter = "All")
        {
            DateTime now = DateTime.Now;
            var residentsQuery = _context.Residents.AsQueryable();

            // 1. Search Logic
            if (!string.IsNullOrEmpty(searchString))
            {
                residentsQuery = residentsQuery.Where(r =>
                    r.FirstName.Contains(searchString) ||
                    r.LastName.Contains(searchString));
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
            ViewBag.TotalHouseholds = residents.Select(r => r.Address).Distinct().Count();

            return View(residents);
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Resident resident)
        {
            if (ModelState.IsValid)
            {
                resident.DateRegistered = DateTime.Now;
                _context.Residents.Add(resident);
                _context.SaveChanges();

                LogActivity($"New resident registered: {resident.FirstName} {resident.LastName}", "bi-person-plus text-success");

                return RedirectToAction(nameof(Index));
            }
            return View(resident);
        }

        // GET: Residents/Delete/5
        public IActionResult Delete(int id)
        {
            var resident = _context.Residents.FirstOrDefault(r => r.ResidentId == id);
            if (resident == null) return NotFound();
            return View(resident); // This usually opens a "Are you sure?" page
        }

        // POST: Residents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var resident = _context.Residents.Find(id);
            if (resident != null)
            {
                string fullName = $"{resident.FirstName} {resident.LastName}";
                _context.Residents.Remove(resident);
                _context.SaveChanges();

                // LOG THE DELETE ACTION
                LogActivity($"Resident record deleted: {fullName}", "bi-trash text-danger");
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult View(int id)
        {
            var resident = _context.Residents.FirstOrDefault(r => r.ResidentId == id);
            if (resident == null) return NotFound();
            return View("ViewResident", resident);
        }

        public IActionResult Edit(int id)
        {
            var resident = _context.Residents.FirstOrDefault(r => r.ResidentId == id);
            if (resident == null) return NotFound();
            return View("EditResident", resident);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Resident resident)
        {
            if (id != resident.ResidentId) 
                return BadRequest();

            if (ModelState.IsValid)
            {
                var existingResident = _context.Residents
                    .FirstOrDefault(r => r.ResidentId == id);

                if (existingResident == null)
                    return NotFound();

                // Update only editable fields
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
                _context.SaveChanges();

                LogActivity($"Resident record updated: {resident.FirstName} {resident.LastName}", "bi-pencil-square text-primary");

                return RedirectToAction(nameof(Index));
            }
            return View("EditResident", resident);
        }

        private void LogActivity(string action, string icon)
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
                _context.SystemActivities.Add(activity);
                _context.SaveChanges();
            }
            catch (Exception) { /* Fail silently to prevent app crash */ }
        }
    }
}
