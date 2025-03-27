namespace WiseHR.Models
{
    public class PerformanceReport
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int TasksCompleted { get; set; }
        public int TasksPending { get; set; }
        public double PerformanceScore { get; set; } // e.g., 0-100
        public string Feedback { get; set; } = string.Empty;
        public DateTime ReviewPeriodStart { get; set; }
        public DateTime ReviewPeriodEnd { get; set; }
    }
}
