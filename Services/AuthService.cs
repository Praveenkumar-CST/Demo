using Microsoft.JSInterop;
using Supabase.Gotrue;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using WiseHR.Models;
using System.IdentityModel.Tokens.Jwt;

namespace WiseHR.Services
{
    // Changed: Use primary constructor
    public class AuthService(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly IJSRuntime _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

        public async Task<(string? Token, string? Error)> Login(string email, string password)
        {
            try
            {
                var requestBody = new { email, password };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (!string.IsNullOrEmpty(result?.Token))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
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

        public async Task<(string? Token, string? UserId, string? Error)> Signup(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/signup", new { email, password });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SignupResponse>();
                    return (result?.Token, result?.UserId, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, null, errorContent);
            }
            catch (HttpRequestException ex)
            {
                return (null, null, $"Failed to connect to the server: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> ForgotPassword(string email)
        {
            try
            {
                var request = new { Email = email.ToLower() };
                var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        // In AuthService.cs
        public async Task<(bool Success, string? Error)> ResetPassword(string email, string token, string newPassword)
        {
            try
            {
                var request = new
                {
                    Email = email.ToLower(),
                    Token = token,
                    NewPassword = newPassword
                };
                var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                await _jsRuntime.InvokeVoidAsync("console.error", $"Reset password error response: {errorContent}");
                var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                return (false, error?.Message ?? "Failed to reset password.");
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Reset password exception: {ex.Message}");
                return (false, ex.Message);
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

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/verify");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyResponse>();
                    // Changed: Simplify substring using range operator
                    var normalizedRole = !string.IsNullOrEmpty(result?.Role)
                        ? char.ToUpper(result.Role[0]) + result.Role[1..].ToLower()
                        : null;
                    if (!string.IsNullOrEmpty(normalizedRole))
                    {
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userRole", normalizedRole);
                    }
                    return (normalizedRole, null);
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, errorContent);
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Failed to connect to the server: {ex.Message}");
            }
        }

        public async Task<bool> IsUserAuthorized(string requiredRole)
        {
            try
            {
                var (role, error) = await VerifyToken();
                return role == requiredRole;
            }
            catch
            {
                return false;
            }
        }

        private class ErrorResponse
        {
            public string? Message { get; set; }
        }

        private class LoginResponse
        {
            public string? Token { get; set; }
            public string? UserId { get; set; }
        }

        private class SignupResponse
        {
            public string? Token { get; set; }
            public string? UserId { get; set; }
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