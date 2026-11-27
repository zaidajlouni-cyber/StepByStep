using System;

namespace StepByStep.Models;

public partial class StudentBehavior
{
    public int BehaviorId { get; set; }

    public int StudentId { get; set; }

    public int TeacherId { get; set; }

    public string BehaviorType { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}