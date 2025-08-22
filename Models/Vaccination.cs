using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Vaccination
{
    public int? VaccineId { get; set; }

    public string? Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Species { get; set; }

    public string? RecommendedSchedule { get; set; }

    public int? Duration { get; set; }

    public bool? IsCore { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<VaccineRecord> VaccineRecords { get; set; } = new List<VaccineRecord>();
}
