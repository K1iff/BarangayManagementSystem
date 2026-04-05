using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Data;
using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.Controllers
{
    public class UserManagementController : BaseController
    {
        private readonly BarangayDBContext _context;

        public UserManagementController(BarangayDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var accounts = _context.UserAccounts.ToList();
            return View(accounts);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var user = _context.UserAccounts.Find(id);
            if (user != null)
            {
                user.is_active = !user.is_active; // Flip status
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Profile()
        {
            // Retrieve the logged-in User ID from Session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Accounts");

            var user = _context.UserAccounts.Find(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.UserAccounts.Find(userId);
            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(string username, string? newPassword, string? confirmPassword, string staffName)
        {
            var user = _context.UserAccounts.Find(HttpContext.Session.GetInt32("UserId"));

            // Update Username and StaffName
            user.username = username;
            user.staff_name = staffName;

            // Password Update Logic
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match!";
                    return RedirectToAction("EditProfile");
                }
                user.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _context.SaveChanges();
            HttpContext.Session.SetString("Username", user.username);
            HttpContext.Session.SetString("StaffName", user.staff_name);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}
