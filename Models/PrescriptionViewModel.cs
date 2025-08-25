using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models
{
    public class PrescriptionViewModel
    {
        public int PrescriptionId { get; set; }

        [Required]
        [Display(Name = "Consultation")]
        public int ConsultationId { get; set; }

        public int PatientId { get; set; }

        [Display(Name = "Patient")]
        public string PatientName { get; set; }

        [Required]
        [Display(Name = "Medication Name")]
        public string MedicationName { get; set; }

        //[Required]
        public string Dosage { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public string Duration { get; set; }

        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Prescribed Date")]
        [DataType(DataType.Date)]
        public DateTime PrescribedDate { get; set; }

        [Required]
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Dispensed?")]
        public bool IsDispensed { get; set; }

        [Display(Name = "Refills Remaining")]
        [Range(0, 10, ErrorMessage = "Refills must be between 0 and 10")]
        public int RefillsRemaining { get; set; }

        public List<SelectListItem> AvailableConsultations { get; set; } = new List<SelectListItem>();
    }
}
