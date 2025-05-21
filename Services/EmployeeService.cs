namespace WiseHR.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using WiseHR.Models;
    using MudBlazor;
    using Amazon.Runtime.Internal.Util;
    using Microsoft.Extensions.Caching.Memory;
    
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public EmployeeService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;

        }

        public async Task<bool> RegisterEmployee(EmployeeDetails employee)
        {
            var response = await _httpClient.PostAsJsonAsync("EmployeeDetails/EmployeeDetailsRegistry", employee);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            Console.WriteLine("Error registering employee");
            return false;
        }


        // Get Employee Details by ID
        //public async Task<EmployeeDetails> GetEmployeeDetails(string employeeId)
        //{
        //    try
        //    {
        //        // Make the HTTP GET request to retrieve EmployeeDetails
        //        var response = await _httpClient.GetFromJsonAsync<EmployeeDetails>($"EmployeeDetails/GetEmployeeDetails/{employeeId}");

        //        // Check if response is null (e.g., if the employee was not found)
        //        if (response == null)
        //        {
        //            Console.WriteLine("Employee details not found.");
        //            return null; // Or handle this scenario as needed
        //        }


        //        return response;
        //    }
        //    catch (Exception ex)
        //    {

        //        // Log the error for debugging
        //        Console.WriteLine($"An error occurred while fetching employee details: {ex.Message}");
        //        return null; // Handle the error gracefully



        //    }
        //}
        public async Task<EmployeeDetails> GetEmployeeDetails(string employeeId)
        {
            string cacheKey = $"Employee_{employeeId}";

            if (_cache.TryGetValue(cacheKey, out EmployeeDetails cachedEmployee))
            {
                return cachedEmployee;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<EmployeeDetails>($"EmployeeDetails/GetEmployeeDetails/{employeeId}");
                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching employee details: {ex.Message}");
                return null;
            }
        }

        // Get All Employees
        //public async Task<List<EmployeeDetails>> GetAllEmployees()
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetAsync($"EmployeeDetails/GetAllEmployees");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return await response.Content.ReadFromJsonAsync<List<EmployeeDetails>>();
        //        }
        //        else
        //        {
        //            // Handle error accordingly
        //            throw new Exception($"Error fetching employees: {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (if needed) and return an empty list or handle as required
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return new List<EmployeeDetails>();
        //    }
        //}
        public async Task<List<EmployeeDetails>> GetAllEmployees()
        {
            const string cacheKey = "AllEmployees";

            if (_cache.TryGetValue(cacheKey, out List<EmployeeDetails> cachedEmployees))
            {
                return cachedEmployees;
            }

            try
            {
                var response = await _httpClient.GetAsync("EmployeeDetails/GetAllEmployees");
                if (response.IsSuccessStatusCode)
                {
                    var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDetails>>();

                    // Store in cache for 5 minutes
                    _cache.Set(cacheKey, employees, TimeSpan.FromMinutes(5));

                    return employees;
                }
                else
                {
                    throw new Exception($"Error fetching employees: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<EmployeeDetails>();
            }
        }


        // Update Employee Details
        public async Task<bool> UpdateEmployee(EmployeeDetails employee)
        {
            var response = await _httpClient.PostAsJsonAsync($"EmployeeDetails/UpdateEmployeeDetails", employee);
            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees"); // Refresh list
                _cache.Remove($"Employee_{employee.EmployeeID}"); // Refresh individual
            }
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        // Delete Employee
        public async Task<bool> DeleteEmployee(string employeeId)
        {
            var response = await _httpClient.DeleteAsync($"EmployeeDetails/DeleteEmployeeDetails/{employeeId}");
            if (response.IsSuccessStatusCode)
            {
                _cache.Remove("AllEmployees");
                _cache.Remove($"Employee_{employeeId}");
            }
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<EmployeeDetails> GetEmployeeDetailsByEmail(string email)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<EmployeeDetails>($"EmployeeDetails/GetEmployeeDetailsByEmail/{email}");

                if (response == null)
                {
                    Console.WriteLine("Employee details not found.");
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching employee details by email: {ex.Message}");
                return null;
            }

        }


    }


}


