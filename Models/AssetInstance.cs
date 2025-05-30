namespace WiseHR.Models
{
    public class AssetInstance
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Dictionary<string, string> Specifications { get; set; } = new();
        public Asset? Asset { get; set; } // Allow null to match API response
    }

    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string AssetTagPrefix { get; set; } = string.Empty;
        public Dictionary<string, string> Specifications { get; set; } = new();
        public List<AssetInstance> Instances { get; set; } = new();
    }
}