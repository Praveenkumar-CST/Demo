using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseHR.Models
{
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

        //[ForeignKey(nameof(MenteeId))]
        //public EmployeeDetails Mentee { get; set; }

        public string MentorId { get; set; }

        //[ForeignKey(nameof(MentorId))]
        //public EmployeeDetails Mentor { get; set; }

        // Mentor Info
        public string? MentorEmail { get; set; }

        public string? MentorName { get; set; }
        public string? MentorDesignation { get; set; }

        // Fortnight Remarks

        [Required(ErrorMessage = "FortnightRemark1 is required")]

        public string FortnightRemarks1 { get; set; } // Replaces Fortnight1Remarks and Fortnight2Remarks

        [Required(ErrorMessage = "FortnightRemark2 is required")]

        public string FortnightRemarks2 { get; set; } // Replaces Fortnight1Remarks and Fortnight2Remarks


        // Project
        [Required(ErrorMessage = "ProjectWorkedOn is required")]
        public string ProjectsWorkedOn { get; set; }

        // Progres
        [Required(ErrorMessage = "ProgressStage is required")]
        public string ProgressStage { get; set; }

        [Required(ErrorMessage = "ProgressPercentage is required")]
        public int ProgressPercentage { get; set; }

        // File Upload
        public byte[] UploadedFile { get; set; }
        public string FileName { get; set; }

        // Time Tracking
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

    }
}