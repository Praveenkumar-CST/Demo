using System.Threading.Tasks;
using WiseHR.Models;

namespace WiseHR.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string role);

        Task UpdateUserRoleAsync(string id, string role);
        Task DeleteUserAsync(string id);
        Task<User> CreateUserAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
    }
}