namespace PetClinicSystem.Models
{
    public class DashboardViewModel
    {
        public int TodayAppointmentsCount { get; set; }
        public int CompletedAppointmentsCount { get; set; }
        public int ActivePatientsCount { get; set; }
        public int DogsCount { get; set; }
        public int CatsCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }
        public int VaccinationsDueCount { get; set; }
        public int OverdueVaccinationsCount { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<ActivityLog> RecentActivities { get; set; }
        public Dictionary<string, int> SpeciesDistribution { get; set; }
        public Dictionary<string, int> MonthlyAppointments { get; set; }
    }
}
