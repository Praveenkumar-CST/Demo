namespace WiseHR.Dtos
{
    public class AnalyticsResultDto
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string? ProfilePicture { get; set; } 
        public string? BloodGroup { get; set; }
    }
} 