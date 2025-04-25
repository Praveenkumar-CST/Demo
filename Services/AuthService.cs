using Microsoft.JSInterop;
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

        private readonly string _supabaseUrl = "https://xnibymvrmxhralsrggau.supabase.co";
        private readonly string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhuaWJ5bXZybXhocmFsc3JnZ2F1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDIzMjIyMDQsImV4cCI6MjA1Nzg5ODIwNH0.DdJTJxUy7FUj0Z4U_JTPqGCg32Sd6mROPhNCCR8x35U";

        public AuthService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

            //_baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5243";

            _baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5243";

            _supabaseUrl = configuration["Supabase:Url"] ?? _supabaseUrl;
            _supabaseKey = configuration["Supabase:AnonKey"] ?? _supabaseKey;
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

                // Try to get role from Supabase first
                var(role, error) = await GetRoleFromSupabase(token);
                if (!string.IsNullOrEmpty(role))
                {
                    Console.WriteLine($"Raw role from Supabase: {role}");
                    var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                // Now verify token via the API
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");

                if (response.IsSuccessStatusCode)
                {
                var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                Console.WriteLine($"Raw role from API: {result?.Role}");
                var normalizedRole = char.ToUpper(result?.Role[0] ?? ' ') + result?.Role.Substring(1).ToLower();
                return (normalizedRole, null);
                }

                // Log the response error details if status code isn't 2xx
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Content: {errorContent}");
                return (null, $"Error from API: {response.StatusCode}. {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
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

                var userData = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
                var userId = userData?.Id;  // Get user ID from Supabase Auth

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
                    var roles = await roleResponse.Content.ReadFromJsonAsync<List<Role>>();
                    var role = roles?.FirstOrDefault()?.RoleName; // No default role
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
                    return false; // User is not authenticated
                }

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                    if (result?.Role == requiredRole)
                    {
                        return true; // User has the required role
                    }
                }

                return false; // User is either not authorized or does not have the required role
            }
            catch (HttpRequestException ex)
            {
                return false; // Connection failed, not authorized
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
    }
}