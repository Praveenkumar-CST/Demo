namespace WiseHR.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using WiseHR.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Polly;
    using Polly.Extensions.Http;

    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public EmployeeService(HttpClient httpClient, IMemoryCache cache)
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

        public async Task<bool> RegisterEmployee(EmployeeDetails employee)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("EmployeeDetails/EmployeeDetailsRegistry", employee)));

            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            Console.WriteLine($"Error registering employee: {response.StatusCode}");
            return false;
        }

        public async Task<EmployeeDetails> GetEmployeeDetails(string employeeId)
        {
            string cacheKey = $"Employee_{employeeId}";
            if (_cache.TryGetValue(cacheKey, out EmployeeDetails cachedEmployee))
            {
                Console.WriteLine($"Cache hit for Employee_{employeeId}");
                return cachedEmployee;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"EmployeeDetails/GetEmployeeDetails/{employeeId}")));

                Console.WriteLine($"GetEmployeeDetails({employeeId}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employee = await response.Content.ReadFromJsonAsync<EmployeeDetails>();
                    if (employee != null)
                    {
                        _cache.Set(cacheKey, employee, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        });
                        return employee;
                    }
                }

                Console.WriteLine($"Employee details not found for ID: {employeeId}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employee details ({employeeId}): {ex.Message}");
                return null;
            }
        }
        public async Task<List<EmployeeDetails>> GetAllEmployees(int page = 1, int pageSize = 50, bool includePhotos = true)
        {
            string cacheKey = $"AllEmployees_{page}_{pageSize}_Photos_{includePhotos}";
            if (_cache.TryGetValue(cacheKey, out List<EmployeeDetails> cachedEmployees))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployees ?? new List<EmployeeDetails>();
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"EmployeeDetails/GetAllEmployees?page={page}&pageSize={pageSize}&includePhotos={includePhotos}")));

                Console.WriteLine($"GetAllEmployees(page={page}, pageSize={pageSize}, includePhotos={includePhotos}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDetails>>() ?? new List<EmployeeDetails>();

                    // Validate and log incomplete records
                    foreach (var emp in employees)
                    {
                        if (string.IsNullOrEmpty(emp.EmployeeID) || string.IsNullOrEmpty(emp.FirstName) || string.IsNullOrEmpty(emp.LastName))
                        {
                            Console.WriteLine($"Invalid employee data: ID={emp.EmployeeID}, FirstName={emp.FirstName}, LastName={emp.LastName}");
                        }
                        if (!string.IsNullOrEmpty(emp.EmployeeID))
                        {
                            _cache.Set($"Employee_{emp.EmployeeID}_Photos_{includePhotos}", emp, new MemoryCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(5)
                            });
                        }
                    }

                    // Cache the page
                    _cache.Set(cacheKey, employees, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    });

                    return employees;
                }

                throw new Exception($"Error fetching employees: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllEmployees: {ex.Message}");
                return new List<EmployeeDetails>();
            }
        }

        public async Task<List<EmployeeDetails>> GetEmployeeDetailsByIds(IEnumerable<string> employeeIds)
        {
            string cacheKey = $"EmployeeBatch_{string.Join("_", employeeIds.OrderBy(id => id))}";
            if (_cache.TryGetValue(cacheKey, out List<EmployeeDetails> cachedEmployees))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployees;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("EmployeeDetails/GetEmployeeDetailsByIds", employeeIds)));

                Console.WriteLine($"GetEmployeeDetailsByIds(count={employeeIds.Count()}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDetails>>() ?? new List<EmployeeDetails>();
                    _cache.Set(cacheKey, employees, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    });

                    // Cache individual employees
                    foreach (var emp in employees)
                    {
                        if (!string.IsNullOrEmpty(emp.EmployeeID))
                        {
                            _cache.Set($"Employee_{emp.EmployeeID}", emp, new MemoryCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(5)
                            });
                        }
                    }

                    return employees;
                }

                throw new Exception($"Error fetching employees by IDs: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEmployeeDetailsByIds: {ex.Message}");
                return new List<EmployeeDetails>();
            }
        }

        public async Task<EmployeeDetails> GetEmployeeDetailsByEmail(string email)
        {
            string cacheKey = $"EmployeeByEmail_{email.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out EmployeeDetails cachedEmployee))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployee;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"EmployeeDetails/GetEmployeeDetailsByEmail/{email}")));

                Console.WriteLine($"GetEmployeeDetailsByEmail({email}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employee = await response.Content.ReadFromJsonAsync<EmployeeDetails>();
                    if (employee != null)
                    {
                        _cache.Set(cacheKey, employee, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        });
                        if (!string.IsNullOrEmpty(employee.EmployeeID))
                        {
                            _cache.Set($"Employee_{employee.EmployeeID}", employee, new MemoryCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(5)
                            });
                        }
                        return employee;
                    }
                }

                Console.WriteLine($"Employee details not found for email: {email}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employee details by email ({email}): {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateEmployee(EmployeeDetails employee)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("EmployeeDetails/UpdateEmployeeDetails", employee)));

            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                _cache.Remove($"Employee_{employee.EmployeeID}");
                _cache.Remove($"EmployeeByEmail_{employee.CurrentEmail?.ToLower()}");
            }

            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> DeleteEmployee(string employeeId)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.DeleteAsync($"EmployeeDetails/DeleteEmployeeDetails/{employeeId}")));

            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                _cache.Remove($"Employee_{employeeId}");
                // Remove email cache entries if necessary
            }

            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}