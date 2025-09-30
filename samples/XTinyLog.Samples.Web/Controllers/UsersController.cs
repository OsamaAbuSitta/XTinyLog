using Microsoft.AspNetCore.Mvc;
using XTinyLog.Samples.Web.Models;
using XTinyLog.Samples.Web.Services;

namespace XTinyLog.Samples.Web.Controllers;

/// <summary>
/// Controller demonstrating XTinyLog usage in API endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ApplicationLogger _appLogger;
    private readonly ErrorLogger _errorLogger;

    public UsersController(IUserService userService, ApplicationLogger appLogger, ErrorLogger errorLogger)
    {
        _userService = userService;
        _appLogger = appLogger;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        try
        {
            await _appLogger.Logger.DebugAsync("GetUsers endpoint called");
            var users = await _userService.GetAllUsersAsync();
            await _appLogger.Logger.InfoAsync($"Retrieved {users.Count()} users");
            return Ok(users);
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error in GetUsers: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        try
        {
            await _appLogger.Logger.DebugAsync($"GetUser endpoint called for ID: {id}");
            
            if (id <= 0)
            {
                await _appLogger.Logger.WarnAsync($"Invalid user ID requested: {id}");
                return BadRequest("Invalid user ID");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                await _appLogger.Logger.WarnAsync($"User not found: {id}");
                return NotFound();
            }

            await _appLogger.Logger.InfoAsync($"Retrieved user: {user.Name} (ID: {id})");
            return Ok(user);
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error in GetUser for ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        try
        {
            await _appLogger.Logger.InfoAsync($"CreateUser endpoint called for: {request.Name}");
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                await _appLogger.Logger.WarnAsync("CreateUser called with empty name");
                return BadRequest("Name is required");
            }

            var user = await _userService.CreateUserAsync(request.Name, request.Email);
            await _appLogger.Logger.InfoAsync($"Created new user: {user.Name} (ID: {user.Id})");
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error in CreateUser for {request.Name}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            await _appLogger.Logger.InfoAsync($"UpdateUser endpoint called for ID: {id}");
            
            if (id <= 0)
            {
                await _appLogger.Logger.WarnAsync($"Invalid user ID for update: {id}");
                return BadRequest("Invalid user ID");
            }

            var success = await _userService.UpdateUserAsync(id, request.Name, request.Email);
            if (!success)
            {
                await _appLogger.Logger.WarnAsync($"User not found for update: {id}");
                return NotFound();
            }

            await _appLogger.Logger.InfoAsync($"Updated user ID: {id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error in UpdateUser for ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _appLogger.Logger.InfoAsync($"DeleteUser endpoint called for ID: {id}");
            
            if (id <= 0)
            {
                await _appLogger.Logger.WarnAsync($"Invalid user ID for deletion: {id}");
                return BadRequest("Invalid user ID");
            }

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                await _appLogger.Logger.WarnAsync($"User not found for deletion: {id}");
                return NotFound();
            }

            await _appLogger.Logger.InfoAsync($"Deleted user ID: {id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error in DeleteUser for ID {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}