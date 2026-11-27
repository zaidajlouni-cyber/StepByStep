using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class AttendanceHomework
{
    public int RecordId { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public DateOnly? RecordDate { get; set; }

    public bool IsPresent { get; set; }

    public bool HomeworkCompleted { get; set; }

    public string? HomeworkDescription { get; set; }

    public string? SubmissionText { get; set; }

    public string? SubmissionImagePath { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}