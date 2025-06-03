using System;
using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
    {
    public class DecommissionDto
        {
        [Required(ErrorMessage = "Asset Tag is required.")]
        public string AssetTag { get; set; } = string.Empty;

        [Required(ErrorMessage = "Decommissioned By is required.")]
        public string DecommissionedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Approved By is required.")]
        public string ApprovedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Decommission Date is required.")]
        public DateTime? DecommissionDate { get; set; }

        [Required(ErrorMessage = "Remark is required.")]
        public string Remark { get; set; } = string.Empty;

        public DateTime? GetUtcDecommissionDate()
            {
            if (!DecommissionDate.HasValue)
                return null;

            var date = DecommissionDate.Value;

            // If the date is in IST, convert it to UTC by subtracting 5:30 hours
            return date.AddHours(-5.5);
            }
        }

    public class DecommissionRecord
        {
        public int Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string DecommissionedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime DecommissionDate { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        }
    }