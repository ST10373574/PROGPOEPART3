namespace Prog2bPOEPart2.Models
{
    public class ClaimViewModel
    {
        public int ClaimID { get; set; }
        public DateTime ClaimDate { get; set; }
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount => HoursWorked * HourlyRate; // Automatically calculate the total
        public string Status { get; set; } // e.g., "Submitted", "Approved", "Rejected"
    }
}
