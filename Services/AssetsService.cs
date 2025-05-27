using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class AssetsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AssetsService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AssetsService(HttpClient httpClient, ILogger<AssetsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<AssetInstance?> GetAssetInstanceAsync(int instanceId)
        {
            try
            {
                _logger.LogInformation($"Fetching asset instance with ID {instanceId} via api/AssetIssueRecords/instance/{instanceId}");
                Console.WriteLine($"Fetching asset instance with ID {instanceId}");

                var url = $"api/AssetIssueRecords/instance/{instanceId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to retrieve asset instance with ID {instanceId}. HTTP {response.StatusCode}");
                    Console.WriteLine($"HTTP error for instance {instanceId}: {response.StatusCode}");
                    return null;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance {instanceId}: {responseContent}");
                Console.WriteLine($"Raw response for instance {instanceId}: {responseContent}");

                using var stream = await response.Content.ReadAsStreamAsync();
                var assetInstance = await JsonSerializer.DeserializeAsync<AssetInstance>(stream, _jsonOptions);
                _logger.LogInformation($"Successfully retrieved asset instance with ID {instanceId}");
                return assetInstance;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve asset instance with ID {instanceId}.");
                Console.WriteLine($"HTTP error for instance {instanceId}: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize asset instance with ID {instanceId}.");
                Console.WriteLine($"Deserialization error for instance {instanceId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AssetIssueRecord>> GetAssetIssueRecordsAsync(int instanceId)
        {
            try
            {
                _logger.LogInformation($"Fetching issue records for instance ID {instanceId} via api/AssetIssueRecords?instanceId={instanceId}&include=assetinstance");
                var response = await _httpClient.GetFromJsonAsync<List<AssetIssueRecord>>(
                    $"api/AssetIssueRecords?instanceId={instanceId}&include=assetinstance",
                    _jsonOptions
                );
                return response ?? new List<AssetIssueRecord>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve issue records for instance ID {instanceId}.");
                Console.WriteLine($"Error fetching issue records for instance {instanceId}: {ex.Message}");
                return new List<AssetIssueRecord>();
            }
        }

        public async Task CreateAssetIssueRecordAsync(AssetIssueRecord record)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/AssetIssueRecords", record, _jsonOptions);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var headers = string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
                    Console.WriteLine($"POST failed: {response.StatusCode} - {errorContent}");
                    Console.WriteLine($"Response headers: {headers}");
                    throw new HttpRequestException($"Failed to create asset issue record: {response.StatusCode} - {errorContent}");
                }
                Console.WriteLine("POST succeeded.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create issue record.");
                throw new Exception("Failed to create issue record.", ex);
            }
        }

        public async Task<bool> UnAssignAssetIssueRecordAsync(UnassignRequest request)
        {
            try
            {
                _logger.LogInformation($"Unassigning issue record for instance ID {request.AssetInstanceId} via api/AssetIssueRecords/unassign");
                var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords/unassign", request, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for unassign: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to unassign issue record for instance ID {request.AssetInstanceId}. HTTP {response.StatusCode}, Content: {responseContent}");
                    return false;
                }

                _logger.LogInformation($"Successfully unassigned issue record for instance ID {request.AssetInstanceId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unassign issue record for instance ID {request.AssetInstanceId}. Exception: {ex.Message}");
                return false;
            }
        }

        // Removed UpdateInstanceStatusAsync since it's not used anymore
    }
}