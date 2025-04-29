using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WiseHRServer.Models;
using WiseHR.Models;

public class ExperienceService
{
    private readonly HttpClient _httpClient;

    public ExperienceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    // Add new experience
    public async Task<bool> RegisterExperienceAsync(Experience experience)
    {
        var response = await _httpClient.PostAsJsonAsync("Experience/ExperienceRegistry", experience);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode)
        {
            // Return the boolean value from the response
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        Console.WriteLine("Error Experience details");
        return false;
    }

    public async Task<List<Experience>> GetAllExperience()
    {
        try
        {
            var response = await _httpClient.GetAsync("Experience/GetAllExperience"); // ✅ Fixed endpoint
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Experience>>();
            }
            else
            {
                throw new Exception($"Error fetching experience details: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<Experience>();
        }
    }

    // Get experience details by Employee ID

    public async Task<Experience> GetExperienceDetails(string employeeId)
    {
        return await _httpClient.GetFromJsonAsync<Experience>($"Experience/GetExperienceInfo/{employeeId}");
    }
    // Update experience
    public async Task<bool> UpdateExperienceAsync(Experience experience)
    {
        var response = await _httpClient.PostAsJsonAsync("Experience/UpdateExperience", experience);
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    // Delete experience by Employee ID
    public async Task<bool> DeleteExperienceAsync(string employeeId)
    {
        var response = await _httpClient.DeleteAsync($"Experience/DeleteExperience/{employeeId}");
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
