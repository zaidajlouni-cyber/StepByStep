using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int ParentId { get; set; }

    public string GradeLevel { get; set; } = null!;

    public virtual ICollection<AttendanceHomework> AttendanceHomeworks { get; set; } = new List<AttendanceHomework>();

    public virtual Parent Parent { get; set; } = null!;

    public virtual User StudentNavigation { get; set; } = null!;

    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();

    public virtual ICollection<WeeklyEvaluation> WeeklyEvaluations { get; set; } = new List<WeeklyEvaluation>();
}
