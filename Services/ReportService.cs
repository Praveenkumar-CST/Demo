using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WiseHR.Models;
using System.Collections.Concurrent;

public class ReportService
{
    private readonly HttpClient _http;
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public ReportService(HttpClient http)
    {
        _http = http;
    }

    private const string AllReportsKey = "AllReports";
    private string ReportByIdKey(int id) => $"Report_{id}";
    private string MenteeReportsKey(string id) => $"MenteeReports_{id}";
    private string MentorReportsKey(string id) => $"MentorReports_{id}";

    // Get all reports with cache
    public async Task<List<ReportModel>> GetReportsAsync()
    {
        if (_cache.TryGetValue(AllReportsKey, out var cachedReports))
            return (List<ReportModel>)cachedReports;

        var reports = await _http.GetFromJsonAsync<List<ReportModel>>("api/reports");
        if (reports != null)
            _cache[AllReportsKey] = reports;

        return reports ?? new List<ReportModel>();
    }

    // Get report by ID with cache
    public async Task<ReportModel?> GetReportByIdAsync(int id)
    {
        var key = ReportByIdKey(id);
        if (_cache.TryGetValue(key, out var cached))
            return (ReportModel)cached;

        var report = await _http.GetFromJsonAsync<ReportModel>($"api/reports/{id}");
        if (report != null)
            _cache[key] = report;

        return report;
    }

    // Get reports by Mentee ID with cache
    public async Task<List<ReportModel>> GetReportsByMenteeIdAsync(string menteeId)
    {
        var key = MenteeReportsKey(menteeId);
        if (_cache.TryGetValue(key, out var cached))
            return (List<ReportModel>)cached;

        var reports = await _http.GetFromJsonAsync<List<ReportModel>>($"api/reports/mentee/{menteeId}");
        if (reports != null)
            _cache[key] = reports;

        return reports ?? new List<ReportModel>();
    }

    // Get reports by Mentor ID with cache
    public async Task<List<ReportModel>> GetReportsByMentorIdAsync(string mentorId)
    {
        var key = MentorReportsKey(mentorId);
        if (_cache.TryGetValue(key, out var cached))
            return (List<ReportModel>)cached;

        var reports = await _http.GetFromJsonAsync<List<ReportModel>>($"api/reports/mentor/{mentorId}");
        if (reports != null)
            _cache[key] = reports;

        return reports ?? new List<ReportModel>();
    }

    // Get reports by mentee email (filtered locally from all reports)
    public async Task<List<ReportModel>> GetReportsByMenteeEmailAsync(string email)
    {
        var reports = await GetReportsAsync();
        return reports.Where(r => r.MenteeEmail == email).ToList();
    }

    // Submit new report and invalidate cache
    public async Task<bool> SubmitReportAsync(ReportModel report)
    {
        var response = await _http.PostAsJsonAsync("api/reports", report);
        if (response.IsSuccessStatusCode)
            InvalidateCache();

        return response.IsSuccessStatusCode;
    }

    // Update report and invalidate cache
    public async Task<bool> UpdateReportAsync(int id, ReportModel report)
    {
        var response = await _http.PutAsJsonAsync($"api/reports/{id}", report);
        if (response.IsSuccessStatusCode)
            InvalidateCache(id);

        return response.IsSuccessStatusCode;
    }

    // Delete report and invalidate cache
    public async Task<bool> DeleteReportAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/reports/{id}");
        if (response.IsSuccessStatusCode)
            InvalidateCache(id);

        return response.IsSuccessStatusCode;
    }

    // Invalidate all or specific cache entries
    private void InvalidateCache(int? id = null)
    {
        _cache.TryRemove(AllReportsKey, out _);

        if (id.HasValue)
            _cache.TryRemove(ReportByIdKey(id.Value), out _);

        // Optional: clear all mentee/mentor keys too
        foreach (var key in _cache.Keys.Where(k => k.StartsWith("MenteeReports_") || k.StartsWith("MentorReports_")).ToList())
        {
            _cache.TryRemove(key, out _);
        }
    }
}
