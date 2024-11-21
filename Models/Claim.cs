using System.ComponentModel.DataAnnotations;

namespace Prog2bPOEPart2.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public string? UserID { get; set; }

        public string? Name { get; set; }

        public DateTime DateSubmitted { get; set; }

        public double HoursWorked { get; set; }

        public double HourlyRate { get; set; }
        public double TotalEarning
        {
            get
            {
                return HoursWorked * HourlyRate;
            }
        }
        public string? ExtraNotes { get; set; }

        public string Status { get; set; } = "Pending";

        // Track the approvals for Programme Coordinator and Academic Manager
        public bool IsProgrammeCoordinatorApproved { get; set; }
        public bool IsAcademicManagerApproved { get; set; }

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
//Code attribution
//Collections in C#
//https://www.geeksforgeeks.org/collections-in-c-sharp/