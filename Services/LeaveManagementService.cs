using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class LeaveManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public LeaveManagementService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                throw new InvalidOperationException("No JWT token found in localStorage.");
            }
        }

        public async Task<(string Name, string Email, string Role)> GetUserClaimsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var name = user.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";

            return (name, email, role);
        }

        public async Task<SubmitLeaveResponse> SubmitLeaveRequestAsync(LeaveRequestDto request)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/LeaveManagement/request", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to submit leave request. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<SubmitLeaveResponse>();
        }

        public async Task<List<LeaveRequest>> GetPendingRequestsAsync()
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("/api/LeaveManagement/requests");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get pending requests. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<List<LeaveRequest>>();
        }

        public async Task<MyHistoryResponse> GetMyLeaveHistoryAsync()
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("/api/LeaveManagement/my-history");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get leave history. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<MyHistoryResponse>();
        }

        public async Task<MyHistoryResponse> GetUserHistoryAsync(UserHistoryRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.UserId) &&
                string.IsNullOrEmpty(request.EmployeeId) && string.IsNullOrEmpty(request.Email))
            {
                throw new ArgumentException("At least one search parameter (Name, UserId, EmployeeId, or Email) is required.");
            }

            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(request.Name)) query["name"] = request.Name;
            if (!string.IsNullOrEmpty(request.UserId)) query["userId"] = request.UserId;
            if (!string.IsNullOrEmpty(request.EmployeeId)) query["employeeId"] = request.EmployeeId;
            if (!string.IsNullOrEmpty(request.Email)) query["email"] = request.Email;
            query["pageNumber"] = request.PageNumber.ToString();
            query["pageSize"] = request.PageSize.ToString();

            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"/api/LeaveManagement/history?{query}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get user history. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<MyHistoryResponse>();
        }

        public async Task<List<LeaveRequest>> GetRequestStatusAsync()
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("/api/LeaveManagement/request-status");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get request status. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<List<LeaveRequest>>();
        }

        public async Task<ApproveRejectResponse> ApproveRejectRequestAsync(ApproveRejectDto request)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("/api/LeaveManagement/approve-reject", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to approve/reject request. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<ApproveRejectResponse>();
        }

        public async Task<QuotaResponse> ManageDefaultLeaveQuotaAsync(DefaultLeaveQuotaDto request)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("/api/LeaveManagement/default-quota", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to manage default leave quota. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<QuotaResponse>();
        }

        public async Task<QuotaResponse> ManageIndividualLeaveQuotaAsync(IndividualLeaveQuotaDto request)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("/api/LeaveManagement/individual-quota", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to manage individual leave quota. Status: {response.StatusCode}, Reason: {errorContent}");
            }
            return await response.Content.ReadFromJsonAsync<QuotaResponse>();
        }
    }
}