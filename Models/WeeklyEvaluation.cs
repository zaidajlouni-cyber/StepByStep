using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class WeeklyEvaluation
{
    public int EvaluationId { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public int WeekNumber { get; set; }

    public decimal Score { get; set; }

    public string? TeacherNotes { get; set; }

    public DateOnly? EvaluationDate { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
