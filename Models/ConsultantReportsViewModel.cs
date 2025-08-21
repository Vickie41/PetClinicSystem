namespace PetClinicSystem.Models
{
    public class ConsultantReportsViewModel
    {
        public List<Consultation> Consultations { get; set; }
        public Dictionary<int, Patient> Patients { get; set; }  // Added
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalConsultations { get; set; }
        public Dictionary<string, int> ConsultationsByVet { get; set; }
    }
}
