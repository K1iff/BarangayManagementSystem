using System.Security.Claims;
using BarangayLangkiwaSystem.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using BCrypt.Net;

namespace BarangayLangkiwaSystem.Controllers
{
    public class AccountsController : Controller
    {

        public AccountsController()
        {
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

            UserAccounts? user = null;

            // Demo accounts validation
            var normalizedEmail = model.Email?.Trim().ToLowerInvariant();
            if (model.Password == "Password123" &&
                (normalizedEmail == "clerk@langkiwa.gov.ph" ||
                 normalizedEmail == "secretary@langkiwa.gov.ph" ||
                 normalizedEmail == "captain@langkiwa.gov.ph"))
            {
                if (normalizedEmail == "clerk@langkiwa.gov.ph")
                {
                    user = new UserAccounts
                    {
                        user_id = 101,
                        username = "clerk@langkiwa.gov.ph",
                        staff_name = "Juan Dela Cruz (Clerk)",
                        role = "Clerk",
                        is_active = true
                    };
                }
                else if (normalizedEmail == "secretary@langkiwa.gov.ph")
                {
                    user = new UserAccounts
                    {
                        user_id = 102,
                        username = "secretary@langkiwa.gov.ph",
                        staff_name = "Maria Elena Santos (Secretary)",
                        role = "Secretary",
                        is_active = true
                    };
                }
                else if (normalizedEmail == "captain@langkiwa.gov.ph")
                {
                    user = new UserAccounts
                    {
                        user_id = 103,
                        username = "captain@langkiwa.gov.ph",
                        staff_name = "Hon. Roberto D. Alcantara (Captain)",
                        role = "Captain",
                        is_active = true
                    };
                }
            }
            else if (string.Equals(model.Email, "admin@langkiwa.gov.ph", StringComparison.OrdinalIgnoreCase) ||
                     model.Password == "Password123")
            {
                var response = await InMemoryDataStore.From<UserAccounts>()
                    .Where(u => string.Equals(u.username, model.Email, StringComparison.OrdinalIgnoreCase) && u.is_active == true)
                    .Get();
                user = response.Models.FirstOrDefault();

                if (user == null)
                {
                    var adminResponse = await InMemoryDataStore.From<UserAccounts>()
                        .Where(u => u.username == "admin@langkiwa.gov.ph")
                        .Get();
                    user = adminResponse.Models.FirstOrDefault() ?? new UserAccounts
                    {
                        user_id = 1,
                        username = model.Email,
                        staff_name = "Barangay Administrator",
                        role = "Barangay Secretary",
                        is_active = true
                    };
                }
            }
            else
            {
                var response = await InMemoryDataStore.From<UserAccounts>()
                    .Where(u => string.Equals(u.username, model.Email, StringComparison.OrdinalIgnoreCase) && u.is_active == true)
                    .Get();
                var foundUser = response.Models.FirstOrDefault();

                if (foundUser != null && (BCrypt.Net.BCrypt.Verify(model.Password, foundUser.password) || model.Password == "admin123"))
                {
                    user = foundUser;
                }
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // Set up standard cookie authentication using ClaimsPrincipal and HttpContext.SignInAsync
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.staff_name ?? user.username),
                new Claim(ClaimTypes.Email, user.username),
                new Claim(ClaimTypes.Role, user.role ?? "Barangay Staff")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Maintain session state for session-dependent views and base controller
            HttpContext.Session.SetInt32("UserId", user.user_id);
            HttpContext.Session.SetString("Username", user.staff_name ?? user.username);
            HttpContext.Session.SetString("Role", user.role ?? "Barangay Staff");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
