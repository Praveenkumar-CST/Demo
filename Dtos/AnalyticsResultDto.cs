using WiseHR.Models;
using WiseHRServer.Models;

namespace WiseHR.Dtos
{
    public class EmployeeAnalyticsResponseDto
    {
        public EmployeeDetails EmployeeDetails { get; set; }
        public BankingInformation BankingInformation { get; set; }
        //Experience response 
        public Experience experience { get; set; }

    }
} 