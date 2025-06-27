
        [Required, MaxLength(100)]
        public string ReceivedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("receivedBy")]
        public string ReceivedByFullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ApprovedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("approvedBy")]
        public string ApprovedByFullName { get; set; } = string.Empty;

        public DateTime ReturnedOn { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    public class AssetReturnRecordDto
    {
        [Required]
        public int AssetInstanceId { get; set; }

        [Required, MaxLength(100)]
        public string ReturnedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("returnedBy")]
        public string ReturnedByFullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ReceivedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("receivedBy")]
        public string ReceivedByFullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ApprovedByEmployeeId { get; set; } = string.Empty;

        [MaxLength(200)]
        [JsonPropertyName("approvedBy")]
        public string ApprovedByFullName { get; set; } = string.Empty;

        [Required]
        public DateTime ReturnedOn { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime GetUtcReturnedOn()
        {
            return ReturnedOn.Subtract(TimeSpan.FromHours(5.5));
        }
    }
}