using System.Net.Http.Json;
using WiseHR.Models;
using Microsoft.Extensions.Caching.Memory;

namespace WiseHR.Services
{
    public class MentorAssignmentService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public MentorAssignmentService(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        private string baseUrl = "api/MentorAssignments";

        // Create
        public async Task<bool> CreateAssignmentAsync(MentorAssignment assignment)
        {
            var response = await _http.PostAsJsonAsync(baseUrl, assignment);
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(assignment.MentorEmployeeID, assignment.MenteeEmail, assignment.Id);
            }
            return response.IsSuccessStatusCode;
        }

        // Read All with cache
        public async Task<List<MentorAssignment>> GetAllAssignmentsAsync()
        {
            const string cacheKey = "AllMentorAssignments";

            if (_cache.TryGetValue(cacheKey, out List<MentorAssignment> cachedAssignments))
            {
                return cachedAssignments;
            }

            var result = await _http.GetFromJsonAsync<List<MentorAssignment>>(baseUrl);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        // Read by ID with cache
        public async Task<MentorAssignment> GetAssignmentByIdAsync(int id)
        {
            string cacheKey = $"MentorAssignment_{id}";

            if (_cache.TryGetValue(cacheKey, out MentorAssignment cached))
                return cached;

            var assignment = await _http.GetFromJsonAsync<MentorAssignment>($"{baseUrl}/{id}");
            if (assignment != null)
                _cache.Set(cacheKey, assignment, TimeSpan.FromMinutes(5));

            return assignment;
        }

        // Get mentees by mentor ID with cache
        public async Task<List<MentorAssignment>> GetMenteesByMentorIdAsync(string mentorId)
        {
            string cacheKey = $"Mentees_{mentorId}";

            if (_cache.TryGetValue(cacheKey, out List<MentorAssignment> cached))
                return cached;

            var mentees = await _http.GetFromJsonAsync<List<MentorAssignment>>($"{baseUrl}/mentees/{mentorId}");
            if (mentees != null)
                _cache.Set(cacheKey, mentees, TimeSpan.FromMinutes(5));

            return mentees;
        }

        // Update
        public async Task<bool> UpdateAssignmentAsync(int id, MentorAssignment assignment)
        {
            var response = await _http.PutAsJsonAsync($"{baseUrl}/{id}", assignment);
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(assignment.MentorEmployeeID, assignment.MenteeEmail, id);
            }
            return response.IsSuccessStatusCode;
        }

        // Delete
        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            var response = await _http.DeleteAsync($"{baseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(null, null, id);
            }
            return response.IsSuccessStatusCode;
        }

        // Delete by MenteeEmail
        public async Task<bool> DeleteAssignmentByMenteeEmailAsync(string menteeEmail)
        {
            var encodedEmail = Uri.EscapeDataString(menteeEmail);
            var response = await _http.DeleteAsync($"{baseUrl}/by-mentee-email/{encodedEmail}");
            if (response.IsSuccessStatusCode)
            {
                InvalidateCache(null, menteeEmail, null);
            }
            return response.IsSuccessStatusCode;
        }

        // Clear related cache entries
        private void InvalidateCache(string? mentorId, string? menteeEmail, int? id)
        {
            _cache.Remove("AllMentorAssignments");

            if (mentorId != null)
                _cache.Remove($"Mentees_{mentorId}");

            if (id != null)
                _cache.Remove($"MentorAssignment_{id}");

            // optionally: remove based on menteeEmail if you use such cache
        }
    }
}
