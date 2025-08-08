using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetClinicSystem.Models
{
    public class AppointmentViewModel
    {
        public int PatientId { get; set; }
        public int  VetId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public List<SelectListItem> AvailablePets { get; set; }
        public List<SelectListItem> AvailableVets { get; set; }
    }

}
