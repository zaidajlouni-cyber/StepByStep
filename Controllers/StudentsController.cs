using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StepByStep.Helpers;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    [RoleAuthorize("Student", "Parent", "Admin")]
    public class StudentsController : Controller
    {
        private readonly StepByStepDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StudentsController(StepByStepDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Students
        public async Task<IActionResult> Index(int? studentId = null)
        {
            var role = HttpContext.Session.GetString("UserRole")?.ToLower();
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            int? targetStudentId = studentId ?? (role == "student" ? sessionUserId : null);

            if (targetStudentId.HasValue)
            {
                var student = await _context.Students
                    .Include(s => s.StudentNavigation)
                    .FirstOrDefaultAsync(s => s.StudentId == targetStudentId.Value);

                if (student == null)
                {
                    ViewBag.ErrorMessage = "لم يتم العثور على ملف الطالب المرتبط بهذا الحساب.";
                    return View("Portal", null);
                }

                var homeworks = await _context.AttendanceHomeworks
                    .Include(h => h.Subject)
                    .Where(h => h.StudentId == student.StudentId)
                    .OrderByDescending(h => h.RecordDate)
                    .ThenByDescending(h => h.RecordId)
                    .ToListAsync();

                var evaluations = await _context.WeeklyEvaluations
                    .Include(e => e.Subject)
                    .Where(e => e.StudentId == student.StudentId)
                    .OrderByDescending(e => e.EvaluationDate)
                    .ThenByDescending(e => e.WeekNumber)
                    .ToListAsync();

                ViewBag.Homeworks = homeworks;
                ViewBag.Evaluations = evaluations;

                return View("Portal", student);
            }

            var students = await _context.Students
                .Include(s => s.StudentNavigation)
                .ToListAsync();
            return View(students);
        }

        // POST: Students/SubmitHomework
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitHomework(int recordId, string? submissionText, IFormFile? submissionImage)
        {
            var role = HttpContext.Session.GetString("UserRole")?.ToLower();
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (!sessionUserId.HasValue || role != "student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var record = await _context.AttendanceHomeworks
                .FirstOrDefaultAsync(r => r.RecordId == recordId && r.StudentId == sessionUserId.Value);

            if (record == null)
            {
                return NotFound();
            }

            var hasText = !string.IsNullOrWhiteSpace(submissionText);
            var hasImage = submissionImage != null && submissionImage.Length > 0;

            if (!hasText && !hasImage)
            {
                TempData["ErrorMessage"] = "الرجاء إدخال حل نصي أو رفع صورة للواجب قبل التسليم.";
                return RedirectToAction(nameof(Index));
            }

            if (hasText)
            {
                record.SubmissionText = submissionText!.Trim();
            }

            if (hasImage)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(submissionImage!.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["ErrorMessage"] = "يُسمح برفع الصور فقط (JPG, JPEG, PNG, WEBP).";
                    return RedirectToAction(nameof(Index));
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "homeworks");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await submissionImage.CopyToAsync(stream);
                }

                record.SubmissionImagePath = $"/uploads/homeworks/{fileName}";
            }

            record.HomeworkCompleted = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسليم الواجب بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.StudentNavigation)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null) return NotFound();

            ViewBag.UserName = student.StudentNavigation?.FullName;

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Users.Where(u => u.Role == "Student"), "UserId", "FullName");
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,ParentId,GradeLevel")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Users.Where(u => u.Role == "Student"), "UserId", "FullName", student.StudentId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == student.StudentId).Select(u => u.FullName).FirstOrDefault();
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,ParentId,GradeLevel")] Student student)
        {
            if (id != student.StudentId) return NotFound();

            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.StudentId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == student.StudentId).Select(u => u.FullName).FirstOrDefault();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}