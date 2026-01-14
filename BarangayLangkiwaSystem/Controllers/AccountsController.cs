using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.Controllers
{
    // Ensure the class name is AccountsController to match your updated file name
    public class AccountsController : Controller
    {
        // GET: Accounts/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Accounts/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                // Hardcoded test user: admin@langkiwa.gov.ph / Password123
                if (model.Email == "admin@langkiwa.gov.ph" && model.Password == "Password123")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        // UPDATED: This action handles the Sign Out request
        public IActionResult Logout()
        {
            // In a real system, you would clear cookies/sessions here.
            // For now, we simply redirect back to the Login page.
            return RedirectToAction("Login");
        }
    }
}