using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models
{
    public class ConsultationViewModel
    {
        public int ConsultationId { get; set; }   // Used only in Edit mode

        [Required(ErrorMessage = "Please select a patient.")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Please select a veterinarian.")]
        [Display(Name = "Veterinarian")]
        public int VetId { get; set; }

        [Required(ErrorMessage = "Consultation date is required.")]
        [Display(Name = "Consultation Date")]
        [DataType(DataType.DateTime)]
        public DateTime ConsultationDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Diagnosis is required.")]
        public string Diagnosis { get; set; }

        public string Notes { get; set; }

        [Range(0, 999, ErrorMessage = "Weight must be a positive number.")]
        public decimal? Weight { get; set; }

        [Range(0, 999, ErrorMessage = "Temperature must be a valid number.")]
        public decimal? Temperature { get; set; }

        [Range(0, 500, ErrorMessage = "Heart rate must be valid.")]
        [Display(Name = "Heart Rate (bpm)")]
        public int? HeartRate { get; set; }

        [Range(0, 500, ErrorMessage = "Respiration rate must be valid.")]
        [Display(Name = "Respiration Rate (breaths/min)")]
        public int? RespirationRate { get; set; }

        [Display(Name = "Is Follow-up?")]
        public bool IsFollowUp { get; set; }

        [Display(Name = "Follow-up Date")]
        [DataType(DataType.Date)]
        public DateTime? FollowUpDate { get; set; }

        // Dropdown lists
        public List<SelectListItem> AvailablePatients { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableVets { get; set; } = new List<SelectListItem>();
    }
}
