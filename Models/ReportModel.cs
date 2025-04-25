

﻿using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models

{
    public class ReportModel
    {
        public string Id { get; set; }
        public string MenteeName { get; set; }
        public string MenteeEmail { get; set; }

        public string MentorEmail { get; set; }
        public string ProgressNote { get; set; }

        public int ProgressPercentage { get; set; }
        public byte[] UploadedFile { get; set; }
        public string FileName { get; set; }


        public string MentorName { get; set; }
        public string MentorDesignation { get; set; }

        // Fortnight Remarks
        // Fortnight Details
        [Required(ErrorMessage = "FortnightNumber is required")]

        public int FortnightNumber { get; set; } // 1 or 2

        [Required(ErrorMessage = "FortnightRemark is required")]

        public string FortnightRemarks { get; set; } // Replaces Fortnight1Remarks and Fortnight2Remarks


        // Project
        [Required(ErrorMessage = "ProjectWorkedOn is required")]
        public string ProjectsWorkedOn { get; set; }

        // Progres

        [Required(ErrorMessage = "PerformanceRating is required")]
        [Range(0, 5, ErrorMessage = "Performance rating must be between 0 and 5.")]

        public int PerformanceRating { get; set; }

        [Required(ErrorMessage = "ProgressStage is required")]
        public string ProgressStage { get; set; }

        [Required(ErrorMessage = "ProgressPercentage is required")]
       

        public DateTime SubmittedOn { get; set; }
    }

}
