using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WiseHR.Models;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;
using WiseHRServer.Models;

public class ExperienceService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private const string BatchKeysCacheKey = "ExperienceBatchKeys";

    public ExperienceService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    // Retry policy for HttpClient
    private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    // Timeout policy
    private static readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

    public async Task<bool> RegisterExperienceAsync(Experience experience)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("Experience/ExperienceRegistry", experience)));

            Console.WriteLine($"RegisterExperienceAsync took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(experience.EmployeeID);
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            Console.WriteLine($"Error registering experience for EmployeeID: {experience.EmployeeID}, Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering experience for EmployeeID: {experience.EmployeeID}, Message: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Experience>> GetAllExperience(int page = 1, int pageSize = 50)
    {
        string cacheKey = $"AllExperience_{page}_{pageSize}";
        if (_cache.TryGetValue(cacheKey, out List<Experience> cachedExperiences))
        {
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cachedExperiences;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"Experience/GetAllExperience?page={page}&pageSize={pageSize}")));

            Console.WriteLine($"GetAllExperience(page={page}, pageSize={pageSize}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var experiences = await response.Content.ReadFromJsonAsync<List<Experience>>() ?? new List<Experience>();

                // Cache individual experiences
                foreach (var exp in experiences)
                {
                    if (!string.IsNullOrEmpty(exp.EmployeeID))
                    {
                        _cache.Set($"Experience_{exp.EmployeeID}", exp, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                    }
                }

                // Cache the page
                _cache.Set(cacheKey, experiences, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                return experiences;
            }

            throw new Exception($"Error fetching experiences: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllExperience: {ex.Message}");
            return new List<Experience>();
        }
    }

    public async Task<Experience> GetExperienceDetails(string employeeId)
    {
        string cacheKey = $"Experience_{employeeId}";
        if (_cache.TryGetValue(cacheKey, out Experience cachedExperience))
        {
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cachedExperience;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"Experience/GetExperienceInfo/{employeeId}")));

            Console.WriteLine($"GetExperienceDetails({employeeId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var experience = await response.Content.ReadFromJsonAsync<Experience>();
                if (experience != null)
                {
                    _cache.Set(cacheKey, experience, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(15)
                    });
                }
                return experience;
            }

            Console.WriteLine($"Experience not found for EmployeeID: {employeeId}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching experience for EmployeeID: {employeeId}, Message: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Experience>> GetExperienceByEmployeeIds(IEnumerable<string> employeeIds)
    {
        string cacheKey = $"ExperienceBatch_{string.Join("_", employeeIds.OrderBy(id => id))}";
        if (_cache.TryGetValue(cacheKey, out List<Experience> cachedExperiences))
        {
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cachedExperiences;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("Experience/GetExperienceByIds", employeeIds)));

            Console.WriteLine($"GetExperienceByEmployeeIds(count={employeeIds.Count()}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var experiences = await response.Content.ReadFromJsonAsync<List<Experience>>() ?? new List<Experience>();

                // Store batch cache key
                if (_cache.TryGetValue(BatchKeysCacheKey, out HashSet<string> batchKeys))
                {
                    batchKeys.Add(cacheKey);
                }
                else
                {
                    batchKeys = new HashSet<string> { cacheKey };
                }
                _cache.Set(BatchKeysCacheKey, batchKeys, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Cache batch result
                _cache.Set(cacheKey, experiences, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Cache individual experiences
                foreach (var exp in experiences)
                {
                    if (!string.IsNullOrEmpty(exp.EmployeeID))
                    {
                        _cache.Set($"Experience_{exp.EmployeeID}", exp, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                    }
                }

                return experiences;
            }

            throw new Exception($"Error fetching experiences by IDs: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetExperienceByEmployeeIds: {ex.Message}");
            return new List<Experience>();
        }
    }

    public async Task<bool> UpdateExperienceAsync(Experience experience)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("Experience/UpdateExperience", experience)));

            Console.WriteLine($"UpdateExperienceAsync took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                if (result)
                {
                    InvalidateCache(experience.EmployeeID);
                }
                return result;
            }

            Console.WriteLine($"Error updating experience for EmployeeID: {experience.EmployeeID}, Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating experience for EmployeeID: {experience.EmployeeID}, Message: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteExperienceAsync(string employeeId)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.DeleteAsync($"Experience/DeleteExperience/{employeeId}")));

            Console.WriteLine($"DeleteExperienceAsync({employeeId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                if (result)
                {
                    InvalidateCache(employeeId);
                }
                return result;
            }

            Console.WriteLine($"Error deleting experience for EmployeeID: {employeeId}, Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting experience for EmployeeID: {employeeId}, Message: {ex.Message}");
            return false;
        }
    }

    private void InvalidateCache(string employeeId)
    {
        _cache.Remove("AllExperience");
        _cache.Remove($"Experience_{employeeId}");
        // Remove batch cache keys
        if (_cache.TryGetValue(BatchKeysCacheKey, out HashSet<string> batchKeys))
        {
            foreach (var key in batchKeys.ToList())
            {
                _cache.Remove(key);
            }
            _cache.Remove(BatchKeysCacheKey);
        }
    }
}