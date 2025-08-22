namespace PetClinicSystem.Models
{
    public class ConsultationTreatmentViewModel
    {
        public int ConsultationTreatmentId { get; set; }
        public int TreatmentId { get; set; }
        public string Details { get; set; }
        public decimal? Cost { get; set; }
        public string Notes { get; set; }
        public TreatmentViewModel Treatment { get; set; }
    }
}
