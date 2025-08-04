using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Treatment
{
    public int TreatmentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal? DefaultCost { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ConsultationTreatment> ConsultationTreatments { get; set; } = new List<ConsultationTreatment>();
}
