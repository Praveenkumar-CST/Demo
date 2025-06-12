using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WiseHR.Models;
using System.Net.Http.Headers;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace WiseHR.Services
{
    public interface IChatHistoryService
    {
        Task<List<ChatMessage>> GetUserHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ChatMessage>> GetSessionHistoryAsync(Guid sessionId);
        Task DeleteSessionAsync(Guid sessionId);
    }

    public class ChatHistoryService : IChatHistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<ChatHistoryService> _logger;

        public ChatHistoryService(HttpClient httpClient, IJSRuntime jsRuntime, ILogger<ChatHistoryService> logger)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        private async Task<string?> GetAccessToken()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token found for chat history");
                    return null;
                }
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving access token");
                return null;
            }
        }

        private ChatMessage ProcessMessage(DatabaseChatMessage dbMessage)
        {
            return new ChatMessage
            {
                Content = dbMessage.Content,
                IsFromUser = dbMessage.MessageType == "User",
                Timestamp = dbMessage.Timestamp,
                Status = MessageStatus.Read, // History messages are always read
                SessionId = dbMessage.SessionId,
                IsTable = IsContentTable(dbMessage.Content)
            };
        }

        private bool IsContentTable(string content)
        {
            return content.Contains("Successfully executed get_assets") || 
                   (content.Contains("Asset Tag Prefix:") && content.Contains("Total Instances:"));
        }

        public async Task<List<ChatMessage>> GetUserHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token available for chat history request");
                    return new List<ChatMessage>();
                }

                var queryParams = new List<string>();
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"api/ChatHistory/user{queryString}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get user chat history: {StatusCode}", response.StatusCode);
                    return new List<ChatMessage>();
                }

                var dbMessages = await response.Content.ReadFromJsonAsync<List<DatabaseChatMessage>>();
                return dbMessages?.Select(ProcessMessage).ToList() ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserHistoryAsync");
                throw;
            }
        }

        public async Task<List<ChatMessage>> GetSessionHistoryAsync(Guid sessionId)
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token available for session history request");
                    return new List<ChatMessage>();
                }

                var url = $"api/ChatHistory/session/{sessionId}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get session history: {StatusCode}", response.StatusCode);
                    return new List<ChatMessage>();
                }

                var dbMessages = await response.Content.ReadFromJsonAsync<List<DatabaseChatMessage>>();
                return dbMessages?.Select(ProcessMessage).ToList() ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSessionHistoryAsync");
                throw;
            }
        }

        public async Task DeleteSessionAsync(Guid sessionId)
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token available for delete session request");
                    return;
                }

                var url = $"api/ChatHistory/session/{sessionId}";
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to delete session: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteSessionAsync");
                throw;
            }
        }
    }

    // Database message model
    public class DatabaseChatMessage
    {
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("messageType")]
        public string MessageType { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }

        [JsonPropertyName("messageMetadata")]
        public MessageMetadata? MessageMetadata { get; set; }
    }

    public class MessageMetadata
    {
        [JsonPropertyName("policyReferences")]
        public List<object> PolicyReferences { get; set; } = new();

        [JsonPropertyName("fileReferences")]
        public List<object> FileReferences { get; set; } = new();

        [JsonPropertyName("context")]
        public Dictionary<string, object> Context { get; set; } = new();
    }
} 