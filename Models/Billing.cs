using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class Billing
{
    public int BillId { get; set; }

    public int? ConsultationId { get; set; }

    public int? AppointmentId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? BillDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<BillingDetail> BillingDetails { get; set; } = new List<BillingDetail>();

    public virtual Consultation? Consultation { get; set; }
    public virtual Patient Patient { get; set; }

}
