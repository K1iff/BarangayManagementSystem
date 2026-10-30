using Microsoft.AspNetCore.Mvc;
using BarangayLangkiwaSystem.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BarangayLangkiwaSystem.Controllers
{
    public class UserManagementController : BaseController
    {
        private readonly Supabase.Client _client;

        public UserManagementController(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.From<UserAccounts>().Get();
            var accounts = response.Models;
            return View(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var response = await _client.From<UserAccounts>().Where(u => u.user_id == id).Get();
            var user = response.Models.FirstOrDefault();
            if (user != null)
            {
                user.is_active = !user.is_active;
                await user.Update<UserAccounts>();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Accounts");

            var response = await _client.From<UserAccounts>().Where(u => u.user_id == userId.Value).Get();
            var user = response.Models.FirstOrDefault();
            if (user == null) return NotFound();

            return View(user);
        }

        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Accounts");

            var response = await _client.From<UserAccounts>().Where(u => u.user_id == userId.Value).Get();
            var user = response.Models.FirstOrDefault();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(string username, string? newPassword, string? confirmPassword, string staffName)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Accounts");

            var response = await _client.From<UserAccounts>().Where(u => u.user_id == userId.Value).Get();
            var user = response.Models.FirstOrDefault();
            if (user == null) return NotFound();

            user.username = username;
            user.staff_name = staffName;

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["Error"] = "Passwords do not match!";
                    return RedirectToAction("EditProfile");
                }
                user.password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            await user.Update<UserAccounts>();

            HttpContext.Session.SetString("Username", user.username);
            HttpContext.Session.SetString("StaffName", user.staff_name);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}
