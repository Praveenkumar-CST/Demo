namespace WiseHR.Models
{
    public class Policy
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
