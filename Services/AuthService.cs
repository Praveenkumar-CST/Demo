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

        private readonly string _supabaseUrl = "https://fhhnmffpdcyktnjahnwq.supabase.co";
        private readonly string _supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZoaG5tZmZwZGN5a3RuamFobndxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzczNzA4MDAsImV4cCI6MjA1Mjk0NjgwMH0.1b7s7qJ-yyZXo9wgzq5ZlOnaSxsKRcHDZyYb9J7LU60";

        public AuthService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));


            //_baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:7028";
            //_baseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7021";
            //_baseUrl = configuration["ApiBaseUrl"] ?? "http://172.210.14.62:5000/";
          _baseUrl = configuration["ApiBaseUrl"] ?? "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/";

            _supabaseUrl = configuration["Supabase:Url"] ?? _supabaseUrl;
            _supabaseKey = configuration["Supabase:AnonKey"] ?? _supabaseKey;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<(string? Token, string? Error)> Login(string email, string password)
        {
            try
            {
                var requestBody = new { email, password };
                var serializedBody = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"Login request JSON: {serializedBody}");

                var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result == null)
                    {
                        Console.WriteLine("Failed to deserialize LoginResponse.");
                    }
                    else
                    {
                        Console.WriteLine($"Token: {result.Token}");
                    }
                    //if (result?.Token != null)
                    //{
                    //    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                    //    Console.WriteLine($"Token stored: {result.Token}");
                    //    return (result.Token, null);
                    //}
                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                        Console.WriteLine($"Token stored: {result.Token}");
                        return (result.Token, null);
                    }
                    else
                    {
                        Console.WriteLine("Failed to store token: Token is null or empty.");
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

        public async Task<(string message, string error)> Signup(string email, string password)
        {
            try
            {
                // Save to MongoDB
                var mongoResponse = await _httpClient.PostAsJsonAsync("api/auth/signup", new { email, password });

                if (mongoResponse.IsSuccessStatusCode)
                {
                    return ("Signup successful", null);
                }

                var mongoError = await mongoResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"MongoDB error: {mongoError}");

                return ("MongoDB insert failed", mongoError);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                return ("Connection error", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return ("Unexpected error", ex.Message);
            }
        }



        // Add this class to handle the signup response
        private class SignupResponse
        {
            public UserResponse? User { get; set; }
            public string? AccessToken { get; set; }
        }

        public async Task<(string? Message, string? Email, string? Error)> ForgotPassword(string email)
        {
            try
            {
                var requestBody = new { email };
                var serializedBody = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"ForgotPassword request JSON: {serializedBody}");

                var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/forgot-password", content);

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
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userRole", normalizedRole);

                    return (normalizedRole, null);
                }
                return (null, $"Failed to fetch role from Supabase. Error: {error ?? "Unknown error"}");

                // Now verify token via the API
                //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                //var response = await _httpClient.GetAsync("api/auth/verify");

                //if (response.IsSuccessStatusCode)
                //{
                //    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                //    Console.WriteLine($"Raw role from API: {result?.Role}");
                //    var normalizedRole = char.ToUpper(result?.Role[0] ?? ' ') + result?.Role.Substring(1).ToLower();
                //    return (normalizedRole, null);
                //}

                //// Log the response error details if status code isn't 2xx
                //var errorContent = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"Error Content: {errorContent}");
                //return (null, $"Error from API: {response.StatusCode}. {errorContent}");
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
            public string? Token { get; set; }
            public string? UserId { get; set; }
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