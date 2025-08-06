using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class ActivityLog
{
    public int Id { get; set; }

    public string Action { get; set; } = null!;

    public string? Type { get; set; }

    public string? Message { get; set; }

    public DateTime Timestamp { get; set; }

    public int? UserId { get; set; }

    public string? Details { get; set; }

    public virtual User? User { get; set; }
}
