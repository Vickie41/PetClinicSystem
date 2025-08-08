using System;
using System.Collections.Generic;

namespace PetClinicSystem.Models;

public partial class BillingDetail
{
    public int BillingDetailId { get; set; }

    public int BillId { get; set; }

    public string ItemType { get; set; } = null!;

    public int? ItemId { get; set; }

    public string Description { get; set; } = null!;

    public int? Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual Billing Bill { get; set; } = null!;

    public virtual Service Service { get; set; }
}
