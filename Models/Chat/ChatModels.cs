using System.Collections.Generic;
using System;
using WiseHR.Services.ChatFormatting;

namespace WiseHR.Models.Chat
{
    public class ChatSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public ChatMessageTableType TableType { get; set; } = ChatMessageTableType.None;
    }

    public enum MessageStatus
    {
        Sending,
        Sent,
        Read,
        Failed
    } tejas
} 
