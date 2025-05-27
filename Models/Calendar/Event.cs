using System.Text.Json.Serialization;

namespace HolidayApp.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Day { get; set; }
        public string HolidayType { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        [JsonPropertyName("location")]
        public string? Location { get; set; }
    }
}