using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Consultation
{
    public int ConsultationId { get; set; }

    public int AppointmentId { get; set; }

    public int VetId { get; set; }

    public int PatientId { get; set; }

    public DateTime? ConsultationDate { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Temperature { get; set; }

    public int? HeartRate { get; set; }

    public int? RespirationRate { get; set; }

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public bool? IsFollowUp { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual ICollection<ConsultationTreatment> ConsultationTreatments { get; set; } = new List<ConsultationTreatment>();

    public virtual ICollection<DiagnosticTest> DiagnosticTests { get; set; } = new List<DiagnosticTest>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual User Vet { get; set; } = null!;
}
