namespace PetClinicSystem.Models
{
    public class SystemReportsViewModel
    {
        public List<User> Users { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int Last24HoursActivity { get; set; }
    }

}
