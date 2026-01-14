using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using System.Collections.Generic;

namespace BarangayLangkiwaSystem.Controllers
{
    public class ResidentsController : Controller
    {
        // GET: Residents/Index
        public IActionResult Index()
        {
            // Data for Summary Cards from Figure 11 of the project report
            ViewBag.TotalRegistered = 245;
            ViewBag.ActiveVoters = 187;
            ViewBag.NewResidents = 12;
            ViewBag.TotalHouseholds = 98;

            // Mock data matches the updated Resident model with Age property
            var residents = new List<Resident>
            {
                new Resident { ResidentId = 1, FirstName = "Juan", LastName = "Dela Cruz", Age = 42, Address = "123 Main St, Langkiwa", DateRegistered = new System.DateTime(2023, 1, 15), VoterStatus = "Active" },
                new Resident { ResidentId = 2, FirstName = "Maria", LastName = "Santos", Age = 35, Address = "456 Second Ave, Langkiwa", DateRegistered = new System.DateTime(2024, 8, 20), VoterStatus = "Active" },
                new Resident { ResidentId = 3, FirstName = "Roberto", LastName = "Garcia", Age = 28, Address = "789 Third St, Langkiwa", DateRegistered = new System.DateTime(2025, 10, 12), VoterStatus = "Not Voter" }
            };

            return View(residents);
        }

        // GET: Residents/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Residents/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Resident resident)
        {
            if (ModelState.IsValid)
            {
                // Future: Database implementation logic
                return RedirectToAction(nameof(Index));
            }
            return View(resident);
        }
    }
}