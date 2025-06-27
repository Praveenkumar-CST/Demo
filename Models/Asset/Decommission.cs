using System;
using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
{
    /// <summary>
    /// Represents a record of an asset decommissioning in the system.
    /// </summary>
    public class DecommissionRecord
    {
        /// <summary>
        /// Unique identifier for the decommission record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Asset tag of the decommissioned instance.
        /// </summary>
        [MaxLength(255, ErrorMessage = "Asset Tag cannot exceed 255 characters.")]
        public string AssetTag { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID of the person who decommissioned the asset.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Decommissioned By Employee ID cannot exceed 100 characters.")]
        public string DecommissionedByEmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the person who decommissioned the asset.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Decommissioned By Full Name cannot exceed 200 characters.")]
        public string DecommissionedByFullName { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID of the person who approved the decommissioning.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Approved By Employee ID cannot exceed 100 characters.")]
        public string ApprovedByEmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the person who approved the decommissioning.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Approved By Full Name cannot exceed 200 characters.")]
        public string ApprovedByFullName { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the asset was decommissioned (UTC).
        /// </summary>
        public DateTime DecommissionDate { get; set; }

        /// <summary>
        /// Remarks or notes about the decommissioning.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the record was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating or updating a decommission record.
    /// </summary>
    public class DecommissionDto
    {
        /// <summary>
        /// Asset tag of the instance to be decommissioned.
        /// </summary>
        [Required(ErrorMessage = "Asset Tag is required.")]
        public string AssetTag { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID of the person decommissioning the asset.
        /// </summary>
        [Required(ErrorMessage = "Decommissioned By Employee ID is required.")]
        [MaxLength(100, ErrorMessage = "Decommissioned By Employee ID cannot exceed 100 characters.")]
        public string DecommissionedByEmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the person decommissioning the asset.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Decommissioned By Full Name cannot exceed 200 characters.")]
        public string DecommissionedByFullName { get; set; } = string.Empty;

        /// <summary>
        /// Employee ID of the person approving the decommissioning.
        /// </summary>
        [Required(ErrorMessage = "Approved By Employee ID is required.")]
        [MaxLength(100, ErrorMessage = "Approved By Employee ID cannot exceed 100 characters.")]
        public string ApprovedByEmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the person approving the decommissioning.
        /// </summary>
        [MaxLength(200, ErrorMessage = "Approved By Full Name cannot exceed 200 characters.")]
        public string ApprovedByFullName { get; set; } = string.Empty;

        /// <summary>
        /// Date and time of decommissioning (IST).
        /// </summary>
        [Required(ErrorMessage = "Decommission Date is required.")]
        public DateTime DecommissionDate { get; set; }

        /// <summary>
        /// Optional remarks about the decommissioning.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }

        /// <summary>
        /// Converts the decommission date from IST to UTC by subtracting 5 hours and 30 minutes.
        /// </summary>
        /// <returns>The decommission date in UTC.</returns>
        public DateTime GetUtcDecommissionDate()
        {
            return DecommissionDate.Subtract(TimeSpan.FromHours(5.5));
        }
    }
}