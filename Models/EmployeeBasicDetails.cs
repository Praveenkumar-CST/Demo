using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
{
    public class EmployeeBasicDetails
    {

        [Required(ErrorMessage = "Employee ID is required")]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "Current Mobile No is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number starting with 6-9")]
        public string CurrentMobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string CurrentEmail { get; set; }

        [Required(ErrorMessage = "Type of Employment is required")]
        public string TypeOfEmployment { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Joining Location is required")]
        public string JoiningLocation { get; set; }

        [Required(ErrorMessage = "Date of Joining is required")]

        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage = "Level is required")]
        public string Level { get; set; }
    }
}
