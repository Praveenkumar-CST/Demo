using Polly.Extensions.Http;
using Polly;
using System.Net.Http.Json;
using WiseHR.Models;
using WiseHRServer.Models;

namespace WiseHR.Services
{
    public class MentorAssignmentService
    {
        private readonly HttpClient _http;

        public MentorAssignmentService(HttpClient http)
        {
            _http = http;
        }

        private string baseUrl = "api/MentorAssignments";


        // Create
        public async Task<bool> CreateAssignmentAsync(MentorAssignment assignment)
        {
            var response = await _http.PostAsJsonAsync(baseUrl, assignment);
            return response.IsSuccessStatusCode;
        }

        // Read All
        public async Task<List<MentorAssignment>> GetAllAssignmentsAsync()
        {
            return await _http.GetFromJsonAsync<List<MentorAssignment>>(baseUrl);
        }

        // Read by ID
        public async Task<MentorAssignment> GetAssignmentByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<MentorAssignment>($"{baseUrl}/{id}");
        }
        // ✅ Get Mentees by Mentor ID
        public async Task<List<MentorAssignment>> GetMenteesByMentorIdAsync(string mentorId)
        {
            return await _http.GetFromJsonAsync<List<MentorAssignment>>($"{baseUrl}/mentees/{mentorId}");
        }
        // Update
        public async Task<bool> UpdateAssignmentAsync(int id, MentorAssignment assignment)
        {
            var response = await _http.PutAsJsonAsync($"{baseUrl}/{id}", assignment);
            return response.IsSuccessStatusCode;
        }

        // Delete
        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            var response = await _http.DeleteAsync($"{baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        // Delete by MenteeEmail
        public async Task<bool> DeleteAssignmentByMenteeEmailAsync(string menteeEmail)
        {
            var encodedEmail = Uri.EscapeDataString(menteeEmail); // safely encode email for URL
            var response = await _http.DeleteAsync($"{baseUrl}/by-mentee-email/{encodedEmail}");
            return response.IsSuccessStatusCode;
        }

    }
}