using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WiseHR.Models;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace WiseHR.Services
    {
    public class AssetStateService
        {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AssetStateService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AssetStateService(HttpClient httpClient, ILogger<AssetStateService> logger)
            {
            _httpClient = httpClient;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
                {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
            }

        public async Task<List<Asset>> GetAssetsAsync()
            {
            try
                {
                _logger.LogInformation("Fetching assets from api/Assets");
                Console.WriteLine("Fetching assets from api/Assets");

                var response = await _httpClient.GetAsync("api/Assets");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw JSON from API: {json}", json);
                Console.WriteLine($"Raw JSON from API: {json}");

                if (string.IsNullOrWhiteSpace(json))
                    {
                    _logger.LogWarning("Received empty JSON response from api/Assets");
                    return new List<Asset>();
                    }

                var assets = JsonSerializer.Deserialize<List<Asset>>(json, _jsonOptions);
                return assets ?? new List<Asset>();
                }
            catch (HttpRequestException ex)
                {
                Console.WriteLine($"Exception when fetching assets: {ex.Message}");
                _logger.LogError(ex, "Failed to retrieve assets from the server.");
                throw new Exception("Failed to retrieve assets from the server.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, "Failed to deserialize the JSON response.");
                throw new Exception("Failed to deserialize the JSON response.", ex);
                }
            }

        public async Task<Asset?> GetAssetAsync(int id)
            {
            try
                {
                Console.WriteLine($"Fetching asset with ID {id}");
                _logger.LogInformation($"Fetching asset with ID {id} from api/Assets/{id}");
                var response = await _httpClient.GetAsync($"api/Assets/{id}");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON for asset ID {id}: {json}");
                Console.WriteLine($"Raw JSON for asset ID {id}: {json}");

                if (string.IsNullOrWhiteSpace(json))
                    {
                    _logger.LogWarning($"Received empty JSON response for asset ID {id}");
                    return null;
                    }

                var asset = JsonSerializer.Deserialize<Asset>(json, _jsonOptions);
                return asset;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to retrieve asset with ID {id}.");
                throw new Exception($"Failed to retrieve asset with ID {id}.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Failed to deserialize asset with ID {id}.");
                throw new Exception($"Failed to deserialize asset with ID {id}.", ex);
                }
            }

        public async Task<AssetInstance?> GetAssetInstanceAsync(int instanceId)
            {
            try
                {
                _logger.LogInformation($"Fetching asset instance with ID {instanceId} from api/AssetIssueRecords?instanceId={instanceId}&include=STRING");
                Console.WriteLine($"Fetching asset instance with ID {instanceId}");

                var url = $"api/AssetIssueRecords?instanceId={instanceId}&include=STRING";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogWarning($"Failed to retrieve asset instance with ID {instanceId}. HTTP {response.StatusCode}");
                    Console.WriteLine($"HTTP error for instance {instanceId}: {response.StatusCode}");
                    return null;
                    }

                using var stream = await response.Content.ReadAsStreamAsync();
                var instance = await JsonSerializer.DeserializeAsync<AssetInstance>(stream, _jsonOptions);
                _logger.LogInformation($"Successfully retrieved asset instance with ID {instanceId}");
                return instance;
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

        public async Task<Asset> CreateAssetAsync(CreateAssetDto assetDto)
            {
            try
                {
                _logger.LogInformation("Creating new asset via api/Assets");
                var jsonPayload = JsonSerializer.Serialize(assetDto, _jsonOptions);
                _logger.LogDebug($"Sending CreateAssetDto: {jsonPayload}");
                Console.WriteLine($"Sending CreateAssetDto: {jsonPayload}");

                var response = await _httpClient.PostAsJsonAsync("api/Assets", assetDto, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for asset creation: HTTP {response.StatusCode}, Content: {responseContent}");
                Console.WriteLine($"Raw response for asset creation: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to create asset: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to create asset: HTTP {response.StatusCode} - {responseContent}");
                    }

                if (string.IsNullOrWhiteSpace(responseContent))
                    {
                    _logger.LogWarning("Asset creation succeeded, but response body was empty.");
                    throw new Exception("Asset creation succeeded, but response body was empty.");
                    }

                var asset = JsonSerializer.Deserialize<Asset>(responseContent, _jsonOptions);
                if (asset == null)
                    {
                    _logger.LogError("Asset creation succeeded, but deserialized asset was null.");
                    throw new Exception("Asset creation succeeded, but deserialized asset was null.");
                    }

                _logger.LogInformation($"Created asset with ID {asset.Id}");
                return asset;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, "Failed to create asset due to a network error.");
                throw new Exception("Failed to create asset due to a network error.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, "Failed to deserialize asset creation response.");
                throw new Exception("Failed to deserialize asset creation response.", ex);
                }
            }

        public async Task<AssetInstance> CreateInstanceAsync(int assetId, CreateInstanceDto instanceDto)
            {
            try
                {
                _logger.LogInformation($"Creating new instance for asset ID {assetId} via api/Assets/{assetId}/instances");
                var jsonPayload = JsonSerializer.Serialize(instanceDto, _jsonOptions);
                _logger.LogDebug($"Sending CreateInstanceDto: {jsonPayload}");
                Console.WriteLine($"Sending CreateInstanceDto: {jsonPayload}");

                var response = await _httpClient.PostAsJsonAsync($"api/Assets/{assetId}/instances", instanceDto, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance creation: HTTP {response.StatusCode}, Content: {responseContent}");
                Console.WriteLine($"Raw response for instance creation: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to create instance: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to create instance: HTTP {response.StatusCode} - {responseContent}");
                    }

                if (string.IsNullOrWhiteSpace(responseContent))
                    {
                    _logger.LogWarning("Instance creation succeeded, but response body was empty.");
                    throw new Exception("Instance creation succeeded, but response body was empty.");
                    }

                var instance = JsonSerializer.Deserialize<AssetInstance>(responseContent, _jsonOptions);
                if (instance == null)
                    {
                    _logger.LogError("Instance creation succeeded, but deserialized instance was null.");
                    throw new Exception("Instance creation succeeded, but deserialized instance was null.");
                    }

                _logger.LogInformation($"Created instance with ID {instance.Id} for asset ID {assetId}");
                Console.WriteLine($"Created instance with ID: {instance.Id}");

                return instance;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, "Failed to create instance due to a network error.");
                throw new Exception("Failed to create instance due to a network error.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, "Failed to deserialize instance creation response.");
                throw new Exception("Failed to deserialize instance creation response.", ex);
                }
            }

        public async Task UpdateAssetAsync(int id, UpdateAssetDto assetDto)
            {
            try
                {
                _logger.LogInformation($"Updating asset with ID {id} via api/Assets/{id}");
                var response = await _httpClient.PutAsJsonAsync($"api/Assets/{id}", assetDto, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for asset update: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to update asset: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to update asset: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Updated asset with ID {id}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to update asset with ID {id} due to a network error.");
                throw new Exception($"Failed to update asset with ID {id} due to a network error.", ex);
                }
            }

        public async Task UpdateAssetInstanceAsync(AssetInstance instance)
            {
            try
                {
                _logger.LogInformation($"Updating instance with ID {instance.Id} via api/AssetInstances/{instance.Id}");
                var response = await _httpClient.PutAsJsonAsync($"api/AssetInstances/{instance.Id}", instance, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance update: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to update instance: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to update instance: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Updated instance with ID {instance.Id}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to update instance with ID {instance.Id} due to a network error.");
                throw new Exception($"Failed to update instance with ID {instance.Id} due to a network error.", ex);
                }
            }

        public async Task UpdateInstanceAsync(int id, UpdateInstanceDto instanceDto)
            {
            try
                {
                _logger.LogInformation($"Updating instance with ID {id} via api/Assets/instances/{id}");
                var response = await _httpClient.PutAsJsonAsync($"api/Assets/instances/{id}", instanceDto, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance update: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to update instance: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to update instance: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Updated instance with ID {id}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to update instance with ID {id} due to a network error.");
                throw new Exception($"Failed to update instance with ID {id} due to a network error.", ex);
                }
            }

        public async Task UpdateInstanceStatusAsync(int instanceId, string status)
            {
            try
                {
                _logger.LogInformation($"Updating status of instance with ID {instanceId} to {status} via api/AssetInstances/{instanceId}/status");
                var response = await _httpClient.PutAsync($"api/AssetInstances/{instanceId}/status?status={status}", null);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance status update: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to update instance status: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to update instance status: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Updated instance status with ID {instanceId} to {status}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to update instance status with ID {instanceId} due to a network error.");
                throw new Exception($"Failed to update instance status with ID {instanceId} due to a network error.", ex);
                }
            }

        public async Task DecommissionInstanceAsync(int instanceId, DecommissionDto decommissionDto)
            {
            try
                {
                _logger.LogInformation($"Decommissioning instance with ID {instanceId} via api/Assets/instances/{instanceId}/decommission");

                // Convert SIT time back to UTC for API
                if (decommissionDto.DecommissionDate.HasValue)
                    {
                    decommissionDto.DecommissionDate = decommissionDto.DecommissionDate.Value.ToUniversalTime();
                    }

                var jsonPayload = JsonSerializer.Serialize(decommissionDto, _jsonOptions);
                _logger.LogDebug($"Sending DecommissionDto: {jsonPayload}");
                Console.WriteLine($"Sending DecommissionDto: {jsonPayload}");

                var response = await _httpClient.PostAsJsonAsync($"api/Assets/instances/{instanceId}/decommission", decommissionDto, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance decommissioning: HTTP {response.StatusCode}, Content: {responseContent}");
                Console.WriteLine($"Raw response for instance decommissioning: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to decommission instance: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to decommission instance: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Decommissioned instance with ID {instanceId}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to decommission instance with ID {instanceId} due to a network error.");
                throw new Exception($"Failed to decommission instance with ID {instanceId} due to a network error.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Failed to process decommission response for instance ID {instanceId}.");
                throw new Exception($"Failed to process decommission response for instance ID {instanceId}.", ex);
                }
            }

        public async Task DeleteAssetAsync(int id)
            {
            try
                {
                _logger.LogInformation($"Deleting asset with ID {id} via api/Assets/{id}");
                var response = await _httpClient.DeleteAsync($"api/Assets/{id}");
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for asset deletion: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to delete asset: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to delete asset: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Deleted asset with ID {id}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to delete asset with ID {id} due to a network error.");
                throw new Exception($"Failed to delete asset with ID {id} due to a network error.", ex);
                }
            }

        public async Task DeleteInstanceAsync(int id)
            {
            try
                {
                _logger.LogInformation($"Deleting instance with ID {id} via api/Assets/instances/{id}");
                var response = await _httpClient.DeleteAsync($"api/Assets/instances/{id}");
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for instance deletion: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to delete instance: HTTP {response.StatusCode} - {responseContent}");
                    throw new Exception($"Failed to delete instance: HTTP {response.StatusCode} - {responseContent}");
                    }
                _logger.LogInformation($"Deleted instance with ID {id}");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to delete instance with ID {id} due to a network error.");
                throw new Exception($"Failed to delete instance with ID {id} due to a network error.", ex);
                }
            }

        public async Task<int> GetAssetCountAsync(string product)
            {
            try
                {
                _logger.LogInformation($"Fetching asset count for product '{product}' via api/Assets/count");
                var response = await _httpClient.GetAsync($"api/Assets/count?product={Uri.EscapeDataString(product)}");
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for asset count: {responseContent}");

                var count = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
                _logger.LogInformation($"Retrieved asset count: {count} for product '{product}'");
                return count;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to retrieve asset count for product '{product}'.");
                throw new Exception($"Failed to retrieve asset count for product '{product}'.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Failed to deserialize asset count for product '{product}'.");
                throw new Exception($"Failed to deserialize asset count for product '{product}'.", ex);
                }
            }

        public async Task<List<AssetIssueRecord>?> GetAssetIssueRecordsAsync(int instanceId)
            {
            try
                {
                _logger.LogInformation($"Fetching issue records for instance ID {instanceId} from api/AssetIssueRecords?instanceId={instanceId}&include=assetinstance");
                Console.WriteLine($"Fetching issue records for instance ID {instanceId}");

                var response = await _httpClient.GetAsync($"api/AssetIssueRecords?instanceId={instanceId}&include=assetinstance");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON for issue records of instance ID {instanceId}: {json}");
                Console.WriteLine($"Raw JSON for issue records of instance ID {instanceId}: {json}");

                if (string.IsNullOrWhiteSpace(json))
                    {
                    _logger.LogWarning($"Received empty JSON response for issue records of instance ID {instanceId}");
                    return new List<AssetIssueRecord>();
                    }

                var records = JsonSerializer.Deserialize<List<AssetIssueRecord>>(json, _jsonOptions);

                if (records != null)
                    {
                    for (int i = 0; i < records.Count; i++)
                        {
                        if (records[i].Sno == 0)
                            {
                            records[i].Sno = i + 1;
                            }
                        }
                    }
                return records ?? new List<AssetIssueRecord>();
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to retrieve issue records for instance ID {instanceId}.");
                Console.WriteLine($"HTTP error for issue records of instance {instanceId}: {ex.Message}");
                return new List<AssetIssueRecord>();
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Failed to deserialize issue records for instance ID {instanceId}.");
                Console.WriteLine($"Deserialization error for issue records of instance {instanceId}: {ex.Message}");
                return new List<AssetIssueRecord>();
                }
            }

        public async Task CreateAssetIssueRecordAsync(AssetIssueRecord record)
            {
            try
                {
                _logger.LogInformation($"Creating issue record for instance ID {record.AssetInstanceId} via api/AssetIssueRecords");
                var jsonPayload = JsonSerializer.Serialize(record, _jsonOptions);
                _logger.LogDebug($"Sending AssetIssueRecord: {jsonPayload}");
                Console.WriteLine($"Sending AssetIssueRecord: {jsonPayload}");

                var response = await _httpClient.PostAsJsonAsync($"api/AssetIssueRecords", record, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for issue record creation: HTTP {response.StatusCode}, Content: {responseContent}");
                Console.WriteLine($"Raw response for issue record creation: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    var headers = string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
                    _logger.LogError($"Failed to create issue record: HTTP {response.StatusCode} - {responseContent}, Headers: {headers}");
                    Console.WriteLine($"POST failed: {response.StatusCode} - {responseContent}");
                    Console.WriteLine($"Response headers: {headers}");
                    throw new HttpRequestException($"Failed to create asset issue record: {response.StatusCode} - {responseContent}");
                    }

                _logger.LogInformation($"Created issue record for instance ID {record.AssetInstanceId}");
                Console.WriteLine("POST succeeded.");
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, "Failed to create issue record due to a network error.");
                throw new Exception("Failed to create issue record due to a network error.", ex);
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, "Failed to process issue record creation response.");
                throw new Exception("Failed to process issue record creation response.", ex);
                }
            }

        public async Task<bool> HasActiveIssueRecordAsync(int instanceId)
            {
            try
                {
                _logger.LogInformation($"Checking active issue records for instance ID {instanceId} from api/AssetIssueRecords/is-issued/{instanceId}");
                Console.WriteLine($"Checking active issue records for instance ID {instanceId}");

                var response = await _httpClient.GetAsync($"api/AssetIssueRecords/is-issued/{instanceId}");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw JSON for active issue records check of instance ID {instanceId}: {json}");
                Console.WriteLine($"Raw JSON for active issue records check of instance ID {instanceId}: {json}");

                if (string.IsNullOrWhiteSpace(json))
                    {
                    _logger.LogWarning($"Received empty JSON response for active issue records check of instance ID {instanceId}");
                    return false;
                    }

                var hasActive = JsonSerializer.Deserialize<bool>(json, _jsonOptions);
                _logger.LogInformation($"Instance ID {instanceId} has active issue record: {hasActive}");
                return hasActive;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Failed to check active issue records for instance ID {instanceId}.");
                Console.WriteLine($"HTTP error for active issue records check of instance {instanceId}: {ex.Message}");
                return false;
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Failed to deserialize active issue records check for instance ID {instanceId}.");
                Console.WriteLine($"Deserialization error for active issue records check of instance {instanceId}: {ex.Message}");
                return false;
                }
            }

        public async Task<bool> UnassignAssetIssueRecordAsync(AssetIssueRecord.UnassignRequest request)
            {
            try
                {
                _logger.LogInformation($"Unassigning issue record for instance ID {request.AssetInstanceId} via api/AssetIssueRecords/unassign");
                var jsonPayload = JsonSerializer.Serialize(request, _jsonOptions);
                _logger.LogDebug($"Sending UnassignRequest: {jsonPayload}");
                Console.WriteLine($"Sending UnassignRequest: {jsonPayload}");

                var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords/unassign", request, _jsonOptions);
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Raw response for issue record unassign: HTTP {response.StatusCode}, Content: {responseContent}");
                Console.WriteLine($"Raw response for issue record unassign: HTTP {response.StatusCode}, Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    {
                    _logger.LogError($"Failed to unassign issue record: HTTP {response.StatusCode} - {responseContent}");
                    Console.WriteLine($"Unassign failed: HTTP {response.StatusCode} - {responseContent}");
                    return false;
                    }

                _logger.LogInformation($"Successfully unassigned issue record for instance ID {request.AssetInstanceId}");
                Console.WriteLine($"Successfully unassigned issue record for instance ID {request.AssetInstanceId}");
                return true;
                }
            catch (HttpRequestException ex)
                {
                _logger.LogError(ex, $"Network error unassigning issue record for instance ID {request.AssetInstanceId}: {ex.Message}");
                Console.WriteLine($"Network error unassigning instance {request.AssetInstanceId}: {ex.Message}");
                return false;
                }
            catch (JsonException ex)
                {
                _logger.LogError(ex, $"Deserialization error unassigning issue record for instance ID {request.AssetInstanceId}: {ex.Message}");
                Console.WriteLine($"Deserialization error unassigning instance {request.AssetInstanceId}: {ex.Message}");
                return false;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, $"Unexpected error unassigning issue record for instance ID {request.AssetInstanceId}: {ex.Message}");
                Console.WriteLine($"Unexpected error unassigning instance {request.AssetInstanceId}: {ex.Message}");
                return false;
                }
            }
        }
    }