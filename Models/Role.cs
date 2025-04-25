using Supabase.Postgrest.Models;

namespace WiseHR.Models
{
    public class Role : BaseModel
    {
        public string? UserId { get; set; }
        public string? RoleName { get; set; }
    }
}