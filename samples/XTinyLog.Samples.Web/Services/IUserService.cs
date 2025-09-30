using XTinyLog.Samples.Web.Models;

namespace XTinyLog.Samples.Web.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(string name, string email);
    Task<bool> UpdateUserAsync(int id, string name, string email);
    Task<bool> DeleteUserAsync(int id);
}