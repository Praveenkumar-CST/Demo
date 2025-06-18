using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using WiseHR.Models;
using WiseHRServer.Models;

namespace WiseHR.Services
{
    public class MentorAssignmentService(HttpClient http, IMemoryCache cache)
    {
        private const string BaseUrl = "api/MentorAssignments";
        private const string CacheVersion = "v1_";

        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));

        private readonly AsyncTimeoutPolicy<HttpResponseMessage> _timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

        private static string ToUpperCase(string input) => string.IsNullOrWhiteSpace(input) ? input : input.ToUpper();

        public async Task<bool> CreateAssignmentAsync(MentorAssignment assignment)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.PostAsJsonAsync(BaseUrl, assignment)));

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCache(menteeId: assignment.MenteeEmployeeID, mentorId: assignment.MentorEmployeeID);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating assignment: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MentorAssignment>> GetAllAssignmentsAsync()
        {
            string cacheKey = $"{CacheVersion}AllAssignments";
            if (cache.TryGetValue(cacheKey, out List<MentorAssignment> cachedAssignments))
            {
                return cachedAssignments;
            }

            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.GetAsync(BaseUrl)));

                if (response.IsSuccessStatusCode)
                {
                    var assignments = await response.Content.ReadFromJsonAsync<List<MentorAssignment>>() ?? [];
                    UpdateCache(assignments, cacheKey);
                    return assignments;
                }

                throw new Exception($"Error fetching all assignments: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAssignmentsAsync: {ex.Message}");
                if (cache.TryGetValue(cacheKey, out List<MentorAssignment> staleAssignments))
                {
                    Console.WriteLine("Returning stale cached assignments due to API failure.");
                    return staleAssignments;
                }
                return [];
            }
        }

        public async Task<MentorAssignment?> GetAssignmentByIdAsync(int id)
        {
            string cacheKey = AssignmentByIdKey(id);
            if (cache.TryGetValue(cacheKey, out MentorAssignment cachedAssignment))
            {
                return cachedAssignment;
            }

            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.GetAsync($"{BaseUrl}/{id}")));

                if (response.IsSuccessStatusCode)
                {
                    var assignment = await response.Content.ReadFromJsonAsync<MentorAssignment>();
                    if (assignment != null)
                    {
                        cache.Set(cacheKey, assignment, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(10)
                        });
                    }
                    return assignment;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching assignment for ID: {id}, Message: {ex.Message}");
                if (cache.TryGetValue(cacheKey, out MentorAssignment staleAssignment))
                {
                    Console.WriteLine("Returning stale cached assignment due to API failure.");
                    return staleAssignment;
                }
                return null;
            }
        }

        public async Task<List<MentorAssignment>> GetMenteesByMentorIdAsync(string mentorId)
        {
            string cacheKey = MenteesByMentorKey(ToUpperCase(mentorId));
            if (cache.TryGetValue(cacheKey, out List<MentorAssignment> cachedAssignments))
            {
                return cachedAssignments;
            }

            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.GetAsync($"{BaseUrl}/mentees/{Uri.EscapeDataString(mentorId)}")));

                if (response.IsSuccessStatusCode)
                {
                    var assignments = await response.Content.ReadFromJsonAsync<List<MentorAssignment>>() ?? [];
                    UpdateCache(assignments, cacheKey);
                    return assignments;
                }

                throw new Exception($"Error fetching assignments for MentorID: {mentorId}, Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMenteesByMentorIdAsync: {ex.Message}");
                if (cache.TryGetValue(cacheKey, out List<MentorAssignment> staleAssignments))
                {
                    Console.WriteLine("Returning stale cached assignments due to API failure.");
                    return staleAssignments;
                }
                return [];
            }
        }

        public async Task<bool> UpdateAssignmentAsync(int id, MentorAssignment assignment)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.PutAsJsonAsync($"{BaseUrl}/{id}", assignment)));

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCache(assignment.MenteeEmployeeID, assignment.MentorEmployeeID, id);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating assignment for ID: {id}, Message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int id)
        {
            try
            {
                var assignment = await GetAssignmentByIdAsync(id);
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.DeleteAsync($"{BaseUrl}/{id}")));

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCache(assignment?.MenteeEmployeeID, assignment?.MentorEmployeeID, id);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting assignment for ID: {id}, Message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssignmentByMenteeEmailAsync(string menteeEmail)
        {
            try
            {
                var encodedEmail = Uri.EscapeDataString(menteeEmail);
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        http.DeleteAsync($"{BaseUrl}/by-mentee-email/{encodedEmail}")));

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCache(ToUpperCase(menteeEmail), null);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting assignment for MenteeEmail: {menteeEmail}, Message: {ex.Message}");
                return false;
            }
        }

        private void UpdateCache(List<MentorAssignment> assignments, string primaryCacheKey)
        {
            foreach (var assignment in assignments)
            {
                if (assignment.Id > 0)
                {
                    string assignmentKey = AssignmentByIdKey(assignment.Id);
                    cache.Set(assignmentKey, assignment, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
                if (!string.IsNullOrEmpty(assignment.MenteeEmployeeID))
                {
                    string menteeKey = AssignmentsByMenteeKey(ToUpperCase(assignment.MenteeEmployeeID));
                    cache.Set(menteeKey, assignments.Where(a => a.MenteeEmployeeID == assignment.MenteeEmployeeID).ToList(), new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
                if (!string.IsNullOrEmpty(assignment.MentorEmployeeID))
                {
                    string mentorKey = MenteesByMentorKey(ToUpperCase(assignment.MentorEmployeeID));
                    cache.Set(mentorKey, assignments.Where(a => a.MentorEmployeeID == assignment.MentorEmployeeID).ToList(), new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }
            }

            cache.Set(primaryCacheKey, assignments, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });
        }

        private void InvalidateCache(string? menteeId = null, string? mentorId = null, int? id = null)
        {
            if (id.HasValue)
            {
                cache.Remove(AssignmentByIdKey(id.Value));
            }

            if (!string.IsNullOrEmpty(menteeId))
            {
                cache.Remove(AssignmentsByMenteeKey(ToUpperCase(menteeId)));
            }

            if (!string.IsNullOrEmpty(mentorId))
            {
                cache.Remove(MenteesByMentorKey(ToUpperCase(mentorId)));
            }

            cache.Remove($"{CacheVersion}AllAssignments");
        }

        private static string AssignmentByIdKey(int id) => $"{CacheVersion}Assignment_{id}";
        private static string MenteesByMentorKey(string id) => $"{CacheVersion}MenteesByMentor_{id}";
        private static string AssignmentsByMenteeKey(string id) => $"{CacheVersion}AssignmentsByMentee_{id}";
    }
}