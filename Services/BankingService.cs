namespace WiseHR.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Components.Forms;
    using WiseHR.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Polly;
    using Polly.Extensions.Http;

    public class BankingService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public BankingService(HttpClient httpClient, IMemoryCache cache)
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

        public async Task<bool> RegisterBankingInfo(BankingInformation bankingInfo)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("BankingInformation/BankingInfoRegistry", bankingInfo)));

                Console.WriteLine($"RegisterBankingInfo took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove($"BankingInfo_{bankingInfo.EmployeeID}");
                    _cache.Remove("AllBankingInfo");
                    return await response.Content.ReadFromJsonAsync<bool>();
                }

                Console.WriteLine($"Error registering banking info for EmployeeID: {bankingInfo.EmployeeID}, Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering banking info for EmployeeID: {bankingInfo.EmployeeID}, Message: {ex.Message}");
                return false;
            }
        }

        public async Task<BankingInformation?> GetBankingInfo(string employeeId)
        {
            string cacheKey = $"BankingInfo_{employeeId}";
            if (_cache.TryGetValue(cacheKey, out BankingInformation cachedInfo))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedInfo;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.GetAsync($"BankingInformation/GetBankingInfo/{employeeId}")));

                Console.WriteLine($"GetBankingInfo({employeeId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var bankingInfo = await response.Content.ReadFromJsonAsync<BankingInformation>();
                    if (bankingInfo != null)
                    {
                        _cache.Set(cacheKey, bankingInfo, new MemoryCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        });
                    }
                    return bankingInfo;
                }

                Console.WriteLine($"Banking info not found for EmployeeID: {employeeId}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching banking info for EmployeeID: {employeeId}, Message: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BankingInformation>> GetBankingInfoByEmployeeIds(IEnumerable<string> employeeIds)
        {
            string cacheKey = $"BankingInfoBatch_{string.Join("_", employeeIds.OrderBy(id => id))}";
            if (_cache.TryGetValue(cacheKey, out List<BankingInformation> cachedInfos))
            {
                Console.WriteLine($"Cache hit for {cacheKey}");
                return cachedInfos;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("BankingInformation/GetBankingInfoByIds", employeeIds)));

                Console.WriteLine($"GetBankingInfoByEmployeeIds(count={employeeIds.Count()}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var bankingInfos = await response.Content.ReadFromJsonAsync<List<BankingInformation>>() ?? new List<BankingInformation>();
                    _cache.Set(cacheKey, bankingInfos, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(15)
                    });

                    // Cache individual banking info
                    foreach (var info in bankingInfos)
                    {
                        if (!string.IsNullOrEmpty(info.EmployeeID))
                        {
                            _cache.Set($"BankingInfo_{info.EmployeeID}", info, new MemoryCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromMinutes(15)
                            });
                        }
                    }

                    return bankingInfos;
                }

                throw new Exception($"Error fetching banking info by IDs: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBankingInfoByEmployeeIds: {ex.Message}");
                return new List<BankingInformation>();
            }
        }

        public async Task<bool> DeleteBankingInfo(string employeeId)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.DeleteAsync($"BankingInformation/DeleteBankingInfo/{employeeId}")));

                Console.WriteLine($"DeleteBankingInfo({employeeId}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove($"BankingInfo_{employeeId}");
                    _cache.Remove("AllBankingInfo");
                    return true;
                }

                Console.WriteLine($"Error deleting banking info for EmployeeID: {employeeId}, Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting banking info for EmployeeID: {employeeId}, Message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateBankingInfo(BankingInformation bankingInfo)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsJsonAsync("BankingInformation/UpdateBankingInfo", bankingInfo)));

                Console.WriteLine($"UpdateBankingInfo took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove($"BankingInfo_{bankingInfo.EmployeeID}");
                    _cache.Remove("AllBankingInfo");
                    return true;
                }

                Console.WriteLine($"Error updating banking info for EmployeeID: {bankingInfo.EmployeeID}, Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating banking info for EmployeeID: {bankingInfo.EmployeeID}, Message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UploadDocumentAsync(IBrowserFile file, string employeeId, string documentType)
        {
            if (file == null || file.Size == 0)
            {
                Console.WriteLine("Invalid file: File is null or empty.");
                return false;
            }

            if (file.Size > 10_000_000) // 10MB max
            {
                Console.WriteLine($"File size exceeds 10MB: {file.Size} bytes.");
                return false;
            }

            // Validate file type (e.g., PDF, PNG, JPG)
            var allowedTypes = new[] { "application/pdf", "image/png", "image/jpeg" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                Console.WriteLine($"Invalid file type: {file.ContentType}. Allowed types: {string.Join(", ", allowedTypes)}");
                return false;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10_000_000));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.Name);
                content.Add(new StringContent(employeeId), "employeeId");
                content.Add(new StringContent(documentType), "documentType");

                var response = await _retryPolicy.ExecuteAsync(() =>
                    _timeoutPolicy.ExecuteAsync(() =>
                        _httpClient.PostAsync("BankingInformation/UploadDocument", content)));

                Console.WriteLine($"UploadDocumentAsync({employeeId}, {documentType}) took {stopwatch.ElapsedMilliseconds}ms, Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _cache.Remove($"BankingInfo_{employeeId}");
                    _cache.Remove("AllBankingInfo");
                    return true;
                }

                Console.WriteLine($"Error uploading document for EmployeeID: {employeeId}, Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading document for EmployeeID: {employeeId}, Message: {ex.Message}");
                return false;
            }
        }

    }

}