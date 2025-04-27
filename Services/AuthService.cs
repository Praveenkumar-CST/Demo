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
        private readonly JsonSerializerOptions _jsonOptions;

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
                Console.WriteLine($"Login attempt for email: {email}");
                HttpResponseMessage response;

                try
                {
                    var primaryJson = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Primary JSON for Login: {primaryJson}");
                    response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    Console.WriteLine($"Falling back to manual serialization for Login: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Fallback JSON for Login: {json}");
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/login", content);
                }

                Console.WriteLine($"Login response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                    if (result?.Token != null)
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                        Console.WriteLine($"Token stored: {result.Token}");
                        return (result.Token, null);
                    }
                    Console.WriteLine("Login failed: No token in response");
                    return (null, "Login failed: No token received.");
                }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in Login: {ex.Message}");
                        return (null, $"Failed to deserialize response: {ex.Message}");
                    }
                }

                Console.WriteLine($"Login failed with status {response.StatusCode}: {rawResponse}");
                return (null, rawResponse);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in Login: {ex.Message}");
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization error in Login: {ex.Message}");
                return (null, $"Failed to deserialize response: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in Login: {ex.Message}");
                return (null, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<string> Signup(string email, string password)
        {
            try
            {
                var request = new SignupRequest { Email = email, Password = password };
                Console.WriteLine($"Signup attempt for email: {email}");
                HttpResponseMessage response;

                try
                {
                    var primaryJson = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Primary JSON for Signup: {primaryJson}");
                    response = await _httpClient.PostAsJsonAsync("api/auth/signup", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    Console.WriteLine($"Falling back to manual serialization for Signup: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Fallback JSON for Signup: {json}");
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/signup", content);
                }

                Console.WriteLine($"Signup response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Signup raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Signup successful");
                    return "Signup successful";
                }

                Console.WriteLine($"Signup failed with status {response.StatusCode}: {rawResponse}");
                return rawResponse;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in Signup: {ex.Message}");
                return $"Failed to connect to the server: {ex.Message}";
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization error in Signup: {ex.Message}");
                return $"Failed to deserialize response: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in Signup: {ex.Message}");
                return $"Unexpected error: {ex.Message}";
            }
        }

        public async Task<(string? Message, string? Email, string? Error)> ForgotPassword(string email)
        {
            try
            {
                var request = new ForgotPasswordRequest { Email = email };
                Console.WriteLine($"ForgotPassword attempt for email: {email}");
                HttpResponseMessage response;

                try
                {
                    var primaryJson = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Primary JSON for ForgotPassword: {primaryJson}");
                    response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    Console.WriteLine($"Falling back to manual serialization for ForgotPassword: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Fallback JSON for ForgotPassword: {json}");
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/forgot-password", content);
                }

                Console.WriteLine($"ForgotPassword response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ForgotPassword raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(_jsonOptions);
                    Console.WriteLine($"ForgotPassword success: Message: {result?.Message}, Email: {result?.Email}");
                    return (result?.Message, result?.Email, null);
                }

                Console.WriteLine($"ForgotPassword failed with status {response.StatusCode}: {rawResponse}");
                return (null, null, rawResponse);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in ForgotPassword: {ex.Message}");
                return (null, null, $"Failed to connect to the server: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization error in ForgotPassword: {ex.Message}");
                return (null, null, $"Failed to deserialize response: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in ForgotPassword: {ex.Message}");
                return (null, null, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> ResetPassword(string email, string otp, string newPassword)
        {
            try
            {
                var request = new ResetPasswordRequest { Email = email, Otp = otp, NewPassword = newPassword };
                Console.WriteLine($"ResetPassword attempt for email: {email}");
                HttpResponseMessage response;

                try
                {
                    var primaryJson = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Primary JSON for ResetPassword: {primaryJson}");
                    response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request, _jsonOptions);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("NullabilityInfoContext"))
                {
                    Console.WriteLine($"Falling back to manual serialization for ResetPassword: {ex.Message}");
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    Console.WriteLine($"Fallback JSON for ResetPassword: {json}");
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync("api/auth/reset-password", content);
                }

                Console.WriteLine($"ResetPassword response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ResetPassword raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("ResetPassword successful");
                    return (true, null);
                }

                Console.WriteLine($"ResetPassword failed with status {response.StatusCode}: {rawResponse}");
                return (false, rawResponse);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in ResetPassword: {ex.Message}");
                return (false, $"Failed to connect to the server: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Deserialization error in ResetPassword: {ex.Message}");
                return (false, $"Failed to deserialize response: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in ResetPassword: {ex.Message}");
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        // Other methods (VerifyToken, GetRoleFromSupabase, IsUserAuthorized) remain unchanged
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
                    Console.WriteLine($"Raw role from Supabase: {role}");
                    var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

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
                Console.WriteLine($"HTTP error in IsUserAuthorized: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in IsUserAuthorized: {ex.Message}");
                return false;
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
}

namespace WiseHR.Models
{
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