using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StepByStep.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (_allowedRoles.Length > 0)
            {
                var userRole = context.HttpContext.Session.GetString("UserRole")?.Trim().ToLower();
                var isAllowed = _allowedRoles.Any(r => r.Trim().ToLower() == userRole);

                if (!isAllowed)
                {
                    context.Result = userRole switch
                    {
                        "teacher" => new RedirectToActionResult("Dashboard", "Teachers", null),
                        "student" => new RedirectToActionResult("Index", "Students", null),
                        "parent" => new RedirectToActionResult("Index", "Parents", null),
                        "admin" => new RedirectToActionResult("Index", "Users", null),
                        _ => new RedirectToActionResult("Login", "Auth", null)
                    };
                }
            }
        }
    }
}