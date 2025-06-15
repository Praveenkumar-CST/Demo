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
    private const string BatchKeysCacheKey = "ReportBatchKeys";
    private const string MenteeMentorCacheKeys = "MenteeMentorCacheKeys";

    public ReportService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
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
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cachedReports;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}?page={page}&pageSize={pageSize}")));

            Console.WriteLine($"GetReportsAsync(page={page}, pageSize={pageSize}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                var menteeMentorKeys = _cache.TryGetValue(MenteeMentorCacheKeys, out HashSet<string> existingKeys)
                    ? existingKeys
                    : new HashSet<string>();

                // Cache individual reports
                foreach (var report in reports)
                {
                    if (report.Id > 0)
                    {
                        _cache.Set(ReportByIdKey(report.Id), report, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                    }
                    if (!string.IsNullOrEmpty(report.MenteeId))
                    {
                        string menteeKey = MenteeReportsKey(report.MenteeId);
                        _cache.Set(menteeKey, reports.Where(r => r.MenteeId == report.MenteeId).ToList(), new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                        menteeMentorKeys.Add(menteeKey);
                    }
                    if (!string.IsNullOrEmpty(report.MentorId))
                    {
                        string mentorKey = MentorReportsKey(report.MentorId);
                        _cache.Set(mentorKey, reports.Where(r => r.MentorId == report.MentorId).ToList(), new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                        menteeMentorKeys.Add(mentorKey);
                    }
                }

                // Update mentee/mentor cache keys
                _cache.Set(MenteeMentorCacheKeys, menteeMentorKeys, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Cache the page
                _cache.Set(cacheKey, reports, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                return reports;
            }

            throw new Exception($"Error fetching reports: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetReportsAsync: {ex.Message}");
            return new List<ReportModel>();
        }
    }

    public async Task<ReportModel?> GetReportByIdAsync(int id)
    {
        string cacheKey = ReportByIdKey(id);
        if (_cache.TryGetValue(cacheKey, out ReportModel cached))
        {
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cached;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/{id}")));

            Console.WriteLine($"GetReportByIdAsync({id}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var report = await response.Content.ReadFromJsonAsync<ReportModel>();
                if (report != null)
                {
                    _cache.Set(cacheKey, report, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(15)
                    });
                }
                return report;
            }

            Console.WriteLine($"Report not found for ID: {id}");
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
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cached;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/mentee/{menteeId}")));

            Console.WriteLine($"GetReportsByMenteeIdAsync({menteeId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                _cache.Set(cacheKey, reports, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Update mentee/mentor cache keys
                if (_cache.TryGetValue(MenteeMentorCacheKeys, out HashSet<string> menteeMentorKeys))
                {
                    menteeMentorKeys.Add(cacheKey);
                }
                else
                {
                    menteeMentorKeys = new HashSet<string> { cacheKey };
                }
                _cache.Set(MenteeMentorCacheKeys, menteeMentorKeys, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                return reports;
            }

            Console.WriteLine($"Reports not found for MenteeID: {menteeId}");
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
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cached;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.GetAsync($"{BaseUrl}/mentor/{mentorId}")));

            Console.WriteLine($"GetReportsByMentorIdAsync({mentorId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                _cache.Set(cacheKey, reports, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Update mentee/mentor cache keys
                if (_cache.TryGetValue(MenteeMentorCacheKeys, out HashSet<string> menteeMentorKeys))
                {
                    menteeMentorKeys.Add(cacheKey);
                }
                else
                {
                    menteeMentorKeys = new HashSet<string> { cacheKey };
                }
                _cache.Set(MenteeMentorCacheKeys, menteeMentorKeys, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                return reports;
            }

            Console.WriteLine($"Reports not found for MentorID: {mentorId}");
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
            Console.WriteLine($"Cache hit for {cacheKey}");
            return cachedReports;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.PostAsJsonAsync($"{BaseUrl}/by-mentee-emails", menteeEmails)));

            Console.WriteLine($"GetReportsByMenteeEmailsAsync(count={menteeEmails.Count()}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<ReportModel>>() ?? new List<ReportModel>();
                var menteeMentorKeys = _cache.TryGetValue(MenteeMentorCacheKeys, out HashSet<string> existingKeys)
                    ? existingKeys
                    : new HashSet<string>();

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
                _cache.Set(cacheKey, reports, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                // Cache individual reports
                foreach (var report in reports)
                {
                    if (report.Id > 0)
                    {
                        _cache.Set(ReportByIdKey(report.Id), report, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                    }
                    if (!string.IsNullOrEmpty(report.MenteeId))
                    {
                        string menteeKey = MenteeReportsKey(report.MenteeId);
                        _cache.Set(menteeKey, reports.Where(r => r.MenteeId == report.MenteeId).ToList(), new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                        menteeMentorKeys.Add(menteeKey);
                    }
                    if (!string.IsNullOrEmpty(report.MentorId))
                    {
                        string mentorKey = MentorReportsKey(report.MentorId);
                        _cache.Set(mentorKey, reports.Where(r => r.MentorId == report.MentorId).ToList(), new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                        menteeMentorKeys.Add(mentorKey);
                    }
                }

                // Update mentee/mentor cache keys
                _cache.Set(MenteeMentorCacheKeys, menteeMentorKeys, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(15)
                });

                return reports;
            }

            throw new Exception($"Error fetching reports by mentee emails: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetReportsByMenteeEmailsAsync: {ex.Message}");
            return new List<ReportModel>();
        }
    }

    public async Task<bool> SubmitReportAsync(ReportModel report)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.PostAsJsonAsync(BaseUrl, report)));

            Console.WriteLine($"SubmitReportAsync took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache();
                return true;
            }

            Console.WriteLine($"Error submitting report for MenteeID: {report.MenteeId}, MentorID: {report.MentorId}, Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting report for MenteeID: {report.MenteeId}, MentorID: {report.MentorId}, Message: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateReportAsync(int id, ReportModel report)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.PutAsJsonAsync($"{BaseUrl}/{id}", report)));

            Console.WriteLine($"UpdateReportAsync({id}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(id);
                return true;
            }

            Console.WriteLine($"Error updating report for ID: {id}, Status: {response.StatusCode}");
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _http.DeleteAsync($"{BaseUrl}/{id}")));

            Console.WriteLine($"DeleteReportAsync({id}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(id);
                return true;
            }

            Console.WriteLine($"Error deleting report for ID: {id}, Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting report for ID: {id}, Message: {ex.Message}");
            return false;
        }
    }

    private void InvalidateCache(int? id = null)
    {
        _cache.Remove(AllReportsKey);

        if (id.HasValue)
            _cache.Remove(ReportByIdKey(id.Value));

        // Remove batch cache keys
        if (_cache.TryGetValue(BatchKeysCacheKey, out HashSet<string> batchKeys))
        {
            foreach (var key in batchKeys.ToList())
            {
                _cache.Remove(key);
            }
            _cache.Remove(BatchKeysCacheKey);
        }

        // Remove mentee/mentor cache keys
        if (_cache.TryGetValue(MenteeMentorCacheKeys, out HashSet<string> menteeMentorKeys))
        {
            foreach (var key in menteeMentorKeys.ToList())
            {
                _cache.Remove(key);
            }
            _cache.Remove(MenteeMentorCacheKeys);
        }
    }

    private string ReportByIdKey(int id) => $"Report_{id}";
    private string MenteeReportsKey(string id) => $"MenteeReports_{id}";
    private string MentorReportsKey(string id) => $"MentorReports_{id}";
}