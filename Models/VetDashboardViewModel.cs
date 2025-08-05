namespace PetClinicSystem.Models
{
    public class VetDashboardViewModel : DashboardViewModel
    {
        public int MyAppointmentsToday { get; set; }
        public int MyCompletedAppointments { get; set; }
        public int PatientsToFollowUp { get; set; }
        public List<Patient> RecentPatients { get; set; }
        public List<Prescription> RecentPrescriptions { get; set; }
    }
}
