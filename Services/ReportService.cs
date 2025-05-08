using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WiseHR.Models; // Adjust this namespace as needed

public class ReportService
{
    private readonly HttpClient _http;

    public ReportService(HttpClient http)
    {
        _http = http;
    }

    // Get all reports
    public async Task<List<ReportModel>> GetReportsAsync()
    {
        return await _http.GetFromJsonAsync<List<ReportModel>>("api/reports");
    }

    // Get report by ID
    public async Task<ReportModel?> GetReportByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ReportModel>($"api/reports/{id}");
    }
    // Get reports by Mentee ID
    public async Task<List<ReportModel>> GetReportsByMenteeIdAsync(string menteeId)
    {
        return await _http.GetFromJsonAsync<List<ReportModel>>($"api/reports/mentee/{menteeId}");
    }

    // Get reports by Mentor ID
    public async Task<List<ReportModel>> GetReportsByMentorIdAsync(string mentorId)
    {
        return await _http.GetFromJsonAsync<List<ReportModel>>($"api/reports/mentor/{mentorId}");
    }

    // Get reports by mentee email
    public async Task<List<ReportModel>> GetReportsByMenteeEmailAsync(string email)
    {
        var reports = await GetReportsAsync();
        return reports.Where(r => r.MenteeEmail == email).ToList();
    }

    // Submit new report
    public async Task<bool> SubmitReportAsync(ReportModel report)
    {
        try
        {
            // Debug: print report data to console
            Console.WriteLine("Submitting Report:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));

            var response = await _http.PostAsJsonAsync("api/reports", report);

            // Debug: print response info
            Console.WriteLine($"Response Status: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Content: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during SubmitReportAsync: {ex.Message}");
            return false;
        }
    }


    // Update report
    public async Task<bool> UpdateReportAsync(int id, ReportModel report)
    {
        try
        {
            Console.WriteLine($"Updating report with ID: {id}");
            var response = await _http.PutAsJsonAsync($"api/reports/{id}", report);
            Console.WriteLine($"Response Status: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Content: {errorContent}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during UpdateReportAsync: {ex.Message}");
            return false;
        }
    }
    // Delete report
    public async Task<bool> DeleteReportAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/reports/{id}");
        return response.IsSuccessStatusCode;
    }
}