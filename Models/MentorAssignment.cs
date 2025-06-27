namespace WiseHR.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
        public class MentorAssignment
        {
            [Key]
            public int Id { get; set; }

            [Required(ErrorMessage = "Mentor Employee ID is required")]
            public string MentorEmployeeID { get; set; } = string.Empty;

            public string? MentorEmail { get; set; }

            public string? MentorDesignation { get; set; }


            [Required(ErrorMessage = "Mentee Employee ID is required")]
            public string MenteeEmployeeID { get; set; } = string.Empty;

            public string? MenteeEmail { get; set; }

            public string? MenteeDesignation { get; set; }


            [Required]
            public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        }
    

}
