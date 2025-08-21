namespace PetClinicSystem.Models
{
    public class DiagnosticReportsViewModel
    {
        public List<DiagnosticTest> Diagnostics { get; set; }
        public Dictionary<int, Patient> Patients { get; set; }  // Added
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTests { get; set; }
        public string MostCommonTest { get; set; }
    }
}
