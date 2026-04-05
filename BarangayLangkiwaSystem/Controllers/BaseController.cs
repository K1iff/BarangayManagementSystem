using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BarangayLangkiwaSystem.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                context.Result = RedirectToAction("Login", "Accounts");
                return;
            }

            // Fetch session data
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Guest";
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "User";

            // Compute initials
            var username = ViewBag.Username as string;
            string initials = "";
            if (!string.IsNullOrEmpty(username))
            {
                var names = username.Split(' ');
                foreach (var n in names)
                {
                    if (!string.IsNullOrEmpty(n))
                        initials += n[0];
                }
            }
            ViewBag.Initials = initials.ToUpper();

            base.OnActionExecuting(context);
        }
    }
}
