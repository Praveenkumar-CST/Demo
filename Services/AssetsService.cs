using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WiseHR.Models;

namespace WiseHR.Services
    {
    public class AssetsService
        {
        private readonly HttpClient _httpClient;

        public AssetsService(HttpClient httpClient)
            {
            _httpClient = httpClient;
            }

        public async Task<AssetInstance> GetAssetInstanceAsync(int instanceId)
            {
            try
                {
                var response = await _httpClient.GetAsync($"api/AssetIssueRecords/instance/{instanceId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<AssetInstance>();
                }
            catch (HttpRequestException ex)
                {
                throw new HttpRequestException($"Failed to retrieve asset instance: {ex.Message}", ex);
                }
            catch (JsonException ex)
                {
                throw new JsonException($"Failed to deserialize asset instance: {ex.Message}", ex);
                }
            }

        public async Task<List<AssetIssueRecord>> GetAssetIssueRecordsAsync(int instanceId)
            {
            try
                {
                var response = await _httpClient.GetAsync($"api/AssetIssueRecords?instanceId={instanceId}&include=assetinstance");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<AssetIssueRecord>>();
                }
            catch (HttpRequestException ex)
                {
                throw new HttpRequestException($"Failed to retrieve issue records: {ex.Message}", ex);
                }
            catch (JsonException ex)
                {
                throw new JsonException($"Failed to deserialize issue records: {ex.Message}", ex);
                }
            }

        public async Task CreateAssetIssueRecordAsync(AssetIssueRecord model)
            {
            try
                {
                var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords", model);
                response.EnsureSuccessStatusCode();
                }
            catch (HttpRequestException ex)
                {
                throw new HttpRequestException($"Failed to create issue record: {ex.Message}", ex);
                }
            }

        public async Task<bool> UnAssignAssetIssueRecordAsync(AssetIssueRecord.UnassignRequest model)
            {
            try
                {
                var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords/unassign", model);
                response.EnsureSuccessStatusCode();
                return true;
                }
            catch (HttpRequestException ex)
                {
                return false;
                }
            }

        public async Task<DecommissionRecord?> GetDecommissionRecordAsync(string assetTag)
            {
            try
                {
                var response = await _httpClient.GetAsync($"api/assets/decommission/{Uri.EscapeDataString(assetTag)}");
                if (response.IsSuccessStatusCode)
                    {
                    return await response.Content.ReadFromJsonAsync<DecommissionRecord>();
                    }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                    return null;
                    }
                throw new HttpRequestException($"Failed to fetch decommission record: {response.ReasonPhrase}");
                }
            catch (HttpRequestException ex)
                {
                throw new HttpRequestException($"Failed to retrieve decommission record: {ex.Message}", ex);
                }
            catch (JsonException ex)
                {
                throw new JsonException($"Failed to deserialize decommission record: {ex.Message}", ex);
                }
            }
        }
    }