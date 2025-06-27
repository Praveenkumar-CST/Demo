namespace WiseHR.Services.ChatFormatting;

public interface IChatMessageFormatter
{
    string FormatGeneralMessageContent(string content);
    string FormatGenericTableContent(string content);
    string FormatAssetTableContent(string content);
    string FormatJsonTableContent(string content);
    ChatMessageTableType GetChatMessageTableType(string content);
}

public enum ChatMessageTableType
{
    None,
    Generic,
    Asset,
    Json
} 