using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace BarangayLangkiwaSystem.Controllers
{
    public class AccountsController : Controller
    {
        private readonly Supabase.Client _client;

        public AccountsController(Supabase.Client client)
        {
            _client = client;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var response = await _client.From<UserAccounts>()
                .Where(u => u.username == model.Email && u.is_active == true)
                .Get();

            var user = response.Models.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

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
        public async Task<IActionResult> Register(UserAccounts model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var existingResponse = await _client.From<UserAccounts>()
                .Where(u => u.username == model.username)
                .Get();

            var existingUser = existingResponse.Models.FirstOrDefault();

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);
            model.is_active = true;

            await _client.From<UserAccounts>().Insert(model);

            return RedirectToAction("Login");
        }
    }
}
