using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public int TeacherId { get; set; }

    public virtual ICollection<AttendanceHomework> AttendanceHomeworks { get; set; } = new List<AttendanceHomework>();

    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();

    public virtual Teacher Teacher { get; set; } = null!;

    public virtual ICollection<WeeklyEvaluation> WeeklyEvaluations { get; set; } = new List<WeeklyEvaluation>();
}
