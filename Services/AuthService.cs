using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _baseUrl;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public AuthService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

            _baseUrl = configuration["ApiBaseUrl"] ?? "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/";
            _supabaseUrl = configuration["Supabase:Url"] ?? "https://xnibymvrmxhralsrggau.supabase.co";
            _supabaseKey = configuration["Supabase:AnonKey"] ?? "YOUR_DEFAULT_SUPABASE_KEY";

            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<(string? Token, string? Error)> Login(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Token != null)
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                        Console.WriteLine($"Token stored: {result.Token}");
                        return (result.Token, null);
                    }
                    return (null, "Login failed: No token received.");
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, errorContent);
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
        }

        public async Task<(string? Role, string? Error)> VerifyToken()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    return (null, "No token found.");
                }

                var (role, error) = await GetRoleFromSupabase(token);
                if (!string.IsNullOrEmpty(role))
                {
                    var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                    var normalizedRole = char.ToUpper(result?.Role[0] ?? ' ') + result?.Role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, $"Error from API: {response.StatusCode}. {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async Task<(string? Role, string? Error)> GetRoleFromSupabase(string token)
        {
            try
            {
                using var supabaseClient = new HttpClient { BaseAddress = new Uri(_supabaseUrl) };
                supabaseClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                supabaseClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);

                var userResponse = await supabaseClient.GetAsync("/auth/v1/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync();
                    return (null, $"Failed to fetch user: {errorContent}");
                }

                var userData = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
                var userId = userData?.Id;

                if (string.IsNullOrEmpty(userId))
                {
                    return (null, "User ID is null or empty.");
                }

                var roleUrl = $"/rest/v1/roles?select=role&user_id=eq.{Uri.EscapeDataString(userId)}";
                var roleResponse = await supabaseClient.GetAsync(roleUrl);
                if (roleResponse.IsSuccessStatusCode)
                {
                    var roles = await roleResponse.Content.ReadFromJsonAsync<List<Role>>();
                    var role = roles?.FirstOrDefault()?.RoleName;
                    if (string.IsNullOrEmpty(role))
                    {
                        return (null, "No role found in roles table.");
                    }
                    return (role, null);
                }

                var roleErrorContent = await roleResponse.Content.ReadAsStringAsync();
                return (null, $"Failed to fetch role: {roleErrorContent}");
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Failed to connect to Supabase: {ex.Message}");
            }
        }

        public async Task<bool> IsUserAuthorized(string requiredRole)
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                    return result?.Role == requiredRole;
                }

                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<string> Signup(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/signup", new { email, password });
                if (response.IsSuccessStatusCode)
                {
                    return "Signup successful";
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                return $"Failed to connect to the server: {ex.Message}";
            }
        }

        public async Task<(string? Message, string? Email, string? Error)> ForgotPassword(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { email });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                    return (result?.Message, result?.Email, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, null, errorContent);
            }
            catch (HttpRequestException ex)
            {
                return (null, null, $"Failed to connect to the server: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> ResetPassword(string email, string otp, string newPassword)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new { email, otp, newPassword });
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Failed to connect to the server: {ex.Message}");
            }
        }

        // -------------------------
        // DTO Classes
        // -------------------------

        private class LoginResponse
        {
            [JsonInclude]
            public string Token { get; set; } = string.Empty;

            [JsonInclude]
            public string UserId { get; set; } = string.Empty;
        }

        private class UserResponse
        {
            [JsonInclude]
            public string? Id { get; set; }

            [JsonInclude]
            public string? Name { get; set; }

            [JsonInclude]
            public string? Email { get; set; }
        }

        private class ProfileResponse
        {
            [JsonInclude]
            public string Role { get; set; } = string.Empty;
        }

        private class VerifyResponse
        {
            [JsonInclude]
            public string UserId { get; set; } = string.Empty;

            [JsonInclude]
            public string Role { get; set; } = string.Empty;
        }

        private class ForgotPasswordResponse
        {
            [JsonInclude]
            public string Message { get; set; } = string.Empty;

            [JsonInclude]
            public string Email { get; set; } = string.Empty;
        }

        private class Role
        {
            [JsonPropertyName("role")]
            [JsonInclude]
            public string RoleName { get; set; } = string.Empty;
        }
    }
}
