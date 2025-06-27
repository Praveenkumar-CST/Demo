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
            return await _httpClient.GetFromJsonAsync<AssetInstance>($"api/AssetIssueRecords/instance/{instanceId}")
                ?? throw new HttpRequestException($"Failed to fetch asset instance with ID {instanceId}.");
        }

        public async Task<List<AssetIssueRecord>> GetAssetIssueRecordsAsync(int instanceId)
        {
            return await _httpClient.GetFromJsonAsync<List<AssetIssueRecord>>($"api/AssetIssueRecords?instanceId={instanceId}")
                ?? new List<AssetIssueRecord>();
        }

        public async Task<List<AssetReturnRecord>> GetAssetReturnRecordsAsync(int instanceId)
        {
            return await _httpClient.GetFromJsonAsync<List<AssetReturnRecord>>($"api/AssetIssueRecords/returns?instanceId={instanceId}")
                ?? new List<AssetReturnRecord>();
        }

        public async Task<DecommissionRecord> GetDecommissionRecordAsync(string assetTag)
        {
            return await _httpClient.GetFromJsonAsync<DecommissionRecord>($"api/DecommissionRecords/{assetTag}")
                ?? throw new HttpRequestException($"Failed to fetch decommission record for asset tag {assetTag}.");
        }

        public async Task<bool> HasActiveIssueAsync(int instanceId)
        {
            return await _httpClient.GetFromJsonAsync<bool>($"api/AssetIssueRecords/is-issued/{instanceId}");
        }

        public async Task CreateAssetIssueRecordAsync(AssetIssueRecord model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> UnAssignAssetIssueRecordAsync(AssetIssueRecord.UnassignRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords/unassign", model);
            return response.IsSuccessStatusCode;
        }

        public async Task CreateAssetReturnRecordAsync(AssetReturnRecordDto model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/AssetIssueRecords/return", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7021/EmployeeDetails/GetAllEmployees?page=1&pageSize=50&includePhotos=false");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var employees = await response.Content.ReadFromJsonAsync<List<Employee>>();
                return employees ?? new List<Employee>();
            }
            catch (JsonException ex)
            {
                var response = await _httpClient.GetAsync("https://localhost:7021/EmployeeDetails/GetAllEmployees?page=1&pageSize=50&includePhotos=false");
                var jsonString = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to deserialize employee data. JSON: {jsonString}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Failed to fetch employees: {ex.Message}");
            }
        }
    }
}