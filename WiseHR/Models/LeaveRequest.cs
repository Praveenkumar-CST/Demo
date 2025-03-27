namespace WiseHR.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // e.g., "Pending", "Approved", "Rejected"
        public DateTime SubmittedDate { get; set; }
    }
}
