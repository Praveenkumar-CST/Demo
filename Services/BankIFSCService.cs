using System.Net.Http.Json;
using WiseHR.Models;
using WiseHR.Pages.Data;

namespace WiseHR.Services
{
        public class BankIFSCService
        {
            private readonly HttpClient _httpClient;
            private readonly string _apiBaseUrl;

            public BankIFSCService(HttpClient httpClient, IConfiguration configuration)
            {
                _httpClient = httpClient;
                _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7021"; // Use your backend URL
            }

            public async Task<BankDetails> GetBankDetailsByIFSCAsync(string ifscCode)
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<BankDetails>($"{_apiBaseUrl}/api/BankIFSC/{ifscCode}");
                    return response;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
