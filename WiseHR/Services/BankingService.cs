namespace WiseHR.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using WiseHR.Models;

    public class BankingService
    {
        private readonly HttpClient _httpClient;

        public BankingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Register Banking Info
        public async Task<bool> RegisterBankingInfo(BankingInformation bankingInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("BankingInformation/BankingInfoRegistry", bankingInfo);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode)
            {
                // Return the boolean value from the response
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            // Log and return false if the registration failed
            Console.WriteLine("Error registering employee bank details");
            return false;
        }

        // Get Banking Info by Employee ID
        public async Task<BankingInformation?> GetBankingInfo(string employeeId)
        {
            return await _httpClient.GetFromJsonAsync<BankingInformation>($"BankingInformation/GetBankingInfo/{employeeId}");
        }

        // Delete Banking Info
        public async Task<bool> DeleteBankingInfo(string employeeId)
        {
            var response = await _httpClient.DeleteAsync($"BankingInformation/DeleteBankingInfo/{employeeId}");
            return response.IsSuccessStatusCode;
        }

        // Update Banking Info
        public async Task<bool> UpdateBankingInfo(BankingInformation bankingInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("BankingInformation/UpdateBankingInfo", bankingInfo);
            return response.IsSuccessStatusCode;
        }
    }
}
