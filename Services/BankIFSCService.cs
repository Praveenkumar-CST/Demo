using System.Net.Http.Json;
using WiseHR.Models;
using WiseHR.Pages.Data;

namespace WiseHR.Services
{
    public class BankIFSCService
    {
        private readonly HttpClient _httpClient;

        public BankIFSCService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BankDetails> GetBankDetailsByIFSCAsync(string ifscCode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<BankDetails>($"https://ifsc.razorpay.com/{ifscCode}");
                return response;
            }
            catch
            {
                return null; // You can log or throw custom exception as needed
            }
        }
    }
}
