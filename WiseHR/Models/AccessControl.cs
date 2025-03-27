namespace WiseHR.Models
{
    public static class AccessControl
    {
        public static readonly Dictionary<string, List<(string Name, string Url)>> RoleAccess = new()
    {
        { "Admin", new List<(string, string)>
            {
                ("Role Management", "/home/rolemanagement"),
                ("View Employees", "/home/employees")            }
        },
        { "Manager", new List<(string, string)>
            {
                ("Role Management", "/home/rolemanagement"),
                ("View Employees", "/home/employees")
            }
        },
        { "Employee", new List<(string, string)>
            {
                ("View Employees", "/home")
            }
        }
    };

        public static bool HasAccess(string role, string path)
        {
            return RoleAccess.ContainsKey(role) && RoleAccess[role].Any(item => item.Url == path);
        }

        public static List<(string Name, string Url)> GetMenuForRole(string role)
        {
            return RoleAccess.ContainsKey(role) ? RoleAccess[role] : new List<(string, string)>();
        }
    }

}
