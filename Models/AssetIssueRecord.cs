using System;
using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
{
    public class AssetIssueRecord
    {
        [Key]
        public int Sno { get; set; }
        public int AssetInstanceId { get; set; }
        [Required(ErrorMessage = "Issued To is required")]
        public string IssuedTo { get; set; } = string.Empty;
        [Required(ErrorMessage = "Issued On is required")]
        public DateTime IssuedOn { get; set; }
        [Required(ErrorMessage = "Issued By is required")]
        public string IssuedBy { get; set; } = string.Empty;
        [Required(ErrorMessage = "Approved By is required")]
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ReturnedOn { get; set; }
        public string? Remarks { get; set; }
        // Navigation property, nullable to avoid serialization issues
        public AssetInstance? AssetInstance { get; set; }
    }

    public class UnassignRequest
    {
        public int AssetInstanceId { get; set; }
        [Required(ErrorMessage = "Remarks is required")]
        public string Remarks { get; set; } = string.Empty;
    }
}