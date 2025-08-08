namespace PetClinicSystem.Models
{
    public class PatientViewModel
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }
        public string OwnerName { get; set; }

       
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Color { get; set; }
        public string MicrochipId { get; set; }
    }
}
