using System.Threading.Tasks;
using WiseHR.Models;

namespace WiseHR.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task UpdateUserRoleAsync(string id, string role);
        Task DeleteUserAsync(string id);
        Task<User> CreateUserAsync(string email, string password); 

    }
}