using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string? Specialization { get; set; }

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public virtual User TeacherNavigation { get; set; } = null!;
}
