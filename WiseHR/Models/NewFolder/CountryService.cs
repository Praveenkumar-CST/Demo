using System.Net.Http.Json;

namespace WiseHR.Models.NewFolder
{
    public class CountryService
    {
        private readonly HttpClient _http;

        public CountryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<string>> GetCountriesAsync()
        {
            var response = await _http.GetFromJsonAsync<CountryResponse>("https://countriesnow.space/api/v0.1/countries");
            return response?.Data.Select(c => c.Country).ToList() ?? new List<string>();
        }

        public class CountryResponse
        {
            public List<CountryData> Data { get; set; }
        }

        public class CountryData
        {
            public string Country { get; set; }
        }
    }

}
