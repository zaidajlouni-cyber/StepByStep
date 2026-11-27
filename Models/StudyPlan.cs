using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class StudyPlan
{
    public int PlanId { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public int WeekNumber { get; set; }

    public decimal RecommendedHours { get; set; }

    public string? PriorityLevel { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
