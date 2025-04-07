namespace WiseHR.Services
{
    using Microsoft.AspNetCore.Components.Forms;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using WiseHR.Models;
    using static WiseHR.Models.BankingInformation;

    public class BankingService
    {
        private readonly HttpClient _httpClient;

        public BankingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        //Register Banking Info
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
            var response = await _httpClient.GetAsync($"BankingInformation/GetBankingInfo/{employeeId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch banking info: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("Received empty response for banking info.");
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<BankingInformation>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deserialization failed: " + ex.Message);
                return null;
            }
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
        // Check if Banking Info Exists
       
      
    }
}



