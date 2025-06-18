 
using System;

namespace WiseHR.Models
{
    public class AssetReturnRecord
    {
        public int Id { get; set; }
        public int AssetInstanceId { get; set; }
        public string ReturnedBy { get; set; } = string.Empty;
        public DateTime ReturnedOn { get; set; }
        public string ReceivedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public AssetInstance? AssetInstance { get; set; }
    }
}