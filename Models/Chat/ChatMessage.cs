using System;

namespace WiseHR.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public bool IsTable { get; set; } = false;
    }

    public enum MessageStatus
    {
        Sending,
        Sent,
        Read,
        Failed
    }
} 