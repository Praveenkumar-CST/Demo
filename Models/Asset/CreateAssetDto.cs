namespace WiseHR.Models
    {
    public class CreateAssetDto
        {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string AssetTagPrefix { get; set; } = string.Empty;
        public int InstanceCount { get; set; } = 1;
        public Dictionary<string, string> Specifications { get; set; } = new(); // Added to match JSON
        }
    public class UpdateAssetDto
        {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string AssetTagPrefix { get; set; } = string.Empty;
        public Dictionary<string, string> Specifications { get; set; } = new(); // Added for consistency
        }

    public class CreateInstanceDto
        {
        public string AssetTag { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public string? AssignedTo { get; set; }
        // Remove Specifications to match JSON example
        // public string Specifications { get; set; } = "{}";
        }

    public class UpdateInstanceDto
        {
        public string AssetTag { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        // Remove Specifications for consistency
        // public string Specifications { get; set; } = "{}";
        }
    }