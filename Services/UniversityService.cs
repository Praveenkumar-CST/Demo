namespace WiseHR.Services
{
    using System.Net.Http.Json;
    using System.Threading;

    public class UniversityServices
    {
        private readonly HttpClient _httpClient;

        public UniversityServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<University>> SearchUniversitiesAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<University>>(
                    $"http://universities.hipolabs.com/search?name={Uri.EscapeDataString(name)}", cancellationToken);
                return response ?? new List<University>();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Error fetching universities: {ex.Message}");
                return new List<University>();
            }
        }

        public async Task<List<College>> SearchCollegesAsync(string name, string university, CancellationToken cancellationToken = default)
        {
            try
            {
                // Example using Indian Colleges API; adjust if using a different API
                var url = $"https://colleges-api.onrender.com/colleges?search={Uri.EscapeDataString(name)}";

                // If the API supports filtering by university, append it here
                // For now, we're not filtering by university due to API limitations
                var response = await _httpClient.GetFromJsonAsync<CollegeResponse>(url, cancellationToken);
                return response?.Colleges ?? new List<College>();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Error fetching colleges: {ex.Message}");
                return new List<College>();
            }
        }
    }

    public class University
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string Domain { get; set; }
        public string Web_page { get; set; }
    }

    public class College
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
    }

    public class CollegeResponse
    {
        public List<College> Colleges { get; set; }
        public int Count { get; set; }
        public int CurrentPage { get; set; }
        public int Pages { get; set; }
    }
}
