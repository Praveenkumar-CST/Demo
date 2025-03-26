namespace WiseHR.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = "Medium"; 
        public string Status { get; set; } = "Pending"; 
        public int? ProjectId { get; set; } 
    }
}
