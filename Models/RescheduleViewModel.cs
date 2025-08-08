using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetClinicSystem.Models
{
    public class RescheduleViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime CurrentAppointmentDate { get; set; }
        public DateTime NewAppointmentDate { get; set; }
        public int VetId { get; set; }
        public List<SelectListItem> AvailableVets { get; set; }
    }


}
