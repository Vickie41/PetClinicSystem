namespace PetClinicSystem.Models
{
    public class FinancialReportsViewModel
    {
        public List<Billing> Bills { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal AverageBillAmount { get; set; }
        public int PaidBillsCount { get; set; }
        public int PendingBillsCount { get; set; }

       

    }
}
