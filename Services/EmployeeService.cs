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
    using System.Text.Json;
    using MudBlazor;
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EmployeeService> _logger;
        public EmployeeService(HttpClient httpClient, IMemoryCache cache, ILogger<EmployeeService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
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

        private string ToLowerCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            return input.ToLower();
        }

        private void CapitalizeEmployeeDetails(EmployeeDetails employee)
        {
            // Names
            employee.FirstName = ToTitleCase(employee.FirstName);
            employee.LastName = ToTitleCase(employee.LastName);
            employee.MiddleName = ToTitleCase(employee.MiddleName);
            employee.FatherName = ToTitleCase(employee.FatherName);
            employee.MotherName = ToTitleCase(employee.MotherName);
            employee.EmergencyContact1Name = ToTitleCase(employee.EmergencyContact1Name);
            employee.EmergencyContact2Name = ToTitleCase(employee.EmergencyContact2Name);
            employee.PassportFullName = ToTitleCase(employee.PassportFullName);

            // Addresses
            employee.CurrentAddress = ToTitleCase(employee.CurrentAddress);
            employee.CurrentCity = ToTitleCase(employee.CurrentCity);
            employee.CurrentState = ToTitleCase(employee.CurrentState);
            employee.PermanentAddress = ToTitleCase(employee.PermanentAddress);
            employee.PermanentCity = ToTitleCase(employee.PermanentCity);
            employee.PermanentState = ToTitleCase(employee.PermanentState);
            employee.EmergencyContact1Address = ToTitleCase(employee.EmergencyContact1Address);
            employee.EmergencyContact1City = ToTitleCase(employee.EmergencyContact1City);
            employee.EmergencyContact1State = ToTitleCase(employee.EmergencyContact1State);
            employee.EmergencyContact2Address = ToTitleCase(employee.EmergencyContact2Address);
            employee.EmergencyContact2City = ToTitleCase(employee.EmergencyContact2City);
            employee.EmergencyContact2State = ToTitleCase(employee.EmergencyContact2State);

            // Emails
            employee.CurrentEmail = ToLowerCase(employee.CurrentEmail);
            employee.PermanentEmail = ToLowerCase(employee.PermanentEmail);

            // Codes
            employee.EmployeeID = ToUpperCase(employee.EmployeeID);
            employee.EmployeeCode = ToUpperCase(employee.EmployeeCode);

            // Other string fields
            employee.TypeOfEmployment = ToTitleCase(employee.TypeOfEmployment);
            employee.Level = ToTitleCase(employee.Level);
            employee.Designation = ToTitleCase(employee.Designation);
            employee.Gender = ToTitleCase(employee.Gender);
            employee.MaritalStatus = ToTitleCase(employee.MaritalStatus);
            employee.BloodGroup = ToTitleCase(employee.BloodGroup);
            employee.Nationality = ToTitleCase(employee.Nationality);
            employee.PhysicallyChallenged = ToTitleCase(employee.PhysicallyChallenged);
            employee.EmergencyContact1Relationship = ToTitleCase(employee.EmergencyContact1Relationship);
            employee.EmergencyContact2Relationship = ToTitleCase(employee.EmergencyContact2Relationship);
            employee.Allergies = ToTitleCase(employee.Allergies);
            employee.Medications = ToTitleCase(employee.Medications);
            employee.PassportNationality = ToTitleCase(employee.PassportNationality);
            employee.PassportPlaceOfIssue = ToTitleCase(employee.PassportPlaceOfIssue);
            employee.Sons = ToTitleCase(employee.Sons);
            employee.Daughters = ToTitleCase(employee.Daughters);
            employee.PhotoContentType = ToTitleCase(employee.PhotoContentType);
        }

        public async Task<bool> RegisterEmployee(EmployeeDetails employee)
        {
            try
            {
                CapitalizeEmployeeDetails(employee);

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
            }catch(HttpRequestException ex)
    {
                Console.WriteLine($"HTTP error: {ex.Message}");
                return false;
            }
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

        public async Task<List<EmployeeDetails>> GetAllEmployees(int page = 1, int pageSize = 50, bool includePhotos = false)
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
        public async Task<(byte[] PhotoBytes, string ContentType)> GetEmployeePhoto(string employeeId)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"EmployeeDetails/GetEmployeePhoto/{employeeId}")));

            response.EnsureSuccessStatusCode();

            var photoData = await response.Content.ReadFromJsonAsync<EmployeePhotoResponse>();
            var base64Content = photoData.PhotoBase64Content;
            var contentType = photoData.PhotoContentType;
            var photoBytes = Convert.FromBase64String(base64Content);
            return (photoBytes, contentType);
        }


        public class EmployeePhotoResponse
        {
            public string PhotoBase64Content { get; set; }
            public string PhotoContentType { get; set; }
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
            CapitalizeEmployeeDetails(employee);

            var response = await _retryPolicy.ExecuteAsync(() =>
                _timeoutPolicy.ExecuteAsync(() =>
                    _httpClient.PostAsJsonAsync("EmployeeDetails/UpdateEmployeeDetails", employee)));

            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                _cache.Remove($"Employee_{employee.EmployeeID}");
                foreach (var suffix in new[] { "", "_Photos_true", "_Photos_false" })
                {
                    _cache.Remove($"Employee_{employee.EmployeeID}{suffix}");
                }

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



        //    public async Task<bool> CreateEmployeeBasicDetails(EmployeeBasicDetails employee)
        //{
        //    try
        //    {
        //        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        //        var response = await _retryPolicy.ExecuteAsync(() =>
        //            _timeoutPolicy.ExecuteAsync(() =>
        //                _httpClient.PostAsJsonAsync("api/EmployeeBasicDetails", employee)));

        //        Console.WriteLine($"CreateEmployeeBasicDetails({employee.EmployeeID}) took {stopwatch.ElapsedMilliseconds}ms");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            _cache.Remove("AllEmployeeBasicDetails");
        //            _cache.Remove($"EmployeeBasic_{employee.EmployeeID}");
        //            return true;
        //        }

        //        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        //        {
        //            Console.WriteLine($"Employee with ID {employee.EmployeeID} already exists");
        //            return false;
        //        }

        //        Console.WriteLine($"Error creating employee basic details: {response.StatusCode} - {response.ReasonPhrase}");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception creating employee basic details ({employee.EmployeeID}): {ex.Message}");
        //        return false;
        //    }
        //}
        public async Task<bool> CreateEmployeeBasicDetails(EmployeeBasicDetails employee, string userEmail, string userPassword)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                // Save employee details
                employee.TypeOfEmployment = ToTitleCase(employee.TypeOfEmployment);
                employee.Level = ToTitleCase(employee.Level);
                employee.Designation = ToTitleCase(employee.Designation);
                employee.JoiningLocation = ToTitleCase(employee.JoiningLocation);
                employee.CurrentEmail = employee.CurrentEmail?.ToLower();
                var saveResponse = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("api/EmployeeBasicDetails", employee)));

                _logger.LogInformation("CreateEmployeeBasicDetails({EmployeeID}) took {ElapsedMilliseconds}ms", employee.EmployeeID, stopwatch.ElapsedMilliseconds);

                if (!saveResponse.IsSuccessStatusCode)
                {
                    if (saveResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        _logger.LogWarning("Employee with ID {EmployeeID} already exists", employee.EmployeeID);
                        return false;
                    }
                    _logger.LogError("Error creating employee basic details: {StatusCode} - {ReasonPhrase}", saveResponse.StatusCode, saveResponse.ReasonPhrase);
                    return false;
                }

                // Clear cache
                _cache.Remove("AllEmployeeBasicDetails");
                _cache.Remove($"EmployeeBasic_{employee.EmployeeID}");

                // Send credentials email via API
                var emailRequest = new
                {
                    ToEmail = employee.CurrentEmail,
                    Username = userEmail,
                    Password = userPassword,
                    EmployeeId = employee.EmployeeID
                };

                var emailResponse = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("api/Email/sendCredentials", emailRequest)));

                if (!emailResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to send credentials email to {Email}: {StatusCode} - {ReasonPhrase}", employee.CurrentEmail, emailResponse.StatusCode, emailResponse.ReasonPhrase);
                    return false; // Consider whether to fail the entire operation if email fails
                }

                _logger.LogInformation("Credentials email sent to {Email} for EmployeeID {EmployeeID}", employee.CurrentEmail, employee.EmployeeID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception creating employee basic details ({EmployeeID})", employee.EmployeeID);
                throw;
            }
        }
        // Get EmployeeBasicDetails by ID
        public async Task<EmployeeBasicDetails> GetEmployeeBasicDetails(string employeeId)
        {
            string cacheKey = $"EmployeeBasic_{employeeId}";
            if (_cache.TryGetValue(cacheKey, out EmployeeBasicDetails cachedEmployee))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployee;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"api/EmployeeBasicDetails/{employeeId}")));

                Console.WriteLine($"GetEmployeeBasicDetails({employeeId}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employee = await response.Content.ReadFromJsonAsync<EmployeeBasicDetails>();
                    if (employee != null)
                    {
                        _cache.Set(cacheKey, employee, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        });
                        return employee;
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Employee basic details not found for ID: {employeeId}");
                }
                else
                {
                    Console.WriteLine($"Error fetching employee basic details ({employeeId}): {response.StatusCode}");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception fetching employee basic details ({employeeId}): {ex.Message}");
                return null;
            }
        }

        // Get all EmployeeBasicDetails
        public async Task<List<EmployeeBasicDetails>> GetAllEmployeeBasicDetails(int page = 1, int pageSize = 50)
        {
            string cacheKey = $"AllEmployeeBasicDetails_{page}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<EmployeeBasicDetails> cachedEmployees))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployees ?? new List<EmployeeBasicDetails>();
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"api/EmployeeBasicDetails?page={page}&pageSize={pageSize}")));

                Console.WriteLine($"GetAllEmployeeBasicDetails(page={page}, pageSize={pageSize}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employees = await response.Content.ReadFromJsonAsync<List<EmployeeBasicDetails>>() ?? new List<EmployeeBasicDetails>();

                    // Validate and log incomplete records
                    foreach (var emp in employees)
                    {
                        if (string.IsNullOrEmpty(emp.EmployeeID) || string.IsNullOrEmpty(emp.CurrentEmail))
                        {
                            Console.WriteLine($"Invalid employee basic data: ID={emp.EmployeeID}, Email={emp.CurrentEmail}");
                        }
                        if (!string.IsNullOrEmpty(emp.EmployeeID))
                        {
                            _cache.Set($"EmployeeBasic_{emp.EmployeeID}", emp, new MemoryCacheEntryOptions
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

                Console.WriteLine($"Error fetching all employee basic details: {response.StatusCode}");
                return new List<EmployeeBasicDetails>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllEmployeeBasicDetails: {ex.Message}");
                return new List<EmployeeBasicDetails>();
            }
        }

        // Update EmployeeBasicDetails
        public async Task<bool> UpdateEmployeeBasicDetails(EmployeeBasicDetails employee)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PutAsJsonAsync($"api/EmployeeBasicDetails/{employee.EmployeeID}", employee)));

                Console.WriteLine($"UpdateEmployeeBasicDetails({employee.EmployeeID}) took {stopwatch.ElapsedMilliseconds}ms");
                employee.TypeOfEmployment = ToTitleCase(employee.TypeOfEmployment);
                employee.Level = ToTitleCase(employee.Level);
                employee.Designation = ToTitleCase(employee.Designation);
                employee.JoiningLocation = ToTitleCase(employee.JoiningLocation);
                employee.CurrentEmail = employee.CurrentEmail?.ToLower();
                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove("AllEmployeeBasicDetails");
                    _cache.Remove($"EmployeeBasic_{employee.EmployeeID}");
                    _cache.Remove($"EmployeeBasicByEmail_{employee.CurrentEmail?.ToLower()}");
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Employee basic details not found for ID: {employee.EmployeeID}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.WriteLine($"Bad request updating employee basic details ({employee.EmployeeID}): {response.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine($"Error updating employee basic details: {response.StatusCode}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating employee basic details ({employee.EmployeeID}): {ex.Message}");
                return false;
            }
        }

        // Delete EmployeeBasicDetails
        public async Task<bool> DeleteEmployeeBasicDetails(string employeeId)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.DeleteAsync($"api/EmployeeBasicDetails/{employeeId}")));

                Console.WriteLine($"DeleteEmployeeBasicDetails({employeeId}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove("AllEmployeeBasicDetails");
                    _cache.Remove($"EmployeeBasic_{employeeId}");
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Employee basic details not found for ID: {employeeId}");
                }
                else
                {
                    Console.WriteLine($"Error deleting employee basic details ({employeeId}): {response.StatusCode}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting employee basic details ({employeeId}): {ex.Message}");
                return false;
            }
        }

        // Get EmployeeBasicDetails by Email
        public async Task<EmployeeBasicDetails> GetEmployeeBasicDetailsByEmail(string email)
        {
            string cacheKey = $"EmployeeBasicByEmail_{email.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out EmployeeBasicDetails cachedEmployee))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedEmployee;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"api/EmployeeBasicDetails/ByEmail/{email}")));

                Console.WriteLine($"GetEmployeeBasicDetailsByEmail({email}) took {stopwatch.ElapsedMilliseconds}ms");

                if (response.IsSuccessStatusCode)
                {
                    var employee = await response.Content.ReadFromJsonAsync<EmployeeBasicDetails>();
                    if (employee != null)
                    {
                        _cache.Set(cacheKey, employee, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        });
                        if (!string.IsNullOrEmpty(employee.EmployeeID))
                        {
                            _cache.Set($"EmployeeBasic_{employee.EmployeeID}", employee, new MemoryCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(5)
                            });
                        }
                        return employee;
                    }
                }

                Console.WriteLine($"Employee basic details not found for email: {email}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception fetching employee basic details by email ({email}): {ex.Message}");
                return null;
            }
        }

    }
}