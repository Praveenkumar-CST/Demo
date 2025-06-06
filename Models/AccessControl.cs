using MudBlazor;

namespace WiseHR.Models
{
    public static class AccessControl
    {
        public static readonly Dictionary<string, List<(string Name, string Url)>> RoleAccess = new()
        {
            {
                "admin", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    ("Role Management", "/home/rolemanagement"),
                    ("HIDE_Create Employee","/home/employees"),
                    ("Organizational Hierarchy", "/role-hierarchy"),           
                    //("Add Employees", "/home/employees"),
                    ("Holiday Calendar", "/calendar"),
                    ("Attendance", "/AdminAttendanceView"),
                    ("Policies", "/policies"),
                    ("HIDE_admin", "/AdminAbsenceReasons"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*"),
                    ("HIDE_personalEditForm", "/personalEditForm/.*"),
                    ("HIDE_experienceEditForm","/experienceEditForm/.*"),
                    ("HIDE_bankEditForm","/bankEditForm/.*"),
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_Fortnight Report", "/fortnight-report"),
                    //("ReportList", "/reports"),
                     ("HIDE_EmployeeProfile", "/employee/.*"),
                     ("HIDE_Analytics", "/analytics"),
                     //("Assign Mentee","/managers"),
                    ("Asset","/asset"),
                    ("HIDE_product","/assets/{id:int}"),
                    ("HIDE_addasset","/add-asset"),
                    ("HIDE_user", "/assets/{assetId:int}/instance/{instanceId:int}/history")

                }
            },
            {
                "manager", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    //("Organizational Hierarchy", "/role-hierarchy"),
                    ("Holiday Calendar", "/calendar"),
                    ("Attendance", "/AdminAttendanceView"),
                    ("Policies", "/policies"),
                    ("HIDE_admin", "/AdminAbsenceReasons"),
                    //("Add Employees", "/home/employees"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*"),
                    ("HIDE_personalEditForm", "/personalEditForm/.*"),
                    ("HIDE_bankEditForm", "/bankEditForm/.*"),
                    ("HIDE_experienceEditForm", "/experienceEditForm/.*"),
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_Fortnight Report", "/fortnight-report"),
                    //("ReportList", "/reports"),
                    ("HIDE_EmployeeProfile", "/employee/.*"),                 

                }
            },    {
                "HR", new List<(string, string)>
                {
                     ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    ("Role Management", "/home/rolemanagement"),
                    //("Organizational Hierarchy", "/home/hierarchy"),
                    //("Add Employees", "/home/employees"),
                    ("Holiday Calendar", "/calendar"),
                    ("Attendance", "/AdminAttendanceView"),
                    ("Policies", "/policies"),
                    ("HIDE_admin", "/AdminAbsenceReasons"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*"),
                    ("HIDE_personalEditForm", "/personalEditForm/.*"),
                    ("HIDE_experienceEditForm","/experienceEditForm/.*"),
                    ("HIDE_bankEditForm","/bankEditForm/.*"),
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_Fortnight Report", "/fortnight-report"),
                    //("ReportList", "/reports"),
                     ("HIDE_EmployeeProfile", "/employee/.*"),
                     ("HIDE_Analytics", "/analytics"),
                     //("Assign Mentee","/managers")
                }
            },
            {
                "employee", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Holiday Calendar", "/user"),
                    ("Attendance", "/UserAttendanceView"),
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_EmployeeProfile", "/employee/.*"),                  
                }
            },
            {
                "mentor", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Fortnight Report", "/fortnight-report"),
                 
                }
            }
        };

        public static List<(string Name, string Url)> GetMenuForRole(string role)
        {
            var normalizedRole = role?.ToLowerInvariant();
            return RoleAccess.FirstOrDefault(r => r.Key.ToLowerInvariant() == normalizedRole).Value
                ?? new List<(string, string)>();
        }
        public static string GetIconForMenu(string menuName)
        {
            return menuName switch
            {
                "Dashboard" => Icons.Material.Filled.Dashboard,
                "Employee List" => Icons.Material.Filled.People,
                "Role Management" => Icons.Material.Filled.Security,
                "Organizational Hierarchy" => Icons.Material.Filled.AccountTree,
                "Holiday Calendar" => Icons.Material.Filled.Event,
                "Fortnight Report" => Icons.Material.Filled.Assignment,
                "ReportList" => Icons.Material.Filled.Assessment,
                _ => Icons.Material.Filled.Menu
            };
        }

    }
}
