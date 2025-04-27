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
            //_baseUrl = configuration["ApiBaseUrl"] ?? "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/";
            _baseUrl = configuration["ApiBaseUrl"] ?? "https://wisehr-main-dce8e0bbg4f6djbs.eastus-01.azurewebsites.net/";
            _supabaseUrl = configuration["Supabase:Url"] ?? _supabaseUrl;
            _supabaseKey = configuration["Supabase:AnonKey"] ?? _supabaseKey;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            Console.WriteLine($"AuthService initialized with base URL: {_baseUrl}, Supabase URL: {_supabaseUrl}");
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
                    try
                    {
                        // AOT-safe JSON construction
                        var json = $$"""{"email":"{{email}}","password":"{{password}}"}""";
                        Console.WriteLine($"Fallback JSON for Signup: {json}");
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync("api/auth/signup", content);
                        Console.WriteLine($"Fallback PostAsync for Signup completed");
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"Fallback serialization failed for Signup: {fallbackEx.GetType().Name}: {fallbackEx.Message}");
                        return $"Fallback error: {fallbackEx.Message}";
                    }
                }

                Console.WriteLine($"Signup response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Signup raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonSerializer.Deserialize<LoginResponse>(rawResponse, _jsonOptions);
                        Console.WriteLine($"Signup successful, UserId: {result?.UserId}");
                        return "Signup successful";
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in Signup response: {ex.Message}");
                        return $"Failed to deserialize response: {ex.Message}";
                    }
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
                Console.WriteLine($"Unexpected error in Signup: {ex.GetType().Name}: {ex.Message}");
                return $"Unexpected error: {ex.Message}";
            }
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
                    try
                    {
                        // AOT-safe JSON construction
                        var json = $$"""{"email":"{{email}}","password":"{{password}}"}""";
                        Console.WriteLine($"Fallback JSON for Login: {json}");
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync("api/auth/login", content);
                        Console.WriteLine($"Fallback PostAsync for Login completed");
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"Fallback serialization failed for Login: {fallbackEx.GetType().Name}: {fallbackEx.Message}");
                        return (null, $"Fallback error: {fallbackEx.Message}");
                    }
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
                            Console.WriteLine($"Token stored: {result.Token}, UserId: {result.UserId}");
                            return (result.Token, null);
                        }
                        Console.WriteLine("Login failed: No token in response");
                        return (null, "Login failed: No token received.");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in Login response: {ex.Message}");
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
                Console.WriteLine($"Unexpected error in Login: {ex.GetType().Name}: {ex.Message}");
                return (null, $"Unexpected error: {ex.Message}");
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
                    try
                    {
                        // AOT-safe JSON construction
                        var json = $$"""{"email":"{{email}}"}""";
                        Console.WriteLine($"Fallback JSON for ForgotPassword: {json}");
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync("api/auth/forgot-password", content);
                        Console.WriteLine($"Fallback PostAsync for ForgotPassword completed");
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"Fallback serialization failed for ForgotPassword: {fallbackEx.GetType().Name}: {fallbackEx.Message}");
                        return (null, null, $"Fallback error: {fallbackEx.Message}");
                    }
                }

                Console.WriteLine($"ForgotPassword response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ForgotPassword raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(_jsonOptions);
                        Console.WriteLine($"ForgotPassword success: Message: {result?.Message}, Email: {result?.Email}");
                        return (result?.Message, result?.Email, null);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in ForgotPassword response: {ex.Message}");
                        return (null, null, $"Failed to deserialize response: {ex.Message}");
                    }
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
                Console.WriteLine($"Unexpected error in ForgotPassword: {ex.GetType().Name}: {ex.Message}");
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
                    try
                    {
                        // AOT-safe JSON construction
                        var json = $$"""{"email":"{{email}}","otp":"{{otp}}","newPassword":"{{newPassword}}"}""";
                        Console.WriteLine($"Fallback JSON for ResetPassword: {json}");
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync("api/auth/reset-password", content);
                        Console.WriteLine($"Fallback PostAsync for ResetPassword completed");
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"Fallback serialization failed for ResetPassword: {fallbackEx.GetType().Name}: {fallbackEx.Message}");
                        return (false, $"Fallback error: {fallbackEx.Message}");
                    }
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
                Console.WriteLine($"Unexpected error in ResetPassword: {ex.GetType().Name}: {ex.Message}");
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<(string? Role, string? Error)> VerifyToken()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("VerifyToken: No token found in localStorage");
                    return (null, "No token found.");
                }

                var (role, error) = await GetRoleFromSupabase(token);
                if (!string.IsNullOrEmpty(role))
                {
                    Console.WriteLine($"Role from Supabase: {role}");
                    var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    return (normalizedRole, null);
                }

                Console.WriteLine("VerifyToken: Falling back to API verification");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");
                Console.WriteLine($"VerifyToken response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"VerifyToken raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = await response.Content.ReadFromJsonAsync<VerifyResponse>(_jsonOptions);
                        Console.WriteLine($"Role from API: {result?.Role}, UserId: {result?.UserId}");
                        var normalizedRole = char.ToUpper(result?.Role[0] ?? ' ') + result?.Role.Substring(1).ToLower();
                        return (normalizedRole, null);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in VerifyToken: {ex.Message}");
                        return (null, $"Failed to deserialize response: {ex.Message}");
                    }
                }

                Console.WriteLine($"VerifyToken failed with status {response.StatusCode}: {rawResponse}");
                return (null, $"Error from API: {response.StatusCode}. {rawResponse}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in VerifyToken: {ex.Message}");
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in VerifyToken: {ex.GetType().Name}: {ex.Message}");
                return (null, $"Unexpected error: {ex.Message}");
            }
        }

        private async Task<(string? Role, string? Error)> GetRoleFromSupabase(string token)
        {
            try
            {
                Console.WriteLine("GetRoleFromSupabase: Starting role fetch");
                using var supabaseClient = new HttpClient { BaseAddress = new Uri(_supabaseUrl) };
                supabaseClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                supabaseClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
                supabaseClient.Timeout = TimeSpan.FromSeconds(30);

                Console.WriteLine("GetRoleFromSupabase: Fetching user from /auth/v1/user");
                var userResponse = await supabaseClient.GetAsync("/auth/v1/user");
                Console.WriteLine($"GetRoleFromSupabase: User response status: {userResponse.StatusCode}");
                var userRawResponse = await userResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"GetRoleFromSupabase: User raw response: {userRawResponse}");

                if (!userResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GetRoleFromSupabase: Failed to fetch user, status {userResponse.StatusCode}");
                    return (null, $"Failed to fetch user: {userRawResponse}");
                }

                try
                {
                    var userData = await userResponse.Content.ReadFromJsonAsync<UserResponse>(_jsonOptions);
                    var userId = userData?.Id;
                    Console.WriteLine($"GetRoleFromSupabase: User ID: {userId}");

                    if (string.IsNullOrEmpty(userId))
                    {
                        Console.WriteLine("GetRoleFromSupabase: User ID is null or empty");
                        return (null, "User ID is null or empty.");
                    }

                    var roleUrl = $"/rest/v1/roles?select=role&user_id=eq.{Uri.EscapeDataString(userId)}";
                    Console.WriteLine($"GetRoleFromSupabase: Fetching role from {roleUrl}");
                    var roleResponse = await supabaseClient.GetAsync(roleUrl);
                    Console.WriteLine($"GetRoleFromSupabase: Role response status: {roleResponse.StatusCode}");
                    var roleRawResponse = await roleResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"GetRoleFromSupabase: Role raw response: {roleRawResponse}");

                    if (roleResponse.IsSuccessStatusCode)
                    {
                        try
                        {
                            var roles = await roleResponse.Content.ReadFromJsonAsync<List<Role>>(_jsonOptions);
                            var role = roles?.FirstOrDefault()?.RoleName;
                            if (string.IsNullOrEmpty(role))
                            {
                                Console.WriteLine($"GetRoleFromSupabase: No role found for user ID: {userId}");
                                return (null, "No role found in roles table.");
                            }
                            Console.WriteLine($"GetRoleFromSupabase: Role fetched: {role}");
                            return (role, null);
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Deserialization error in GetRoleFromSupabase: {ex.Message}");
                            return (null, $"Failed to deserialize role response: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"GetRoleFromSupabase: Failed to fetch role, status {roleResponse.StatusCode}");
                    return (null, $"Failed to fetch role: {roleRawResponse}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Deserialization error in GetRoleFromSupabase: {ex.Message}");
                    return (null, $"Failed to deserialize user response: {ex.Message}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in GetRoleFromSupabase: {ex.Message}");
                return (null, $"Failed to connect to Supabase: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in GetRoleFromSupabase: {ex.GetType().Name}: {ex.Message}");
                return (null, $"Unexpected error: {ex.Message}");
            }
        }

        public async Task<bool> IsUserAuthorized(string requiredRole)
        {
            try
            {
                Console.WriteLine($"IsUserAuthorized: Checking for role: {requiredRole}");
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                Console.WriteLine($"IsUserAuthorized: Token found: {token != null}");
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("IsUserAuthorized: No token found");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");
                Console.WriteLine($"IsUserAuthorized response status: {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"IsUserAuthorized raw response: {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = await response.Content.ReadFromJsonAsync<VerifyResponse>(_jsonOptions);
                        Console.WriteLine($"IsUserAuthorized: Role: {result?.Role}, UserId: {result?.UserId}");
                        return result?.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase) ?? false;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Deserialization error in IsUserAuthorized: {ex.Message}");
                        return false;
                    }
                }

                Console.WriteLine($"IsUserAuthorized failed with status {response.StatusCode}: {rawResponse}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error in IsUserAuthorized: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in IsUserAuthorized: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
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

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class VerifyResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

}