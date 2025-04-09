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
            try
            {
                return await _httpClient.GetFromJsonAsync<BankingInformation>($"BankingInformation/GetBankingInfo/{employeeId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching banking info: {ex.Message}");
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
        public async Task<bool> UploadDocumentAsync(IBrowserFile file, string employeeId, string documentType)
        {
            try
            {
                var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10_000_000)); // 10MB max
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.Name);
                content.Add(new StringContent(employeeId), "employeeId");
                content.Add(new StringContent(documentType), "documentType");

                var response = await _httpClient.PostAsync("BankingInformation/UploadDocument", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File upload failed: {ex.Message}");
                return false;
            }
        }


    }
}



