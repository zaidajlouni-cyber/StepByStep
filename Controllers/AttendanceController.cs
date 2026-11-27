using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepByStep.Helpers;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    [RoleAuthorize("Attendance", "Admin")]
    public class AttendanceController : Controller
    {
        private readonly StepByStepDbContext _context;

        public AttendanceController(StepByStepDbContext context)
        {
            _context = context;
        }

        // GET: Attendance/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? gradeLevel = null, DateOnly? date = null)
        {
            var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var grades = await _context.Students
                .Select(s => s.GradeLevel)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            var selectedGrade = !string.IsNullOrWhiteSpace(gradeLevel) ? gradeLevel : grades.FirstOrDefault();

            var students = new List<Student>();
            var absentStudentIds = new HashSet<int>();

            if (!string.IsNullOrWhiteSpace(selectedGrade))
            {
                students = await _context.Students
                    .Include(s => s.StudentNavigation)
                    .Where(s => s.GradeLevel == selectedGrade)
                    .OrderBy(s => s.StudentNavigation != null ? s.StudentNavigation.FullName : "")
                    .ToListAsync();

                var studentIds = students.Select(s => s.StudentId).ToList();

                var absentRecords = await _context.AttendanceHomeworks
                    .Where(a => studentIds.Contains(a.StudentId) && a.RecordDate == selectedDate && a.IsPresent == false)
                    .Select(a => a.StudentId)
                    .Distinct()
                    .ToListAsync();

                absentStudentIds = new HashSet<int>(absentRecords);
            }

            ViewBag.Grades = grades;
            ViewBag.SelectedGrade = selectedGrade;
            ViewBag.SelectedDate = $"{selectedDate.Year:0000}-{selectedDate.Month:00}-{selectedDate.Day:00}";
            ViewBag.AbsentStudentIds = absentStudentIds;

            return View(students);
        }

        // POST: Attendance/SaveAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(string gradeLevel, DateOnly recordDate, List<int>? absentIds)
        {
            absentIds ??= new List<int>();
            var absentSet = new HashSet<int>(absentIds);

            var classStudents = await _context.Students
                .Where(s => s.GradeLevel == gradeLevel)
                .ToListAsync();

            var defaultSubjectId = await _context.Subjects
                .Select(s => s.SubjectId)
                .FirstOrDefaultAsync();

            if (defaultSubjectId == 0)
            {
                TempData["ErrorMessage"] = "يجب إضافة مادة واحدة على الأقل في النظام قبل تسجيل الحضور.";
                return RedirectToAction(nameof(Index), new { gradeLevel, date = recordDate });
            }

            foreach (var student in classStudents)
            {
                bool isPresent = !absentSet.Contains(student.StudentId);

                var existingRecords = await _context.AttendanceHomeworks
                    .Where(a => a.StudentId == student.StudentId && a.RecordDate == recordDate)
                    .ToListAsync();

                if (existingRecords.Any())
                {
                    foreach (var rec in existingRecords)
                    {
                        rec.IsPresent = isPresent;
                    }
                }
                else if (!isPresent)
                {
                    _context.AttendanceHomeworks.Add(new AttendanceHomework
                    {
                        StudentId = student.StudentId,
                        SubjectId = defaultSubjectId,
                        RecordDate = recordDate,
                        IsPresent = false,
                        HomeworkCompleted = false
                    });
                }
            }

            await _context.SaveChangesAsync();
            var dateText = $"{recordDate.Year:0000}-{recordDate.Month:00}-{recordDate.Day:00}";
            TempData["SuccessMessage"] = $"تم حفظ كشف الحضور والغياب للصف ({gradeLevel}) بتاريخ {dateText} بنجاح.";

            return RedirectToAction(nameof(Index), new { gradeLevel, date = recordDate });
        }
    }
}