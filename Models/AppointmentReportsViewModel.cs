namespace PetClinicSystem.Models
{
    public class AppointmentReportsViewModel
    {
        public List<Appointment> Appointments { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int PendingCount { get; set; }
    }
}
