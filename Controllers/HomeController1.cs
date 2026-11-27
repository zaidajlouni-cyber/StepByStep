using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepByStep.Models;

namespace StepByStep.Controllers
{
    public class HomeController : Controller
    {
        private readonly StepByStepDbContext _context;

        public HomeController(StepByStepDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var evaluations = await _context.WeeklyEvaluations
                .Include(e => e.Student)
                    .ThenInclude(s => s.StudentNavigation)
                .Include(e => e.Subject)
                .ToListAsync();

            var topStudentsBySubject = evaluations
                .Where(e => e.Subject != null && e.Student != null)
                .GroupBy(e => new { e.SubjectId, SubjectName = e.Subject!.SubjectName })
                .OrderBy(g => g.Key.SubjectName)
                .Select(g => new TopSubjectStudentsVm
                {
                    SubjectName = g.Key.SubjectName,
                    TopStudents = g.GroupBy(e => new
                    {
                        e.StudentId,
                        StudentName = e.Student!.StudentNavigation?.FullName ?? $"طالب #{e.StudentId}"
                    })
                        .Select(sg => new TopStudentItemVm
                        {
                            StudentName = sg.Key.StudentName,
                            TotalScore = sg.Sum(x => x.Score),
                            AverageScore = Math.Round(sg.Average(x => x.Score), 1)
                        })
                        .OrderByDescending(x => x.TotalScore)
                        .ThenByDescending(x => x.AverageScore)
                        .Take(3)
                        .ToList()
                })
                .ToList();

            ViewBag.TopStudentsBySubject = topStudentsBySubject;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class TopSubjectStudentsVm
    {
        public string SubjectName { get; set; } = string.Empty;
        public List<TopStudentItemVm> TopStudents { get; set; } = new();
    }

    public class TopStudentItemVm
    {
        public string StudentName { get; set; } = string.Empty;
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
    }
}