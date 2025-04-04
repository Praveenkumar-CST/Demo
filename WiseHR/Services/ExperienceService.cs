using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WiseHRServer.Models;

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
        return await response.Content.ReadFromJsonAsync<bool>();
    }

    // Get experience details by Employee ID
    public async Task<Experience> GetExperienceAsync(string employeeId)
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
