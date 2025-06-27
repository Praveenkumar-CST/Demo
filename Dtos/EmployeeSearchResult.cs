namespace WiseHR.Dtos
{
    public class EmployeeSearchResult
    {
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? ProfilePicture { get; set; }
        public string? EmployeeID { get; set; }
        public List<string> MatchedFields { get; set; } = new List<string>();
        public int Score { get; set; }
        public bool IsNoResultsMessage { get; set; }
    }
} 