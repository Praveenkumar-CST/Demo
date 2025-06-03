namespace WiseHR.Models
    {
    using System.ComponentModel.DataAnnotations;

    public class AssetIssueRecord
        {
        [Key]
        public int Sno { get; set; }

        [Required]
        public int AssetInstanceId { get; set; }

        [Required, MaxLength(100)]
        public string IssuedTo { get; set; }

        public DateTime IssuedOn { get; set; }

        [Required, MaxLength(100)]
        public string IssuedBy { get; set; }

        [Required, MaxLength(100)]
        public string ApprovedBy { get; set; }

        public DateTime ReturnedOn { get; set; } // Expected return date during assignment

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public AssetInstance? AssetInstance { get; set; }

        public class UnassignRequest
            {
            [Required]
            public int AssetInstanceId { get; set; }

            [Required, MaxLength(500)]
            public string Remarks { get; set; }
            }
        }
    }