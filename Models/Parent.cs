using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class Parent
{
    public int ParentId { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User ParentNavigation { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
