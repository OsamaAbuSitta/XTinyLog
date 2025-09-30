using XTinyLog.Serilog.Samples.Web.Models;

namespace XTinyLog.Serilog.Samples.Web.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}