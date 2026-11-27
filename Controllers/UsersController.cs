using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepByStep.Helpers;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    [RoleAuthorize("Admin")]
    public class UsersController : Controller
    {
        private readonly StepByStepDbContext _context;

        public UsersController(StepByStepDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(string? search = null, string? roleFilter = null)
        {
            var usersQuery = _context.Users
                .Include(u => u.Student)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var rf = roleFilter.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.Role != null && u.Role.ToLower() == rf);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.Role != null && u.Role.ToLower().Contains(term)) ||
                    (u.Student != null && u.Student.GradeLevel != null && u.Student.GradeLevel.ToLower().Contains(term))
                );
            }

            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;

            return View(await usersQuery.OrderByDescending(u => u.UserId).ToListAsync());
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Parents = await _context.Parents
                .Include(p => p.ParentNavigation)
                .Select(p => new
                {
                    p.ParentId,
                    FullName = p.ParentNavigation != null ? p.ParentNavigation.FullName : $"ولي أمر #{p.ParentId}"
                })
                .ToListAsync();

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string FullName,
            string Email,
            string PasswordHash,
            string Role,
            int? ParentId,
            string? GradeName,
            string? SectionName,
            string? PhoneNumber,
            string? Specialization)
        {
            var user = new User
            {
                FullName = FullName.Trim(),
                Email = Email.Trim(),
                PasswordHash = PasswordHash,
                Role = Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var roleLower = Role.Trim().ToLower();

            if (roleLower == "parent")
            {
                _context.Parents.Add(new Parent
                {
                    ParentId = user.UserId,
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim()
                });
                await _context.SaveChangesAsync();
            }
            else if (roleLower == "teacher")
            {
                _context.Teachers.Add(new Teacher
                {
                    TeacherId = user.UserId,
                    Specialization = string.IsNullOrWhiteSpace(Specialization) ? "عام" : Specialization.Trim()
                });
                await _context.SaveChangesAsync();
            }
            else if (roleLower == "student" && ParentId.HasValue && !string.IsNullOrWhiteSpace(GradeName))
            {
                string fullGrade = string.IsNullOrWhiteSpace(SectionName)
                    ? GradeName.Trim()
                    : $"{GradeName.Trim()} - شعبة {SectionName.Trim()}";

                _context.Students.Add(new Student
                {
                    StudentId = user.UserId,
                    ParentId = ParentId.Value,
                    GradeLevel = fullGrade
                });
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"تم إضافة المستخدم ({user.FullName}) بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}