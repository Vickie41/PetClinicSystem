namespace PetClinicSystem.Models
{
    public class PrescriptionReportsViewModel
    {
        public List<Prescription> Prescriptions { get; set; }
        public Dictionary<int, Patient> Patients { get; set; }  // Added
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPrescriptions { get; set; }
        public string MostPrescribedMedication { get; set; }
    }
}
