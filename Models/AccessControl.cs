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
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    //("Add Employees", "/home/employees"),
                    ("Holiday Calendar", "/calendar"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*"),
                    ("HIDE_personalEditForm", "/personalEditForm/.*"),
                    ("HIDE_bankEditForm", "/bankEditForm/.*"),
                    ("HIDE_experienceEditForm", "/experienceEditForm/.*"),
<<<<<<< HEAD
                    ("HIDE_Registration","/registration"),
                    ("HIDE_confirmationPage","/confirmation"),
                    ("HIDE_workExperience","/workExperience"),
                    ("HIDE_employee-form","/employee-form"),
                    ("HIDE_bankingForm","/bankingForm"),
                    ("HIDE_EmployeeProfile", "/employee/.*")


=======
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_Fortnight Report", "/fortnight-report"),
                    ("ReportList", "/reports")
>>>>>>> 2336a7881d8675e0f5cef80c8bea954dadf056f6
                }
            },
            {
                "manager", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    ("Role Management", "/home/rolemanagement"),
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    ("Holiday Calendar", "/calendar"),
                    //("Add Employees", "/home/employees"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*"),
                    ("HIDE_personalEditForm", "/personalEditForm/.*"),
                    ("HIDE_bankEditForm", "/bankEditForm/.*"),
                    ("HIDE_experienceEditForm", "/experienceEditForm/.*"),
<<<<<<< HEAD
                    ("HIDE_Registration","/registration"),
                    ("HIDE_confirmationPage","/confirmation"),
                    ("HIDE_workExperience","/workExperience"),
                    ("HIDE_employee-form","/employee-form"),
                    ("HIDE_bankingForm","/bankingForm"),
                    ("HIDE_EmployeeProfile", "/employee/.*")



=======
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("HIDE_Fortnight Report", "/fortnight-report"),
                    ("ReportList", "/reports")
>>>>>>> 2336a7881d8675e0f5cef80c8bea954dadf056f6
                }
            },
            {
                "employee", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Holiday Calendar", "/user"),
<<<<<<< HEAD
                    ("HIDE_Registration","/registration"),
                    ("HIDE_confirmationPage","/confirmation"),
                    ("HIDE_workExperience","/workExperience"),
                    ("HIDE_employee-form","/employee-form"),
                    ("HIDE_bankingForm","/bankingForm"),
                    ("HIDE_EmployeeProfile", "/employee/.*")

=======
                    ("HIDE_Registration", "/registration"),
                    ("HIDE_confirmationPage", "/confirmation"),
                    ("HIDE_workExperience", "/workExperience"),
                    ("HIDE_employee-form", "/employee-form"),
                    ("HIDE_bankingForm", "/bankingForm"),
                    ("ReportList", "/reports")
>>>>>>> 2336a7881d8675e0f5cef80c8bea954dadf056f6
                }
            },
            {
                "mentor", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Fortnight Report", "/fortnight-report")
                }
            }
        };

        public static List<(string Name, string Url)> GetMenuForRole(string role)
        {
            var normalizedRole = role?.ToLowerInvariant();
            return RoleAccess.FirstOrDefault(r => r.Key.ToLowerInvariant() == normalizedRole).Value
                ?? new List<(string, string)>();
        }
    }
}
