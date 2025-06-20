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

public class ReportService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private const string BaseUrl = "api/reports";
    private const string AllReportsKey = "AllReports";
    private const string CacheKeys = "ReportCacheKeys";

    public ReportService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    // Optimized retry policy: shorter backoff for faster recovery
    private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

    // Optimized timeout policy: reduced to 5 seconds for faster failure
    private static readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy = Policy
        .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

    private string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    private string ToUpperCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;
        return input.ToUpper();
    }

    public async Task<List<ReportModel>> GetReportsAsync(int page = 1, int pageSize = 50)
    {
        string cacheKey = $"{AllReportsKey}_{page}_{pageSize}";
        if (_cache.TryGetValue(cacheKey, out List<ReportModel> cachedReports))
        {
            return cachedReports;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}?page={page}&pageSize={pageSize}")));

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                UpdateCache(reports, cacheKey);
                return reports;
            }

            throw new Exception($"Error fetching reports: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetReportsAsync: {ex.Message}");
            if (_cache.TryGetValue(cacheKey, out List<ReportModel> staleReports))
            {
                Console.WriteLine("Returning stale cached reports due to API failure.");
                return staleReports;
            }
            return new List<ReportModel>();
        }
    }

    public async Task<ReportModel?> GetReportByIdAsync(int id)
    {
        string cacheKey = ReportByIdKey(id);
        if (_cache.TryGetValue(cacheKey, out ReportModel cached))
        {
            return cached;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/{id}")));

            if (response.IsSuccessStatusCode)
            {
                var report = await response.Content.ReadFromJsonAsync<ReportModel>();
                if (report != null)
                {
                    _cache.Set(cacheKey, report, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                    UpdateCacheKeys(cacheKey);
                }
                return report;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching report for ID: {id}, Message: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ReportModel>> GetReportsByMenteeIdAsync(string menteeId)
    {
        string cacheKey = MenteeReportsKey(menteeId);
        if (_cache.TryGetValue(cacheKey, out List<ReportModel> cached))
        {
            return cached;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/mentee/{menteeId}")));

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                UpdateCache(reports, cacheKey);
                return reports;
            }

            return new List<ReportModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reports for MenteeID: {menteeId}, Message: {ex.Message}");
            return new List<ReportModel>();
        }
    }

    public async Task<List<ReportModel>> GetReportsByMentorIdAsync(string mentorId)
    {
        string cacheKey = MentorReportsKey(mentorId);
        if (_cache.TryGetValue(cacheKey, out List<ReportModel> cached))
        {
            return cached;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/mentor/{mentorId}")));

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                UpdateCache(reports, cacheKey);
                return reports;
            }

            return new List<ReportModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reports for MentorID: {mentorId}, Message: {ex.Message}");
            return new List<ReportModel>();
        }
    }

    public async Task<List<ReportModel>> GetReportsByMenteeEmailsAsync(IEnumerable<string> menteeEmails)
    {
        string cacheKey = $"MenteeReportsBatch_{string.Join("_", menteeEmails.OrderBy(email => email))}";
        if (_cache.TryGetValue(cacheKey, out List<ReportModel> cachedReports))
        {
            return cachedReports;
        }

        // Check individual mentee caches first to avoid API call
        var reports = new List<ReportModel>();
        var missingEmails = new List<string>();
        foreach (var email in menteeEmails)
        {
            string menteeCacheKey = MenteeReportsKey(email);
            if (_cache.TryGetValue(menteeCacheKey, out List<ReportModel> menteeReports))
            {
                reports.AddRange(menteeReports);
            }
            else
            {
                missingEmails.Add(email);
            }
        }

        if (missingEmails.Any())
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _http.PostAsJsonAsync($"{BaseUrl}/by-mentee-emails", missingEmails)));

                if (response.IsSuccessStatusCode)
                {
                    var newReports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                    reports.AddRange(newReports);
                    UpdateCache(newReports, cacheKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetReportsByMenteeEmailsAsync: {ex.Message}");
            }
        }

        // Cache the combined result
        _cache.Set(cacheKey, reports, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });
        UpdateCacheKeys(cacheKey);

        return reports;
    }

    public async Task<bool> SubmitReportAsync(ReportModel report)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.PostAsJsonAsync(BaseUrl, report)));

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(report.Id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting report for MenteeID: {report.MenteeEmployeeID}, MentorID: {report.MentorEmployeeID}, Message: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateReportAsync(int id, ReportModel report)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.PutAsJsonAsync($"{BaseUrl}/{id}", report)));

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating report for ID: {id}, Message: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteReportAsync(int id)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.DeleteAsync($"{BaseUrl}/{id}")));

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting report for ID: {id}, Message: {ex.Message}");
            return false;
        }
    }

    private void UpdateCache(List<ReportModel> reports, string primaryCacheKey)
    {
        var cacheKeys = _cache.TryGetValue(CacheKeys, out HashSet<string> existingKeys)
            ? existingKeys
            : new HashSet<string>();

        // Cache individual reports and mentee/mentor groupings
        foreach (var report in reports)
        {
            if (report.Id > 0)
            {
                string reportKey = ReportByIdKey(report.Id);
                _cache.Set(reportKey, report, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
                cacheKeys.Add(reportKey);
            }
            if (!string.IsNullOrEmpty(report.MenteeEmployeeID))
            {
                string menteeKey = MenteeReportsKey(report.MenteeEmployeeID);
                _cache.Set(menteeKey, reports.Where(r => r.MenteeEmployeeID == report.MenteeEmployeeID).ToList(), new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
                cacheKeys.Add(menteeKey);
            }
            if (!string.IsNullOrEmpty(report.MentorEmployeeID))
            {
                string mentorKey = MentorReportsKey(report.MentorEmployeeID);
                _cache.Set(mentorKey, reports.Where(r => r.MentorEmployeeID == report.MentorEmployeeID).ToList(), new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
                cacheKeys.Add(mentorKey);
            }
        }

        // Cache the primary result
        _cache.Set(primaryCacheKey, reports, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });
        cacheKeys.Add(primaryCacheKey);

        // Update cache keys
        _cache.Set(CacheKeys, cacheKeys, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });
    }

    private void UpdateCacheKeys(string cacheKey)
    {
        var cacheKeys = _cache.TryGetValue(CacheKeys, out HashSet<string> existingKeys)
            ? existingKeys
            : new HashSet<string>();
        cacheKeys.Add(cacheKey);
        _cache.Set(CacheKeys, cacheKeys, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });
    }

    private void InvalidateCache(int? id = null)
    {
        var cacheKeys = _cache.TryGetValue(CacheKeys, out HashSet<string> existingKeys)
            ? existingKeys
            : new HashSet<string>();

        if (id.HasValue)
        {
            string reportKey = ReportByIdKey(id.Value);
            _cache.Remove(reportKey);
            cacheKeys.Remove(reportKey);
        }

        foreach (var key in cacheKeys.ToList())
        {
            if (id == null || key.Contains("MenteeReports_") || key.Contains("MentorReports_") || key.Contains("MenteeReportsBatch_"))
            {
                _cache.Remove(key);
                cacheKeys.Remove(key);
            }
        }

        _cache.Set(CacheKeys, cacheKeys, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });
    }

    private string ReportByIdKey(int id) => $"Report_{id}";
    private string MenteeReportsKey(string id) => $"MenteeReports_{id}";
    private string MentorReportsKey(string id) => $"MentorReports_{id}";
}