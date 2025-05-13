namespace WiseHR.Dtos
{
    public class AnalyticsResultDto
    {
        public int Id { get; set; }
        public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? TypeOfEmployment { get; set; }
        public string? Level { get; set; }
        public string? Designation { get; set; }
        public string? JoiningLocation { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? BloodGroup { get; set; }
        public string? Nationality { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? PhysicallyChallenged { get; set; }
        public int? Sons { get; set; }
        public int? Daughters { get; set; }
        public string? CurrentAddress { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentState { get; set; }
        public string? CurrentZip { get; set; }
        public string? CurrentMobile { get; set; }
        public string? CurrentEmail { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentState { get; set; }
        public string? PermanentZip { get; set; }
        public string? PermanentMobile { get; set; }
        public string? PermanentEmail { get; set; }
        public string? PassportFullName { get; set; }
        public string? PassportNo { get; set; }
        public string? PassportNationality { get; set; }
        public DateTime? PassportIssueDate { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? PassportPlaceOfIssue { get; set; }
        public string? EmergencyContact1Name { get; set; }
        public string? EmergencyContact1Relationship { get; set; }
        public string? EmergencyContact1Address { get; set; }
        public string? EmergencyContact1City { get; set; }
        public string? EmergencyContact1State { get; set; }
        public string? EmergencyContact1ZipCode { get; set; }
        public string? EmergencyContact1Mobile { get; set; }
        public string? EmergencyContact2Name { get; set; }
        public string? EmergencyContact2Relationship { get; set; }
        public string? EmergencyContact2Address { get; set; }
        public string? EmergencyContact2City { get; set; }
        public string? EmergencyContact2State { get; set; }
        public string? EmergencyContact2ZipCode { get; set; }
        public string? EmergencyContact2Mobile { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? ProfilePicture { get; set; }
    }
} 