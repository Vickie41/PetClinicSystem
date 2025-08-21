using System.Reflection.PortableExecutable;

namespace PetClinicSystem.Models
{
    public class VaccineReportsViewModel
    {
        public List<VaccineRecord> VaccineRecords { get; set; } = new List<VaccineRecord>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalVaccines { get; set; }
        public string MostCommonVaccine { get; set; } = string.Empty;
    }
}
