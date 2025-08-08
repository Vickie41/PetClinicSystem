namespace PetClinicSystem.Models
{
    public class PaymentViewModel
    {
        public int BillingId { get; set; }
        public decimal AmountDue { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}
