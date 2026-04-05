using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayLangkiwaSystem.Models;
using BarangayLangkiwaSystem.Data;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace BarangayLangkiwaSystem.Controllers
{
    public class AccountsController : Controller
    {

        private readonly BarangayDBContext _context;

        public AccountsController(BarangayDBContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var user = _context.UserAccounts
                .FirstOrDefault(u => u.username == model.Email && u.is_active);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.password)) // For production, hash passwords!
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.user_id);
            HttpContext.Session.SetString("Username", user.staff_name);
            HttpContext.Session.SetString("Role", user.role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserAccounts model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            // Check if username already exists
            var existingUser = _context.UserAccounts
                .FirstOrDefault(u => u.username == model.username);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            // Hash the password before saving
            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);

            model.is_active = true;

            _context.UserAccounts.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        
    }
}