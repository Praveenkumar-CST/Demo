namespace WiseHR.Models
{
    public static class AccessControl
    {
        public static readonly Dictionary<string, List<(string Name, string Url)>> RoleAccess = new()
        {
            { "admin", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    ("Role Management", "/home/rolemanagement"),
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    ("Add Employees", "/home/employees"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*")  // You can match the pattern here
                }
            },
            { "manager", new List<(string, string)>
                {
                     ("Dashboard", "/home"),
                    ("Employee List", "/home/employeeList"),
                    ("Role Management", "/home/rolemanagement"),
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    ("Add Employees", "/home/employees"),
                    ("HIDE_employeeDetailsData", "/employeeDetailsData/.*")  // You can match the pattern here

                }
            },
            { "employee", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Registration","/registration"),
                    ("HIDE_confirmationPage","/confirmation")

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