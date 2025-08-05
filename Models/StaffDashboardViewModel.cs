namespace PetClinicSystem.Models
{
    public class StaffDashboardViewModel : DashboardViewModel
    {
        public int NewPatientsThisMonth { get; set; }
        public int PendingPayments { get; set; }
        public decimal PendingPaymentsAmount { get; set; }
        public List<Owner> NewOwners { get; set; }
    }
}
