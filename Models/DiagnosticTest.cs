using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class DiagnosticTest
{
    public int TestId { get; set; }

    public int? ConsultationId { get; set; }

    public string TestType { get; set; } = null!;

    public string TestName { get; set; } = null!;

    public DateTime? TestDate { get; set; }

    public string? Results { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public string? FilePath { get; set; }

    public virtual Consultation? Consultation { get; set; }
}
