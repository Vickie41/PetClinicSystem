namespace PetClinicSystem.Models
{
    public class TreatmentReportsViewModel
    {
        public List<ConsultationTreatment> ConsultationTreatments { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TotalTreatments { get; set; }
        public string? MostCommonTreatment { get; set; }
    }
}
