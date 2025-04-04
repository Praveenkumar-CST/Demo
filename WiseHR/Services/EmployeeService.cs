namespace WiseHR.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using WiseHR.Models;  

    public class EmployeeService
    {
        private readonly HttpClient _httpClient;

        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Register Employee
        public async Task<bool> RegisterEmployee(EmployeeDetails employee)
        {
            var response = await _httpClient.PostAsJsonAsync("EmployeeDetails/EmployeeDetailsRegistry", employee);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        // Get Employee Details by ID
        public async Task<EmployeeDetails> GetEmployeeDetails(string employeeId)
        {
            return await _httpClient.GetFromJsonAsync<EmployeeDetails>($"EmployeeDetails/GetEmployeeDetails/{employeeId}");
        }
        // Get All Employees
        public async Task<List<EmployeeDetails>> GetAllEmployees()
        {
            try
            {
                var response = await _httpClient.GetAsync("EmployeeDetails/GetAllEmployees");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<EmployeeDetails>>();
                }
                else
                {
                    // Handle error accordingly
                    throw new Exception($"Error fetching employees: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log the error (if needed) and return an empty list or handle as required
                Console.WriteLine($"Error: {ex.Message}");
                return new List<EmployeeDetails>();
            }
        }



        // Update Employee Details
        public async Task<bool> UpdateEmployee(EmployeeDetails employee)
        {
            var response = await _httpClient.PostAsJsonAsync("EmployeeDetails/UpdateEmployeeDetails", employee);
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        // Delete Employee
        public async Task<bool> DeleteEmployee(string employeeId)
        {
            var response = await _httpClient.DeleteAsync($"EmployeeDetails/DeleteEmployeeDetails/{employeeId}");
            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }

}
