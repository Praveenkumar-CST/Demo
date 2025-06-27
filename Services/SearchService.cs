using System.Net.Http.Headers;
using System.Text.Json;
using WiseHR.Dtos;
using WiseHRServer.Models;
using FuzzySharp;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace WiseHR.Services
{
    public class SearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<SearchService> _logger;
        private const int FuzzyScoreThreshold = 60;
        private SearchCacheResponse? _searchCache;
        private DateTime _lastCacheRefresh = DateTime.MinValue;
        private readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromMinutes(1);

        // Define field weights for prioritization
        private static readonly Dictionary<string, int> FieldWeights = new(StringComparer.OrdinalIgnoreCase)
        {
            { "FirstName", 100 },
            { "LastName", 95 },
            { "MiddleName", 90 },
            { "Designation", 85 },
            { "FatherName", 80 },
            { "MotherName", 75 },
            { "CurrentEmail", 70 },
            { "CurrentMobile", 65 },
            { "EmployeeID", 60 },
            { "PANNumber", 55 },
            { "AadhaarNumber", 50 },
            { "PermanentEmail", 45 },
            { "PermanentMobile", 40 },
            { "Nationality", 35 },
            { "PassportNo", 30 },
            { "JoiningLocation", 25 },
            { "Level", 20 },
            { "CurrentCity", 15 },
            { "CurrentState", 10 },
            { "PermanentCity", 5 },
            { "PermanentState", 4 },
            { "TypeOfEmployment", 3 },
            { "BloodGroup", 2 }
        };

        public SearchService(
            HttpClient httpClient, 
            IJSRuntime jsRuntime, 
            ILogger<SearchService> logger)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<List<EmployeeSearchResult>> SearchEmployees(string query)
        {
            try
            {
                // Check if cache needs refresh
                if (_searchCache == null || DateTime.UtcNow - _lastCacheRefresh > _cacheRefreshInterval)
                {
                    await RefreshCache();
                }

                if (_searchCache?.Data == null)
                {
                    return new List<EmployeeSearchResult>();
                }

                var queryLower = query?.ToLower() ?? string.Empty;
                var results = new List<EmployeeSearchResult>();

                foreach (var item in _searchCache.Data)
                {
                    var matchedFields = new List<string>();
                    Action<string?, string> checkField = (fieldValue, fieldName) =>
                    {
                        if (string.IsNullOrWhiteSpace(fieldValue) || string.IsNullOrWhiteSpace(queryLower)) return;
                        
                        var fieldValueLower = fieldValue.ToLower();
                        
                        // Check if field contains the query (case-insensitive)
                        if (fieldValueLower.Contains(queryLower))
                        {
                            if (!matchedFields.Contains(fieldName))
                                matchedFields.Add(fieldName);
                            return;
                        }

                        // If no contains match, try fuzzy match
                        var fuzzyScore = Fuzz.Ratio(queryLower, fieldValueLower);
                        if (fuzzyScore > FuzzyScoreThreshold)
                        {
                            if (!matchedFields.Contains(fieldName))
                                matchedFields.Add(fieldName);
                        }
                    };

                    // Check all relevant fields based on cache type
                    if (_searchCache.Type == "admin")
                    {
                        // Admin cache fields - check in order of priority
                        checkField(item.FirstName, "FirstName");
                        checkField(item.LastName, "LastName");
                        checkField(item.MiddleName, "MiddleName");
                        checkField(item.Designation, "Designation");
                        checkField(item.FatherName, "FatherName");
                        checkField(item.MotherName, "MotherName");
                        checkField(item.CurrentEmail, "CurrentEmail");
                        checkField(item.CurrentMobile, "CurrentMobile");
                        checkField(item.EmployeeID, "EmployeeID");
                        checkField(item.BloodGroup, "BloodGroup");
                        checkField(item.TypeOfEmployment, "TypeOfEmployment");
                        checkField(item.Level, "Level");
                        checkField(item.JoiningLocation, "JoiningLocation");
                        checkField(item.Gender, "Gender");
                        checkField(item.MaritalStatus, "MaritalStatus");
                        checkField(item.Nationality, "Nationality");
                        checkField(item.Allergies, "Allergies");
                        checkField(item.Medications, "Medications");
                        checkField(item.PhysicallyChallenged, "PhysicallyChallenged");
                        checkField(item.Sons, "Sons");
                        checkField(item.Daughters, "Daughters");
                        checkField(item.CurrentAddress, "CurrentAddress");
                        checkField(item.CurrentCity, "CurrentCity");
                        checkField(item.CurrentState, "CurrentState");
                        checkField(item.CurrentZip, "CurrentZip");
                        checkField(item.PermanentAddress, "PermanentAddress");
                        checkField(item.PermanentCity, "PermanentCity");
                        checkField(item.PermanentState, "PermanentState");
                        checkField(item.PermanentMobile, "PermanentMobile");
                        checkField(item.PermanentEmail, "PermanentEmail");
                        checkField(item.PassportFullName, "PassportFullName");
                        checkField(item.PassportNo, "PassportNo");
                        checkField(item.PassportNationality, "PassportNationality");
                        checkField(item.PassportPlaceOfIssue, "PassportPlaceOfIssue");
                        checkField(item.EmergencyContact1Name, "EmergencyContact1Name");
                        checkField(item.EmergencyContact1Relationship, "EmergencyContact1Relationship");
                        checkField(item.EmergencyContact1Address, "EmergencyContact1Address");
                        checkField(item.EmergencyContact1City, "EmergencyContact1City");
                        checkField(item.EmergencyContact1State, "EmergencyContact1State");
                        checkField(item.EmergencyContact1ZipCode, "EmergencyContact1ZipCode");
                        checkField(item.EmergencyContact1Mobile, "EmergencyContact1Mobile");
                        checkField(item.BankName, "Bank.BankName");
                        checkField(item.BankBranch, "Bank.Branch");
                        checkField(item.BankAccountHolderName, "Bank.AccountHolderName");
                        checkField(item.BankAccountNumber, "Bank.AccountNumber");
                        checkField(item.BankIFSCode, "Bank.IFSCode");
                        checkField(item.BankPhone, "Bank.Phone");
                        checkField(item.BankPANNumber, "Bank.PANNumber");
                        checkField(item.BankAadhaarNumber, "Bank.AadhaarNumber");
                        checkField(item.BankState, "Bank.State");
                        checkField(item.BankAccountType, "Bank.AccountType");
                    }
                    else
                    {
                        // User cache fields (limited access)
                        checkField(item.FirstName, "FirstName");
                        checkField(item.LastName, "LastName");
                        checkField(item.MiddleName, "MiddleName");
                        checkField(item.Designation, "Designation");
                        checkField(item.CurrentEmail, "CurrentEmail");
                        checkField(item.CurrentMobile, "CurrentMobile");
                        checkField(item.BloodGroup, "BloodGroup");
                    }

                    if (matchedFields.Any())
                    {
                        results.Add(new EmployeeSearchResult
                        {
                            Name = $"{item.FirstName} {item.MiddleName} {item.LastName}".Trim(),
                            Email = item.CurrentEmail,
                            Mobile = item.CurrentMobile,
                            Designation = item.Designation,
                            ProfileUrl = $"/employee/{item.EmployeeID}",
                            EmployeeID = item.EmployeeID,
                            MatchedFields = matchedFields,
                            ProfilePicture = !string.IsNullOrEmpty(item.PhotoBase64Content) 
                                ? $"data:{item.PhotoContentType};base64,{item.PhotoBase64Content}"
                                : null,
                            Score = CalculateScore(matchedFields)
                        });
                    }
                }

                // Sort results by score (highest first) and then by name
                var sortedResults = results.OrderByDescending(r => r.Score)
                             .ThenBy(r => r.Name)
                             .ToList();
                foreach (var result in sortedResults)

                {

                    result.Name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.Name.ToLower());

                    result.Email = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.Email?.ToLower() ?? string.Empty);

                    result.Mobile = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.Mobile?.ToLower() ?? string.Empty);

                    result.Designation = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.Designation.ToLower());

                }


                return sortedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching employees");
                return new List<EmployeeSearchResult>();
            }
        }

        private async Task RefreshCache()
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access token found for search cache refresh");
                    return;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/SearchCache");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to refresh search cache: {StatusCode}", response.StatusCode);
                    return;
                }

                _searchCache = await response.Content.ReadFromJsonAsync<SearchCacheResponse>();
                _lastCacheRefresh = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing search cache");
            }
        }

        private int CalculateScore(List<string> matchedFields)
        {
            return matchedFields.Sum(field => FieldWeights.TryGetValue(field, out var weight) ? weight : 1);
        }

        private async Task<string?> GetAccessToken()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving access token");
                return null;
            }
        }
    }

    public class SearchCacheResponse
    {
        public string Type { get; set; } = string.Empty;
        public List<SearchCacheItem> Data { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class SearchCacheItem
    {
        public string EmployeeID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? CurrentEmail { get; set; }
        public string? CurrentMobile { get; set; }
        public string? BloodGroup { get; set; }
        public string? Designation { get; set; }
        public string? TypeOfEmployment { get; set; }
        public string? Level { get; set; }
        public string? JoiningLocation { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Nationality { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? PhysicallyChallenged { get; set; }
        public string? Sons { get; set; }
        public string? Daughters { get; set; }
        public string? CurrentAddress { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentState { get; set; }
        public string? CurrentZip { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentState { get; set; }
        public string? PermanentMobile { get; set; }
        public string? PermanentEmail { get; set; }
        public string? PassportFullName { get; set; }
        public string? PassportNo { get; set; }
        public string? PassportNationality { get; set; }
        public string? PassportPlaceOfIssue { get; set; }
        public string? EmergencyContact1Name { get; set; }
        public string? EmergencyContact1Relationship { get; set; }
        public string? EmergencyContact1Address { get; set; }
        public string? EmergencyContact1City { get; set; }
        public string? EmergencyContact1State { get; set; }
        public string? EmergencyContact1ZipCode { get; set; }
        public string? EmergencyContact1Mobile { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public string? BankAccountHolderName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSCode { get; set; }
        public string? BankPhone { get; set; }
        public string? BankPANNumber { get; set; }
        public string? BankAadhaarNumber { get; set; }
        public string? BankState { get; set; }
        public string? BankAccountType { get; set; }
        public string? PhotoBase64Content { get; set; }
        public string? PhotoContentType { get; set; }
    }
} 