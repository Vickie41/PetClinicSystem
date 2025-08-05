namespace PetClinicSystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string? Action { get; set; }  
        public string? Message { get; set; } 
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string? Details { get; set; }

        public virtual User? User { get; set; }
    }
}