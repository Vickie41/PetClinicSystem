using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models
{
    public class TreatmentFormModel
    {
        public int ConsultationId { get; set; }
        public int TreatmentId { get; set; }

        [Required]
        [Display(Name = "Treatment")]
        public int SelectedTreatmentId { get; set; }

        public string Details { get; set; }
        public decimal? Cost { get; set; }
        public string Notes { get; set; }

        public List<SelectListItem> AvailableTreatments { get; set; } = new List<SelectListItem>();
    }

    public class PrescriptionFormModel
    {
        public int ConsultationId { get; set; }
        public int PatientId { get; set; }

        [Required]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public string Duration { get; set; }

        public string Instructions { get; set; }

        [Display(Name = "Dispensed?")]
        public bool IsDispensed { get; set; }

        [Display(Name = "Refills")]
        [Range(0, 10, ErrorMessage = "Refills must be between 0 and 10")]
        public int Refills { get; set; }
    }

    public class DiagnosticTestFormModel
    {
        public int ConsultationId { get; set; }

        [Required]
        [Display(Name = "Test Name")]
        public string TestName { get; set; }

        [Display(Name = "Results")]
        public string Results { get; set; }
    }

    public class VaccineFormModel
    {
        public int PatientId { get; set; }
        public int ConsultationId { get; set; }

        [Required]
        [Display(Name = "Vaccine")]
        public int SelectedVaccineId { get; set; }

        [Required]
        [Display(Name = "Date Given")]
        [DataType(DataType.Date)]
        public DateTime DateGiven { get; set; } = DateTime.Today;

        [Display(Name = "Next Due Date")]
        [DataType(DataType.Date)]
        public DateTime? NextDueDate { get; set; }

        public List<SelectListItem> AvailableVaccines { get; set; } = new List<SelectListItem>();
    }
}