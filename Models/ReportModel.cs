using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseHR.Models
{
    // Custom validation attribute for fortnight-based required fields
    public class RequiredForFortnightAttribute : ValidationAttribute
    {
        private readonly string _fortnight;
        private readonly int _day;
        private readonly int _month;
        private readonly int _year;
        public RequiredForFortnightAttribute(string fortnight)
        {
            _fortnight = fortnight;
            var now = DateTime.UtcNow;
            _day = now.Day;
            _month = now.Month;
            _year = now.Year;// Current day for validation
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // First fortnight: 11th to 24th of the current month
            bool isFirstFortnightSubmission = _day >= 11 && _day <= 24;

            // Second fortnight: 24th of current month to 11th of next month
            bool isSecondFortnightSubmission = (_day >= 24 && _month == DateTime.UtcNow.Month) ||
                                              (_day <= 11 && _month == DateTime.UtcNow.Month && _year == DateTime.UtcNow.Year);

            if (_fortnight == "First" && isFirstFortnightSubmission && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required for the first fortnight (11th to 24th).");
            }
            else if (_fortnight == "Second" && isSecondFortnightSubmission && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required for the second fortnight (24th to next month's 11th).");
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
        // Time Tracking
        public DateTime? FirstFortnightSubmittedOn { get; set; } // Added for first fortnight submission time
        public DateTime? SecondFortnightSubmittedOn { get; set; } // Added for second fortnight submission time
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow; // Keep for initial creation
        public bool IsEditable { get; set; }
        public string? CustomProjectName { get; set; }
    }
}