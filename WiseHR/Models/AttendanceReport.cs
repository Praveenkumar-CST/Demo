namespace WiseHR.Models
{
    public class AttendanceReport
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int DaysPresent { get; set; }
        public int DaysAbsent { get; set; }
        public int DaysLate { get; set; }
        public double AttendancePercentage => TotalDays > 0 ? (DaysPresent / (double)TotalDays) * 100 : 0;
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
    }
}
