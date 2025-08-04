using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class VaccineRecord
{
    public int RecordId { get; set; }

    public int VaccineId { get; set; }

    public int PatientId { get; set; }

    public int AdministeredBy { get; set; }

    public DateOnly DateGiven { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public string? LotNumber { get; set; }

    public string? Notes { get; set; }

    public virtual User AdministeredByNavigation { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual Vaccination Vaccine { get; set; } = null!;
}
