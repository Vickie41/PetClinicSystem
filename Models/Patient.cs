using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models;

public partial class Patient
{

    public int PatientId { get; set; }

    public int OwnerId { get; set; }

    public string Name { get; set; } = null!;

    public string Species { get; set; } = null!;

    public string? Breed { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Color { get; set; }

    public string? MicrochipId { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalNotes { get; set; }

    public string? PhotoPath { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<VaccineRecord> VaccineRecords { get; set; } = new List<VaccineRecord>();

    
}
