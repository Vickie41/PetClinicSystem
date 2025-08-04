using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class ConsultationTreatment
{
    public int ConsultationTreatmentId { get; set; }

    public int ConsultationId { get; set; }

    public int TreatmentId { get; set; }

    public string? Details { get; set; }

    public decimal? Cost { get; set; }

    public string? Notes { get; set; }

    public virtual Consultation Consultation { get; set; } = null!;

    public virtual Treatment Treatment { get; set; } = null!;
}
