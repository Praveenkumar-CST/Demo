 
using System;
using System.ComponentModel.DataAnnotations;

namespace WiseHR.Models
{
    public class AssetReturnRecordDto
    {
        public int AssetInstanceId { get; set; }

        [Required(ErrorMessage = "Returned By is required")]
        public string ReturnedBy { get; set; } = string.Empty;

        public DateTime ReturnedOn { get; set; }

        [Required(ErrorMessage = "Received By is required")]
        public string ReceivedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Approved By is required")]
        public string ApprovedBy { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;
    }
}