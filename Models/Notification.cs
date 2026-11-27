using System;
using System.Collections.Generic;

namespace StepByStep.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int ParentId { get; set; }

    public string Message { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Parent Parent { get; set; } = null!;
}
