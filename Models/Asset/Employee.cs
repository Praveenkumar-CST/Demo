using System.Text.Json.Serialization;

namespace WiseHR.Models
{
    public class Employee
    {
        [JsonPropertyName("employeeID")]
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName
        {
            get
            {
                var fullName = FirstName;
                if (!string.IsNullOrEmpty(MiddleName))
                    fullName += " " + MiddleName;
                if (!string.IsNullOrEmpty(LastName))
                    fullName += " " + LastName;
                return fullName.Trim();
            }
        }
    }
}