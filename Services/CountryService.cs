using System.Text.Json;
using WiseHR.Models;

namespace WiseHR.Services
{
    public class CountryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "aE9wRXJ0aWx3d3pPWHF1dUJZamlYbTFjdVBkMnNZZGhoS0ZlMVAyOA=="; // Replace with your actual API key

        public CountryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Country>> GetCountriesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.countrystatecity.in/v1/countries");
            request.Headers.Add("X-CSCAPI-KEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<Country>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Country>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

}
