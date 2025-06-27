using System;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace WiseHR.Models
{
    public class PolicyDto
    {
        public int PolicyId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
    }

    public class PolicyUploadDto
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public IBrowserFile File { get; set; }
        public string Category { get; set; }
    }

    public class PolicyResponseDto
    {
        public int PolicyId { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }
} 