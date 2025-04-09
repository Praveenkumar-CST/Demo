namespace WiseHR.Services
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using WiseHR.Models;
    using MudBlazor;

    public class EmployeeService
    {
        private readonly HttpClient _httpClient;

        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

            public async Task<bool> RegisterEmployee(EmployeeDetails employee)
            {
                // Calling the backend API to register employee details
                var response = await _httpClient.PostAsJsonAsync("EmployeeDetails/EmployeeDetailsRegistry", employee);
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                //Snackbar.Add(await response.Content.ReadAsStringAsync());
                // Check if the response status is successful
                if (response.IsSuccessStatusCode)
                {
                    // Return the boolean value from the response
                    return await response.Content.ReadFromJsonAsync<bool>();
                }

                // Log and return false if the registration failed
                Console.WriteLine("Error registering employee");
                return false;
            }


            // Get Employee Details by ID
            public async Task<EmployeeDetails> GetEmployeeDetails(string employeeId)
            {
                try
                {
                    // Make the HTTP GET request to retrieve EmployeeDetails
                    var response = await _httpClient.GetFromJsonAsync<EmployeeDetails>($"EmployeeDetails/GetEmployeeDetails/{employeeId}");

                    // Check if response is null (e.g., if the employee was not found)
                    if (response == null)
                    {
                        Console.WriteLine("Employee details not found.");
                        return null; // Or handle this scenario as needed
                    }

                    // Return the deserialized employee details
                    return response;
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    Console.WriteLine($"An error occurred while fetching employee details: {ex.Message}");
                    return null; // Handle the error gracefully
                }
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

