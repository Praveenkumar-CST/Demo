using System.Net.Http.Json;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class EmployeeBasicDetailsService
    {
        private readonly HttpClient _httpClient;

        public EmployeeBasicDetailsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<EmployeeBasicDetails> GetEmployeeBasicDetailsAsync(string employeeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/EmployeeBasicDetails/{employeeId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EmployeeBasicDetails>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw new Exception($"Failed to fetch employee basic details: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEmployeeBasicDetailsAsync: {ex.Message}");
                throw;
            }
        }
    }
}
