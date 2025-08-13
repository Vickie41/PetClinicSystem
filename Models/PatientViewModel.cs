using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models
{
    public class PatientViewModel
    {
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Pet name is required")]
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public string? OwnerName { get; set; }

        public string? Allergies { get; set; }
        public string? MedicalNotes { get; set; }

        [Display(Name = "Pet Photo")]
        [DataType(DataType.Upload)]
        public IFormFile? PhotoFile { get; set; }  // For file upload

        public string? PhotoPath { get; set; }  // To store the path

        public int OwnerId { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Color { get; set; }
        public string? MicrochipId { get; set; } 


       
    }
}
