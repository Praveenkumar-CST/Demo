
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WiseHRServer.Models
{
    public class WorkExperience
    {
        public string Employer { get; set; }
        public string Location { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public string Designation { get; set; }
    }
    public class Education
    {

        

        [Required(ErrorMessage = "Qualification is required")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "university is required")]
        public string University { get; set; }

        [Required(ErrorMessage = "year of passing is required")]
        [Range(1900, 2100, ErrorMessage = "Enter a valid year")]

        [Display(Name = "Year of Passing")]
        public int YearOfPassing { get; set; }

        [Required(ErrorMessage = "percentage is required")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]


        public double Percentage { get; set; }
    }


    public class Experience
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        public string EmployeeID { get; set; }

        [JsonIgnore]
        [Column(TypeName = "nvarchar(max)")]
        public string EducationJson { get; set; }

        [JsonIgnore]
        [Column(TypeName = "nvarchar(max)")]
        public string WorkExperienceJson { get; set; }

        [NotMapped]
        public List<Education> EducationQualifications
        {
            get => string.IsNullOrEmpty(EducationJson) ? new List<Education>() : JsonSerializer.Deserialize<List<Education>>(EducationJson);
            set => EducationJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<WorkExperience> WorkExperiences
        {
            get => string.IsNullOrEmpty(WorkExperienceJson) ? new List<WorkExperience>() : JsonSerializer.Deserialize<List<WorkExperience>>(WorkExperienceJson);
            set => WorkExperienceJson = JsonSerializer.Serialize(value);
        }

    }
}