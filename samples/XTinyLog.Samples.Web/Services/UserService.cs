using XTinyLog.Samples.Web.Models;

namespace XTinyLog.Samples.Web.Services;

/// <summary>
/// User service demonstrating XTinyLog integration in business logic
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationLogger _appLogger;
    private readonly ErrorLogger _errorLogger;
    private static readonly List<User> _users = new();
    private static int _nextId = 1;

    public UserService(ApplicationLogger appLogger, ErrorLogger errorLogger)
    {
        _appLogger = appLogger;
        _errorLogger = errorLogger;
        
        // Initialize with sample data
        if (!_users.Any())
        {
            InitializeSampleData();
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        await _appLogger.Logger.DebugAsync("UserService.GetAllUsersAsync called");
        
        // Simulate database delay
        await Task.Delay(50);
        
        await _appLogger.Logger.InfoAsync($"Retrieved {_users.Count} users from service");
        return _users.ToList();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await _appLogger.Logger.DebugAsync($"UserService.GetUserByIdAsync called for ID: {id}");
        
        // Simulate database delay
        await Task.Delay(25);
        
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            await _appLogger.Logger.WarnAsync($"User not found in service: {id}");
        }
        else
        {
            await _appLogger.Logger.DebugAsync($"Found user in service: {user.Name} (ID: {id})");
        }
        
        return user;
    }

    public async Task<User> CreateUserAsync(string name, string email)
    {
        await _appLogger.Logger.InfoAsync($"UserService.CreateUserAsync called for: {name}");
        
        try
        {
            // Simulate validation
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty");
            }
            
            if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                await _appLogger.Logger.WarnAsync($"Duplicate email attempted: {email}");
                throw new InvalidOperationException("Email already exists");
            }
            
            // Simulate database delay
            await Task.Delay(100);
            
            var user = new User
            {
                Id = _nextId++,
                Name = name,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
            
            _users.Add(user);
            
            await _appLogger.Logger.InfoAsync($"Created user in service: {user.Name} (ID: {user.Id})");
            return user;
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error creating user {name}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, string name, string email)
    {
        await _appLogger.Logger.InfoAsync($"UserService.UpdateUserAsync called for ID: {id}");
        
        try
        {
            // Simulate database delay
            await Task.Delay(75);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                await _appLogger.Logger.WarnAsync($"User not found for update in service: {id}");
                return false;
            }
            
            // Check for duplicate email (excluding current user)
            if (_users.Any(u => u.Id != id && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                await _appLogger.Logger.WarnAsync($"Duplicate email attempted in update: {email}");
                throw new InvalidOperationException("Email already exists");
            }
            
            var oldName = user.Name;
            var oldEmail = user.Email;
            
            user.Name = name;
            user.Email = email;
            
            await _appLogger.Logger.InfoAsync($"Updated user in service: {oldName} -> {name}, {oldEmail} -> {email} (ID: {id})");
            return true;
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error updating user {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        await _appLogger.Logger.InfoAsync($"UserService.DeleteUserAsync called for ID: {id}");
        
        try
        {
            // Simulate database delay
            await Task.Delay(50);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                await _appLogger.Logger.WarnAsync($"User not found for deletion in service: {id}");
                return false;
            }
            
            _users.Remove(user);
            
            await _appLogger.Logger.InfoAsync($"Deleted user from service: {user.Name} (ID: {id})");
            return true;
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error deleting user {id}: {ex.Message}");
            throw;
        }
    }

    private void InitializeSampleData()
    {
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new User { Id = _nextId++, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new User { Id = _nextId++, Name = "Bob Johnson", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        });
    }
}