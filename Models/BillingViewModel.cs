using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetClinicSystem.Models
{
    public class BillingViewModel
    {
        public int BillId { get; set; }
        public int? ConsultationId { get; set; }
        public int? AppointmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? Balance { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public int? PatientId { get; set; }

        // Select lists for dropdowns
        public List<SelectListItem> AvailableConsultations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableAppointments { get; set; } = new List<SelectListItem>();
    }
}
