using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    public class AuthController : Controller
    {
        private readonly StepByStepDbContext _context;

        public AuthController(StepByStepDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            var existingUserId = HttpContext.Session.GetInt32("UserId");
            if (existingUserId.HasValue)
            {
                var existingRole = HttpContext.Session.GetString("UserRole")?.Trim().ToLower();
                return RedirectByRole(existingRole);
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                SetLoginError("يرجى إدخال البريد الإلكتروني وكلمة المرور.");
                return View();
            }

            var normalizedEmail = email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail);

            if (user == null || user.PasswordHash != password)
            {
                SetLoginError("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
                return View();
            }

            var normalizedRole = (user.Role ?? string.Empty).Trim().ToLower();

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName ?? "مستخدم");
            HttpContext.Session.SetString("UserRole", normalizedRole);

            return RedirectByRole(normalizedRole);
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        private void SetLoginError(string message)
        {
            ViewBag.ErrorMessage = message;
            ViewBag.Error = message;
            TempData["ErrorMessage"] = message;
            ModelState.AddModelError(string.Empty, message);
        }

        private IActionResult RedirectByRole(string? role)
        {
            return role switch
            {
                "admin" => RedirectToAction("Index", "Users"),
                "teacher" => RedirectToAction("Dashboard", "Teachers"),
                "student" => RedirectToAction("Index", "Students"),
                "parent" => RedirectToAction("Index", "Parents"),
                "attendance" => RedirectToAction("Index", "Attendance"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}