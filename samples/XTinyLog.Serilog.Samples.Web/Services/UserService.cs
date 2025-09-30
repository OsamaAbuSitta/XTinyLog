using XTinyLog.Serilog.Samples.Web.Models;

namespace XTinyLog.Serilog.Samples.Web.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private static readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "John Doe", Email = "john.doe@example.com", CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new User { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com", CreatedAt = DateTime.UtcNow.AddDays(-25) },
        new User { Id = 3, Name = "Bob Johnson", Email = "bob.johnson@example.com", CreatedAt = DateTime.UtcNow.AddDays(-20) },
        new User { Id = 4, Name = "Alice Brown", Email = "alice.brown@example.com", CreatedAt = DateTime.UtcNow.AddDays(-15) },
        new User { Id = 5, Name = "Charlie Wilson", Email = "charlie.wilson@example.com", CreatedAt = DateTime.UtcNow.AddDays(-10) }
    };
    private static int _nextId = 6;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogDebug("Retrieving all users from data store");
        
        // Simulate async database operation
        await Task.Delay(50);
        
        var activeUsers = _users.Where(u => u.IsActive).ToList();
        _logger.LogDebug("Found {ActiveUserCount} active users out of {TotalUserCount} total users", 
            activeUsers.Count, _users.Count);
        
        return activeUsers;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving user {UserId} from data store", id);
        
        // Simulate async database operation
        await Task.Delay(25);
        
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found or inactive", id);
        }
        else
        {
            _logger.LogDebug("Found user {UserId}: {UserName} ({UserEmail})", id, user.Name, user.Email);
        }
        
        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogDebug("Creating new user with email {Email}", request.Email);
        
        // Validate email uniqueness
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && u.IsActive))
        {
            _logger.LogWarning("Attempted to create user with duplicate email {Email}", request.Email);
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }
        
        // Simulate async database operation
        await Task.Delay(100);
        
        var user = new User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        _users.Add(user);
        
        _logger.LogInformation("Created new user {UserId} with name {UserName} and email {UserEmail}", 
            user.Id, user.Name, user.Email);
        
        return user;
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        _logger.LogDebug("Updating user {UserId} with request {@UpdateRequest}", id, request);
        
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found for update", id);
            return null;
        }
        
        // Check for email conflicts if email is being updated
        if (!string.IsNullOrWhiteSpace(request.Email) && 
            !request.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && u.IsActive && u.Id != id))
            {
                _logger.LogWarning("Attempted to update user {UserId} with duplicate email {Email}", id, request.Email);
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }
        }
        
        // Simulate async database operation
        await Task.Delay(75);
        
        var originalUser = new { user.Name, user.Email, user.IsActive };
        
        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Name))
            user.Name = request.Name;
        
        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;
        
        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;
        
        user.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updated user {UserId}: Name: {OldName} -> {NewName}, Email: {OldEmail} -> {NewEmail}, Active: {OldActive} -> {NewActive}",
            id, originalUser.Name, user.Name, originalUser.Email, user.Email, originalUser.IsActive, user.IsActive);
        
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        _logger.LogDebug("Deleting user {UserId}", id);
        
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        if (user == null)
        {
            _logger.LogDebug("User {UserId} not found for deletion", id);
            return false;
        }
        
        // Simulate async database operation
        await Task.Delay(50);
        
        // Soft delete by marking as inactive
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Soft deleted user {UserId} ({UserName})", id, user.Name);
        
        return true;
    }
}