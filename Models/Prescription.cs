using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Prescription
{
    public int? PrescriptionId { get; set; }

    public int? ConsultationId { get; set; }

    public string? MedicationName { get; set; } = null!;

    public string? Dosage { get; set; } = null!;
        
    public string? Frequency { get; set; } = null!;

    public string? Duration { get; set; } = null!;

    public string? Instructions { get; set; }

    public DateTime? PrescribedDate { get; set; }

    public bool? IsDispensed { get; set; }

    public int? Refills { get; set; }

    public virtual Consultation Consultation { get; set; } = null!;
}
