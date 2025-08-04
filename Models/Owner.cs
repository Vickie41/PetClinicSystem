using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Owner
{
    public int OwnerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
