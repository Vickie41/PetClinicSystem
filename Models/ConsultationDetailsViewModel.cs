namespace PetClinicSystem.Models
{
    public class ConsultationDetailsViewModel
    {
        // Consultation
        public int ConsultationId { get; set; }
        public int? AppointmentId { get; set; }
        public int VetId { get; set; }
        public int PatientId { get; set; }
        public DateTime ConsultationDate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespirationRate { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool IsFollowUp { get; set; }

        // Navigation ViewModels
        public PatientViewModel? Patient { get; set; }
        public OwnerViewModel? Owner { get; set; }
        public VetViewModel? Vet { get; set; }

        // Collections
        public List<ConsultationTreatmentViewModel> ConsultationTreatments { get; set; } = new();
        public List<PrescriptionViewModel> Prescriptions { get; set; } = new();
        public List<DiagnosticTestViewModel> DiagnosticTests { get; set; } = new();
        public List<VaccineRecordViewModel> VaccineRecords { get; set; } = new();


        // Form Models
        public TreatmentFormModel TreatmentForm { get; set; } = new();
        public PrescriptionFormModel PrescriptionForm { get; set; } = new();
        public DiagnosticTestFormModel DiagnosticForm { get; set; } = new();
        public VaccineFormModel VaccineForm { get; set; } = new();
    }

   
   

    public class TreatmentViewModel
    {
        public int TreatmentId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal? DefaultCost { get; set; }
    }

   

    public class DiagnosticTestViewModel
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public string? Results { get; set; }
    }

    public class VaccineRecordViewModel
    {
        public int RecordId { get; set; }
        public int VaccineId { get; set; }
        public DateTime VaccinationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public VaccineViewModel? Vaccine { get; set; }
    }

    public class VaccineViewModel
    {
        public int VaccineId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
