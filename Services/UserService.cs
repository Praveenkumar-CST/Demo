using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<List<User>>("api/users");
                return users ?? new List<User>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching users: {ex.Message}", ex);
            }
        }

        public async Task UpdateUserRoleAsync(string id, string role)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role))
                throw new ArgumentException("ID and role cannot be null or empty");

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/users/{id}/role", role);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating role: {ex.Message}", ex);
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty");

            try
            {
                var response = await _httpClient.DeleteAsync($"api/users/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}", ex);
            }
        }
        public async Task<User> CreateUserAsync(string email, string password)
        {
            var request = new CreateUserRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Users", request);
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<User>();
            return user ?? throw new Exception("Failed to deserialize created user.");
        }
        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            try
            {
                // Assuming your backend API endpoint is: api/users/by-role/{role}
                var response = await _httpClient.GetAsync($"api/users/by-role/{role}");
                response.EnsureSuccessStatusCode();

                var users = await response.Content.ReadFromJsonAsync<List<User>>();
                return users ?? new List<User>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error while fetching users with role '{role}': {ex.Message}", ex);
            }
            catch (NotSupportedException ex)
            {
                throw new Exception("The content type is not supported for deserialization.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON in the response.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error fetching users with role '{role}': {ex.Message}", ex);
            }
        }


    }
}