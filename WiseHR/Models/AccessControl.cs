namespace WiseHR.Models
{
    public static class AccessControl
    {
        public static readonly Dictionary<string, List<(string Name, string Url)>> RoleAccess = new()
        {
            { "admin", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Employee Data", "/home/employeeData"),
                    ("Role Management", "/home/rolemanagement"),
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    ("Add Employees", "/home/employees")
                }
            },
            { "manager", new List<(string, string)>
                {
                      ("Dashboard", "/home"),
                    ("Employee Data", "/home/employeeData"),
                    ("Role Management", "/home/rolemanagement"),
                    ("Organizational Hierarchy", "/home/hierarchy"),
                    ("Add Employees", "/home/employees")
                }
            },
            { "employee", new List<(string, string)>
                {
                    ("Dashboard", "/home"),
                    ("Registration","/registration"),
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