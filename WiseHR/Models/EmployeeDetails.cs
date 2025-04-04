using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
{
    public class EmployeeDetails
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Father's Name is required")]
        public string FatherName { get; set; }

        [Required(ErrorMessage = "Mother's Name is required")]
        public string MotherName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Marital Status is required")]
        public string MaritalStatus { get; set; }

        [Required(ErrorMessage = "Blood Group is required")]
        public string BloodGroup { get; set; }

        [Required(ErrorMessage = "Nationality is required")]
        public string Nationality { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateOfBirth { get; set; }
        public string Sons { get; set; }

        public string Daughters { get; set; }

        public string TypeOfEmployment { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Joining Location is required")]
        public string JoiningLocation { get; set; }

        [Required(ErrorMessage = "Date of Joining is required")]
        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage = "Date of Relieving is required")]
        public DateTime DateOfRelieving { get; set; }

     

        public string Allergies { get; set; } = string.Empty;

        public string Medications { get; set; } = string.Empty;

        [Required(ErrorMessage = "Physically Challenged status is required")]
        public string PhysicallyChallenged { get; set; }

        public int NoOfChildren { get; set; }

      

        [Required(ErrorMessage = "Current Address is required")]
        public string CurrentAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string CurrentCity { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string CurrentState { get; set; }

        [Required(ErrorMessage = "Zip / Pin Code is required")]
        public string CurrentZip { get; set; }

        [Required(ErrorMessage = "Mobile No is required")]
        public string CurrentMobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string CurrentEmail { get; set; }

        // Permanent Address Fields
        [Required(ErrorMessage = "Permanent Address is required")]
        public string PermanentAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string PermanentCity { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string PermanentState { get; set; }

        [Required(ErrorMessage = "Zip / Pin Code is required")]
        public string PermanentZip { get; set; }

        [Required(ErrorMessage = "Mobile No is required")]
        public string PermanentMobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string PermanentEmail { get; set; }

        // Passport Details
        [Required(ErrorMessage = "Full Name as in Passport is required")]
        public string PassportFullName { get; set; }

        [Required(ErrorMessage = "Passport No is required")]
        public string PassportNo { get; set; }

        [Required(ErrorMessage = "Nationality is required")]
        public string PassportNationality { get; set; }

        [Required(ErrorMessage = "Date of Issue is required")]
        public DateTime PassportIssueDate { get; set; }

        [Required(ErrorMessage = "Date of Expiry is required")]
        public DateTime PassportExpiryDate { get; set; }

        [Required(ErrorMessage = "Place of Issue is required")]
        public string PassportPlaceOfIssue { get; set; }

        // Emergency Contact Fields
        [Required(ErrorMessage = "Emergency Contact 1 Name is required")]
        public string EmergencyContact1Name { get; set; }

        [Required(ErrorMessage = "Relationship with Employee is required")]
        public string EmergencyContact1Relationship { get; set; }

        [Required(ErrorMessage = "Emergency Contact 1 Address is required")]
        public string EmergencyContact1Address { get; set; }

        [Required(ErrorMessage = "Emergency Contact 1 City is required")]
        public string EmergencyContact1City { get; set; }

        [Required(ErrorMessage = "Emergency Contact 1 State is required")]
        public string EmergencyContact1State { get; set; }

        [Required(ErrorMessage = "Emergency Contact 1 Zip/Pin Code is required")]
        public string EmergencyContact1ZipCode { get; set; }

        [Required(ErrorMessage = "Emergency Contact 1 Mobile No is required")]
        public string EmergencyContact1Mobile { get; set; }

        // Emergency Contact 2 Fields
        [Required(ErrorMessage = "Emergency Contact 2 Name is required")]
        public string EmergencyContact2Name { get; set; }

        [Required(ErrorMessage = "Relationship with Employee is required")]
        public string EmergencyContact2Relationship { get; set; }

        [Required(ErrorMessage = "Emergency Contact 2 Address is required")]
        public string EmergencyContact2Address { get; set; }

        [Required(ErrorMessage = "Emergency Contact 2 City is required")]
        public string EmergencyContact2City { get; set; }

        [Required(ErrorMessage = "Emergency Contact 2 State is required")]
        public string EmergencyContact2State { get; set; }

        [Required(ErrorMessage = "Emergency Contact 2 Zip/Pin Code is required")]
        public string EmergencyContact2ZipCode { get; set; }

        [Required(ErrorMessage = "Emergency Contact 2 Mobile No is required")]
        public string EmergencyContact2Mobile { get; set; }

        // Nominee Details
        [Required(ErrorMessage = "Nominee Name is required")]
        public string NomineeName { get; set; }

        [Required(ErrorMessage = "Nominee Relationship with Employee is required")]
        public string NomineeRelationship { get; set; }

        [Required(ErrorMessage = "Nominee Address is required")]
        public string NomineeAddress { get; set; }

        [Required(ErrorMessage = "Nominee City is required")]
        public string NomineeCity { get; set; }

        [Required(ErrorMessage = "Nominee State is required")]
        public string NomineeState { get; set; }

        [Required(ErrorMessage = "Nominee Zip/Pin Code is required")]
        public string NomineeZipCode { get; set; }

        [Required(ErrorMessage = "Nominee Mobile No is required")]
        public string NomineeMobile { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    }
}