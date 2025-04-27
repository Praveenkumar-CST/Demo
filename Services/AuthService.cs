using Microsoft.JSInterop;
using MongoDB.Bson;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _supabaseUrl = "https://xnibymvrmxhralsrggau.supabase.co";
        private readonly string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhuaWJ5bXZybXhocmFsc3JnZ2F1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDIzMjIyMDQsImV4cCI6MjA1Nzg5ODIwNH0.DdJTJxUy7FUj0Z4U_JTPqGCg32Sd6mROPhNCCR8x35U";

        public AuthService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _baseUrl = configuration["ApiBaseUrl"] ?? "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/";
            _supabaseUrl = configuration["Supabase:Url"] ?? _supabaseUrl;
            _supabaseKey = configuration["Supabase:AnonKey"] ?? _supabaseKey;
            _httpClient.BaseAddress = new Uri(_baseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<(string? Token, string? Error)> Login(string email, string password)
        {
            try
            {
                var request = new LoginRequest { Email = email, Password = password };
                HttpResponseMessage response;

                // Primary approach: Use PostAsJsonAsync
                try
                {
                    response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    // Fallback: Manual serialization with StringContent
                    Console.WriteLine($"Falling back to manual serialization for Login: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/login", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
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

                // Try to get role from Supabase first
                var (role, error) = await GetRoleFromSupabase(token);
                if (!string.IsNullOrEmpty(role))
                {
                    Console.WriteLine($"Raw role from Supabase: {role}");
                    var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                // Now verify token via the API
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>(_jsonOptions);
                    Console.WriteLine($"Raw role from API: {result?.Role}");
                    var normalizedRole = char.ToUpper(result?.Role[0] ?? ' ') + result?.Role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Content: {errorContent}");
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

                // Step 1: Get user details from Supabase Auth
                var userResponse = await supabaseClient.GetAsync("/auth/v1/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching user from Supabase: Status {userResponse.StatusCode}, Content: {errorContent}");
                    return (null, $"Failed to fetch user: {errorContent}");
                }

                var userData = await userResponse.Content.ReadFromJsonAsync<UserResponse>(_jsonOptions);
                var userId = userData?.Id;

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("Error: User ID is null or empty after fetching user data.");
                    return (null, "User ID is null or empty.");
                }
                Console.WriteLine($"User ID from Supabase: {userId}");

                // Step 2: Fetch role from roles table using user_id
                var roleUrl = $"/rest/v1/roles?select=role&user_id=eq.{Uri.EscapeDataString(userId)}";
                var roleResponse = await supabaseClient.GetAsync(roleUrl);
                if (roleResponse.IsSuccessStatusCode)
                {
                    var roles = await roleResponse.Content.ReadFromJsonAsync<List<Role>>(_jsonOptions);
                    var role = roles?.FirstOrDefault()?.RoleName;
                    if (string.IsNullOrEmpty(role))
                    {
                        Console.WriteLine($"No role found for user ID: {userId}");
                        return (null, "No role found in roles table.");
                    }
                    Console.WriteLine($"Role fetched successfully for user ID {userId}: {role}");
                    return (role, null);
                }

                var roleErrorContent = await roleResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Error fetching role from Supabase: Status {roleResponse.StatusCode}, Content: {roleErrorContent}");
                return (null, $"Failed to fetch role: {roleErrorContent}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request error connecting to Supabase: {ex.Message}");
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
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>(_jsonOptions);
                    if (result?.Role == requiredRole)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (HttpRequestException ex)
            {
                return false;
            }
        }

        public async Task<string> Signup(string email, string password)
        {
            try
            {
                var request = new SignupRequest { Email = email, Password = password };
                HttpResponseMessage response;

                // Primary approach: Use PostAsJsonAsync
                try
                {
                    response = await _httpClient.PostAsJsonAsync("api/auth/signup", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    // Fallback: Manual serialization with StringContent
                    Console.WriteLine($"Falling back to manual serialization for Signup: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/signup", content);
                }

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
                var request = new ForgotPasswordRequest { Email = email };
                HttpResponseMessage response;

                // Primary approach: Use PostAsJsonAsync
                try
                {
                    response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    // Fallback: Manual serialization with StringContent
                    Console.WriteLine($"Falling back to manual serialization for ForgotPassword: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/forgot-password", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(_jsonOptions);
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
                var request = new ResetPasswordRequest { Email = email, Otp = otp, NewPassword = newPassword };
                HttpResponseMessage response;

                // Primary approach: Use PostAsJsonAsync
                try
                {
                    response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    // Fallback: Manual serialization with StringContent
                    Console.WriteLine($"Falling back to manual serialization for ResetPassword: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/reset-password", content);
                }

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

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        private class UserResponse
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
        }

        private class ProfileResponse
        {
            public string Role { get; set; } = string.Empty;
        }

        private class VerifyResponse
        {
            public string UserId { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private class ForgotPasswordResponse
        {
            public string Message { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        private class Role
        {
            public string RoleName { get; set; } = string.Empty;
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}