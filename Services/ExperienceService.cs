using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WiseHRServer.Models;
using WiseHR.Models;
using Microsoft.Extensions.Caching.Memory;
using Amazon.Runtime.Internal.Util;

public class ExperienceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;


    public ExperienceService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;

    }


    // Add new experience
    public async Task<bool> RegisterExperienceAsync(Experience experience)
    {
        var response = await _httpClient.PostAsJsonAsync("Experience/ExperienceRegistry", experience);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode)
        {
            // Invalidate cache
            _cache.Remove("AllExperience");
            _cache.Remove($"Experience_{experience.EmployeeID}");

            return await response.Content.ReadFromJsonAsync<bool>();
        }

        Console.WriteLine("Error Experience details");
        return false;
    }

    public async Task<List<Experience>> GetAllExperience()
    {
        const string cacheKey = "AllExperience";

        if (_cache.TryGetValue(cacheKey, out List<Experience> cachedExperiences))
        {
            return cachedExperiences;
        }

        try
        {
            var response = await _httpClient.GetAsync("Experience/GetAllExperience");
            if (response.IsSuccessStatusCode)
            {
                var experiences = await response.Content.ReadFromJsonAsync<List<Experience>>();

                _cache.Set(cacheKey, experiences, TimeSpan.FromMinutes(5));
                return experiences;
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
        string cacheKey = $"Experience_{employeeId}";

        if (_cache.TryGetValue(cacheKey, out Experience cachedExperience))
        {
            return cachedExperience;
        }

        var experience = await _httpClient.GetFromJsonAsync<Experience>($"Experience/GetExperienceInfo/{employeeId}");

        if (experience != null)
        {
            _cache.Set(cacheKey, experience, TimeSpan.FromMinutes(5));
        }

        return experience;
    }
    // Update experience
    public async Task<bool> UpdateExperienceAsync(Experience experience)
    {
        var response = await _httpClient.PostAsJsonAsync("Experience/UpdateExperience", experience);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        if (result)
        {
            _cache.Remove("AllExperience");
            _cache.Remove($"Experience_{experience.EmployeeID}");
        }

        return result;
    }

    // Delete experience by Employee ID
    public async Task<bool> DeleteExperienceAsync(string employeeId)
    {
        var response = await _httpClient.DeleteAsync($"Experience/DeleteExperience/{employeeId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        if (result)
        {
            _cache.Remove("AllExperience");
            _cache.Remove($"Experience_{employeeId}");
        }
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
