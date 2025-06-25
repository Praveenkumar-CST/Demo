using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace WiseHR.Models
{
    public class Role : BaseModel
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("role")]
        public string? RoleName { get; set; }

        [JsonPropertyName("empid")]
        public string? EmpId { get; set; }
    }
}