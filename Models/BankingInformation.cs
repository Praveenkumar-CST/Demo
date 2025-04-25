using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WiseHR.Models
{
    public class BankingInformation
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "Bank Name is required")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Branch Name is required")]
        public string Branch { get; set; }

        [Required(ErrorMessage = "Account Holder Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Account Number is required")]
        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Enter a valid account number (9-18 digits)")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "IFSCode is required")]

       

        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter a valid IFSCode")]

        public string IFSCode { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "PAN Number is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Enter a valid PAN number (e.g., ABCDE1234F)")]
        public string PANNumber { get; set; }

        [Required(ErrorMessage = "AADHAAR Number is required")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Enter a valid 12-digit AADHAAR number")]
        public string AadhaarNumber { get; set; }

        [Required(ErrorMessage = "Branch State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Salary Account Type is required")]
        public string AccountType { get; set; } // "Savings" or "Current"

        public string AadhaarFileName { get; set; }

        [Required(ErrorMessage = "Aadhaar document is required")]
        public string AadhaarBase64Content { get; set; }

        public string? AadhaarContentType { get; set; }

        // PAN File Properties
        public string PanFileName { get; set; }

        [Required(ErrorMessage = "Pan document is required")]
        public string? PanBase64Content { get; set; }
        public string? PanContentType { get; set; }

        // Passbook File Properties
        public string PassbookFileName { get; set; }

        [Required(ErrorMessage = "Passbook document is required")]
        public string? PassbookBase64Content { get; set; }
        public string? PassbookContentType { get; set; }
    }
}

