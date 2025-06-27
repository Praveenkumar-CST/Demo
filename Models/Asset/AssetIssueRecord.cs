using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WiseHR.Models
{
    public class AssetIssueRecord
    {
        public int Id { get; set; }

        [Required]
        public int AssetInstanceId { get; set; }

        [Required, MaxLength(100)]
        public string IssuedToEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("issuedTo")]
        public string IssuedToFullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string IssuedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("issuedBy")]
        public string IssuedByFullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ApprovedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("approvedBy")]
        public string ApprovedByFullName { get; set; } = string.Empty;

        public DateTime IssuedOn { get; set; }

        public DateTime? ReturnedOn { get; set; }

        public class UnassignRequest
        {
            [Required]
            public int AssetInstanceId { get; set; }

            [Required, MaxLength(100)]
            public string ReceivedByEmployeeId { get; set; } = string.Empty;

            [MaxLength(200)]
            public string ReceivedByFullName { get; set; } = string.Empty;

            [Required]
            public DateTime ReturnDate { get; set; }

            [MaxLength(500)]
            public string? Remarks { get; set; }
        }
    }
}