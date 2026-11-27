using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StepByStep.Models;
using StepByStep.Helpers;

namespace StepByStep.Controllers
{
    [RoleAuthorize("Parent", "Admin")]
    public class ParentsController : Controller
    {
        private readonly StepByStepDbContext _context;

        public ParentsController(StepByStepDbContext context)
        {
            _context = context;
        }

        // GET: Parents
        public async Task<IActionResult> Index(int? parentId = null)
        {
            var role = HttpContext.Session.GetString("UserRole")?.ToLower();
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            int? targetParentId = parentId ?? (role == "parent" ? sessionUserId : null);

            if (targetParentId.HasValue)
            {
                var parent = await _context.Parents
                    .Include(p => p.ParentNavigation)
                    .Include(p => p.Students)
                        .ThenInclude(s => s.StudentNavigation)
                    .FirstOrDefaultAsync(p => p.ParentId == targetParentId.Value);

                if (parent == null)
                {
                    ViewBag.ErrorMessage = "لم يتم العثور على ملف ولي الأمر المرتبط بهذا الحساب.";
                    return View("Portal", null);
                }

                var childIds = parent.Students.Select(s => s.StudentId).ToList();

                var homeworks = await _context.AttendanceHomeworks
                    .Include(h => h.Student)
                        .ThenInclude(s => s.StudentNavigation)
                    .Include(h => h.Subject)
                    .Where(h => childIds.Contains(h.StudentId))
                    .OrderByDescending(h => h.RecordDate)
                    .ThenByDescending(h => h.RecordId)
                    .ToListAsync();

                var evaluations = await _context.WeeklyEvaluations
                    .Include(e => e.Student)
                        .ThenInclude(s => s.StudentNavigation)
                    .Include(e => e.Subject)
                    .Where(e => childIds.Contains(e.StudentId))
                    .OrderByDescending(e => e.EvaluationDate)
                    .ThenByDescending(e => e.WeekNumber)
                    .ToListAsync();

                var behaviors = await _context.StudentBehaviors
                    .Include(b => b.Student)
                        .ThenInclude(s => s.StudentNavigation)
                    .Include(b => b.Teacher)
                        .ThenInclude(t => t.TeacherNavigation)
                    .Where(b => childIds.Contains(b.StudentId))
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                ViewBag.Homeworks = homeworks;
                ViewBag.Evaluations = evaluations;
                ViewBag.Behaviors = behaviors;

                return View("Portal", parent);
            }

            var parents = await _context.Parents
                .Include(p => p.ParentNavigation)
                .ToListAsync();
            return View(parents);
        }

        // GET: Parents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var parent = await _context.Parents
                .FirstOrDefaultAsync(m => m.ParentId == id);

            if (parent == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == parent.ParentId).Select(u => u.FullName).FirstOrDefault();

            return View(parent);
        }

        // GET: Parents/Create
        public IActionResult Create()
        {
            ViewData["ParentId"] = new SelectList(_context.Users.Where(u => u.Role == "Parent"), "UserId", "FullName");
            return View();
        }

        // POST: Parents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParentId,PhoneNumber")] Parent parent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(parent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(_context.Users.Where(u => u.Role == "Parent"), "UserId", "FullName", parent.ParentId);
            return View(parent);
        }

        // GET: Parents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var parent = await _context.Parents.FindAsync(id);
            if (parent == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == parent.ParentId).Select(u => u.FullName).FirstOrDefault();
            return View(parent);
        }

        // POST: Parents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParentId,PhoneNumber")] Parent parent)
        {
            if (id != parent.ParentId) return NotFound();

            try
            {
                _context.Update(parent);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParentExists(parent.ParentId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Parents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var parent = await _context.Parents
                .FirstOrDefaultAsync(m => m.ParentId == id);

            if (parent == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == parent.ParentId).Select(u => u.FullName).FirstOrDefault();

            return View(parent);
        }

        // POST: Parents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent != null)
            {
                _context.Parents.Remove(parent);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ParentExists(int id)
        {
            return _context.Parents.Any(e => e.ParentId == id);
        }
    }
}