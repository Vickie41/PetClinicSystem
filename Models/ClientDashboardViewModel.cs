namespace PetClinicSystem.Models
{
    public class ClientDashboardViewModel
    {
        public List<Patient> MyPets { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; }
        public List<VaccineRecord> UpcomingVaccinations { get; set; }
        public List<Prescription> ActivePrescriptions { get; set; }
        public decimal OutstandingBalance { get; set; }
    }
}
