using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseHR.Models
{
    // Custom validation attribute for fortnight-based required fields
    public class RequiredForFortnightAttribute : ValidationAttribute
    {
        private readonly string _fortnight;
        private readonly int _day;

        public RequiredForFortnightAttribute(string fortnight)
        {
            _fortnight = fortnight;
            _day = DateTime.UtcNow.Day; // Current day for validation
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isFirstFortnight = _day >= 1 && _day <= 15;
            bool isSecondFortnight = _day >= 16 && _day <= 31;

            if (_fortnight == "First" && isFirstFortnight && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required for the first fortnight.");
            }
            else if (_fortnight == "Second" && isSecondFortnight && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required for the second fortnight.");
            }

            return ValidationResult.Success;
        }
    }

    public class ReportModel
    {
        [Key]
        public int Id { get; set; }

        // Mentee Info
        [Required(ErrorMessage = "MenteeName is required")]
        public string? MenteeName { get; set; }

        [Required(ErrorMessage = "MenteeEmail is required")]
        public string? MenteeEmail { get; set; }

        public string MenteeId { get; set; }
        public string MentorId { get; set; }

        // Mentor Info
        public string? MentorEmail { get; set; }
        public string? MentorName { get; set; }
        public string? MentorDesignation { get; set; }

        // Fortnight Remarks1 (Required only in first fortnight, i.e., 1st to 15th)
        [RequiredForFortnight("First", ErrorMessage = "Performance1 is required for the first fortnight")]
        public string? Performance1 { get; set; }

        [RequiredForFortnight("First", ErrorMessage = "Feedback1 is required for the first fortnight")]
        public string? Feedback1 { get; set; }

        [RequiredForFortnight("First", ErrorMessage = "Completed Story Points1 is required for the first fortnight")]
        [Range(0, int.MaxValue, ErrorMessage = "Completed Story Points1 must be a non-negative number")]
        public int? CompletedStoryPoints1 { get; set; }

        // Fortnight Remarks2 (Required only in second fortnight, i.e., 16th to 31st)
        [RequiredForFortnight("Second", ErrorMessage = "Performance2 is required for the second fortnight")]
        public string? Performance2 { get; set; }

        [RequiredForFortnight("Second", ErrorMessage = "Feedback2 is required for the second fortnight")]
        public string? Feedback2 { get; set; }

        [RequiredForFortnight("Second", ErrorMessage = "Completed Story Points2 is required for the second fortnight")]
        [Range(0, int.MaxValue, ErrorMessage = "Completed Story Points2 must be a non-negative number")]
        public int? CompletedStoryPoints2 { get; set; }

        // Project
        [Required(ErrorMessage = "ProjectWorkedOn is required")]
        public string ProjectsWorkedOn { get; set; }

        // Progress
        [Required(ErrorMessage = "ProgressStage is required")]
        public string ProgressStage { get; set; }

        [Required(ErrorMessage = "ProgressPercentage is required")]
        public int ProgressPercentage { get; set; }

        // Time Tracking
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    }
}