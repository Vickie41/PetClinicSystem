namespace PetClinicSystem.Models
{
    public class AdminDashboardViewModel : DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveVets { get; set; }
        public int ActiveStaff { get; set; }
        public decimal YearlyRevenue { get; set; }
        public List<Billing> RecentPayments { get; set; }
        public Dictionary<string, decimal> RevenueByService { get; set; }
    }
}
