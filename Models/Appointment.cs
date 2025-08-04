using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int VetId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public int? Duration { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual User Vet { get; set; } = null!;
}
