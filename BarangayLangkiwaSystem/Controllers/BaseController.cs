using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BarangayLangkiwaSystem.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // If the endpoint has [AllowAnonymous], allow execution without session
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
            {
                base.OnActionExecuting(context);
                return;
            }

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // If user is authenticated by cookie but session has expired or is absent, restore session if possible or redirect to Login
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(idClaim, out int uid))
                    {
                        HttpContext.Session.SetInt32("UserId", uid);
                        HttpContext.Session.SetString("Username", User.Identity.Name ?? "User");
                        HttpContext.Session.SetString("Role", User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Staff");
                    }
                    else
                    {
                        context.Result = RedirectToAction("Login", "Accounts");
                        return;
                    }
                }
                else
                {
                    context.Result = RedirectToAction("Login", "Accounts");
                    return;
                }
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
