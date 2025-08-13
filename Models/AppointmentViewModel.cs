using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PetClinicSystem.Models
{
    public class AppointmentViewModel
    {
        public int PatientId { get; set; }
        public int  VetId { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public List<SelectListItem> AvailablePets { get; set; }
        public List<SelectListItem> AvailableVets { get; set; }


        public int? Duration { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; } = "Scheduled";
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public int AppointmentId { get; set; }
        public string? VetName { get; set; }

        public string PatientName { get; set; }
    }

}
