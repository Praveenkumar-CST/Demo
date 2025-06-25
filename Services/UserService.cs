using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WiseHR.Models;
using WiseHR.Interfaces;

namespace WiseHR.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IJSRuntime _jsRuntime;

        public UserService(HttpClient httpClient, IMemoryCache cache, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _cache = cache;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            await EnsureAuthHeader();
            return await _cache.GetOrCreateAsync("AllUsers", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return await _httpClient.GetFromJsonAsync<List<User>>("api/users") ?? new();
            });
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            await EnsureAuthHeader();
            return await _cache.GetOrCreateAsync($"UsersByRole_{role}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var response = await _httpClient.GetAsync($"api/users/by-role/{role}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<User>>() ?? new();
            });
        }

        public async Task UpdateUserRoleAsync(string id, string role)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}/role", role);
            response.EnsureSuccessStatusCode();
            _cache.Remove("AllUsers");
            _cache.Remove($"UsersByRole_{role}");
        }

        public async Task DeleteUserAsync(string id)
        {
            await EnsureAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            response.EnsureSuccessStatusCode();
            _cache.Remove("AllUsers");
        }

        public async Task<User> CreateUserAsync(string email, string password)
        {
            await EnsureAuthHeader();
            var request = new CreateUserRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/users", request);
            response.EnsureSuccessStatusCode();
            _cache.Remove("AllUsers");
            return await response.Content.ReadFromJsonAsync<User>() ?? throw new Exception("Failed to deserialize created user.");
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            await EnsureAuthHeader();
            return await _cache.GetOrCreateAsync($"UserByEmail_{email}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var response = await _httpClient.GetAsync($"api/users/by-email/{email}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<User>();
                }
                return null;
            });
        }

        private async Task EnsureAuthHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
