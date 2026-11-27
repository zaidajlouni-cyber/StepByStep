using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StepByStep.Helpers;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    [RoleAuthorize("Teacher", "Admin")]
    public class TeachersController : Controller
    {
        private readonly StepByStepDbContext _context;

        public TeachersController(StepByStepDbContext context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Teachers.ToListAsync();
            return View(teachers);
        }

        // GET: Teachers/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard(int? teacherId = null)
        {
            int? currentTeacherId = teacherId ?? HttpContext.Session.GetInt32("UserId");
            if (!currentTeacherId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var teacherSubjects = await _context.Subjects
                .Where(sub => sub.TeacherId == currentTeacherId.Value)
                .ToListAsync();

            var subjectIds = teacherSubjects.Select(s => s.SubjectId).ToList();

            var students = await _context.Students
                .Include(s => s.StudentNavigation)
                .OrderBy(s => s.GradeLevel)
                .ToListAsync();

            var grades = students
                .Select(s => s.GradeLevel)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .ToList();

            var homeworkSubmissions = await _context.AttendanceHomeworks
                .Include(h => h.Student)
                    .ThenInclude(s => s.StudentNavigation)
                .Include(h => h.Subject)
                .Where(h => subjectIds.Contains(h.SubjectId))
                .OrderByDescending(h => h.RecordDate)
                .ThenByDescending(h => h.RecordId)
                .ToListAsync();

            var recentBehaviors = await _context.StudentBehaviors
                .Include(b => b.Student)
                    .ThenInclude(s => s.StudentNavigation)
                .Where(b => b.TeacherId == currentTeacherId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.TeacherName = HttpContext.Session.GetString("UserName")
                ?? await _context.Users.Where(u => u.UserId == currentTeacherId.Value).Select(u => u.FullName).FirstOrDefaultAsync()
                ?? "المعلم";

            ViewBag.TeacherSubjects = teacherSubjects;
            ViewBag.Students = students;
            ViewBag.Grades = grades;
            ViewBag.HomeworkSubmissions = homeworkSubmissions;
            ViewBag.RecentBehaviors = recentBehaviors;

            return View();
        }

        // POST: Teachers/SendBehavior (إرسال سلوك الطالب لولي الأمر فقط)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBehavior(int studentId, string behaviorType, string notes, int? teacherId = null)
        {
            int? currentTeacherId = teacherId ?? HttpContext.Session.GetInt32("UserId");
            if (!currentTeacherId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(notes))
            {
                TempData["ErrorMessage"] = "يرجى كتابة ملاحظة السلوك قبل الإرسال.";
                return RedirectToAction(nameof(Dashboard), new { teacherId });
            }

            // التأكد من وجود سجل في جدول Teachers للمعلم الحالي
            var teacherExists = await _context.Teachers.AnyAsync(t => t.TeacherId == currentTeacherId.Value);
            if (!teacherExists)
            {
                _context.Teachers.Add(new Teacher
                {
                    TeacherId = currentTeacherId.Value,
                    Specialization = "عام"
                });
                await _context.SaveChangesAsync();
            }

            _context.StudentBehaviors.Add(new StudentBehavior
            {
                StudentId = studentId,
                TeacherId = currentTeacherId.Value,
                BehaviorType = string.IsNullOrWhiteSpace(behaviorType) ? "ملاحظة سلوكية" : behaviorType.Trim(),
                Notes = notes.Trim(),
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إرسال تقرير السلوك إلى ولي أمر الطالب بنجاح (لا يظهر للطالب).";

            return RedirectToAction(nameof(Dashboard), new { teacherId });
        }

        // POST: Teachers/AssignHomeworkToClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignHomeworkToClass(string gradeLevel, int subjectId, bool homeworkCompleted, string? homeworkDescription, int? teacherId = null)
        {
            int? currentTeacherId = teacherId ?? HttpContext.Session.GetInt32("UserId");
            if (!currentTeacherId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var ownsSubject = await _context.Subjects
                .AnyAsync(sub => sub.SubjectId == subjectId && sub.TeacherId == currentTeacherId.Value);

            if (!ownsSubject)
            {
                TempData["ErrorMessage"] = "لا يمكنك إضافة واجب لمادة غير مسندة إليك.";
                return RedirectToAction(nameof(Dashboard), new { teacherId });
            }

            var classStudents = await _context.Students
                .Where(s => s.GradeLevel == gradeLevel)
                .ToListAsync();

            if (!classStudents.Any())
            {
                TempData["ErrorMessage"] = "لا يوجد طلاب في هذا الصف.";
                return RedirectToAction(nameof(Dashboard), new { teacherId });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var cleanedDescription = string.IsNullOrWhiteSpace(homeworkDescription) ? null : homeworkDescription.Trim();

            foreach (var student in classStudents)
            {
                var existingRecord = await _context.AttendanceHomeworks
                    .FirstOrDefaultAsync(a => a.StudentId == student.StudentId && a.SubjectId == subjectId && a.RecordDate == today);

                if (existingRecord != null)
                {
                    existingRecord.HomeworkCompleted = homeworkCompleted;
                    if (cleanedDescription != null)
                    {
                        existingRecord.HomeworkDescription = cleanedDescription;
                    }
                }
                else
                {
                    _context.AttendanceHomeworks.Add(new AttendanceHomework
                    {
                        StudentId = student.StudentId,
                        SubjectId = subjectId,
                        RecordDate = today,
                        IsPresent = true,
                        HomeworkCompleted = homeworkCompleted,
                        HomeworkDescription = cleanedDescription
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"تم حفظ الواجب لجميع طلاب الصف ({gradeLevel}) بنجاح.";

            return RedirectToAction(nameof(Dashboard), new { teacherId });
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == teacher.TeacherId).Select(u => u.FullName).FirstOrDefault();

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            ViewData["TeacherId"] = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "UserId", "FullName");
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,Specialization")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "UserId", "FullName", teacher.TeacherId);
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == teacher.TeacherId).Select(u => u.FullName).FirstOrDefault();
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeacherId,Specialization")] Teacher teacher)
        {
            if (id != teacher.TeacherId) return NotFound();

            try
            {
                _context.Update(teacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(teacher.TeacherId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null) return NotFound();

            ViewBag.UserName = _context.Users.Where(u => u.UserId == teacher.TeacherId).Select(u => u.FullName).FirstOrDefault();

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Teachers/AddEvaluation
        [HttpGet]
        public async Task<IActionResult> AddEvaluation(int? teacherId = null)
        {
            int? currentTeacherId = teacherId ?? HttpContext.Session.GetInt32("UserId");

            var students = await _context.Students
                .Include(s => s.StudentNavigation)
                .Select(s => new
                {
                    s.StudentId,
                    DisplayName = s.StudentNavigation != null
                        ? $"{s.StudentNavigation.FullName} ({s.GradeLevel})"
                        : $"Student #{s.StudentId} ({s.GradeLevel})"
                })
                .ToListAsync();

            var subjectsQuery = _context.Subjects.AsQueryable();
            if (currentTeacherId.HasValue)
            {
                subjectsQuery = subjectsQuery.Where(sub => sub.TeacherId == currentTeacherId.Value);
            }

            var subjects = await subjectsQuery
                .Select(sub => new
                {
                    sub.SubjectId,
                    sub.SubjectName
                })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "StudentId", "DisplayName");
            ViewBag.Subjects = new SelectList(subjects, "SubjectId", "SubjectName");

            return View();
        }

        // POST: Teachers/AddEvaluation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvaluation(WeeklyEvaluation evaluation, int? teacherId = null)
        {
            ModelState.Remove(nameof(WeeklyEvaluation.Student));
            ModelState.Remove(nameof(WeeklyEvaluation.Subject));

            int? currentTeacherId = teacherId ?? HttpContext.Session.GetInt32("UserId");

            if (currentTeacherId.HasValue)
            {
                var ownsSubject = await _context.Subjects
                    .AnyAsync(sub => sub.SubjectId == evaluation.SubjectId && sub.TeacherId == currentTeacherId.Value);

                if (!ownsSubject)
                {
                    ModelState.AddModelError(nameof(evaluation.SubjectId), "لا يمكنك تقييم مادة غير مسندة إليك.");
                }
            }

            if (ModelState.IsValid)
            {
                if (evaluation.EvaluationDate == null)
                {
                    evaluation.EvaluationDate = DateOnly.FromDateTime(DateTime.Today);
                }

                _context.WeeklyEvaluations.Add(evaluation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Dashboard), new { teacherId });
            }

            var students = await _context.Students
                .Include(s => s.StudentNavigation)
                .Select(s => new
                {
                    s.StudentId,
                    DisplayName = s.StudentNavigation != null
                        ? $"{s.StudentNavigation.FullName} ({s.GradeLevel})"
                        : $"Student #{s.StudentId} ({s.GradeLevel})"
                })
                .ToListAsync();

            var subjectsQuery = _context.Subjects.AsQueryable();
            if (currentTeacherId.HasValue)
            {
                subjectsQuery = subjectsQuery.Where(sub => sub.TeacherId == currentTeacherId.Value);
            }

            var subjects = await subjectsQuery
                .Select(sub => new
                {
                    sub.SubjectId,
                    sub.SubjectName
                })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "StudentId", "DisplayName", evaluation.StudentId);
            ViewBag.Subjects = new SelectList(subjects, "SubjectId", "SubjectName", evaluation.SubjectId);

            return View(evaluation);
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}