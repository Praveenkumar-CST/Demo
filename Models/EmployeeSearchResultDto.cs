namespace WiseHRServer.Models
{
    public class EmployeeSearchResultDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string BloodGroup { get; set; }
        public string ProfileUrl { get; set; }
        public List<string> MatchedFields { get; set; }
        public bool IsAdmin { get; set; }

        // Admin-only fields
        public string EmployeeID { get; set; }
        public string Designation { get; set; }
        public string Allergies { get; set; }
        public string CurrentState { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
    }
} 