using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using XTinyLog.Serilog.Samples.Web.Models;
using XTinyLog.Serilog.Samples.Web.Services;

namespace XTinyLog.Serilog.Samples.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;

    public UsersController(ILogger<UsersController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        _logger.LogInformation("Getting all users");
        
        try
        {
            var users = await _userService.GetAllUsersAsync();
            _logger.LogInformation("Retrieved {UserCount} users", users.Count());
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        using (LogContext.PushProperty("UserId", id))
        {
            _logger.LogInformation("Getting user {UserId}", id);

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved user {UserId}: {UserName}", id, user.Name);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with email {Email}", request.Email);

        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Invalid user creation request: {@Request}", request);
                return BadRequest("Name and Email are required");
            }

            var user = await _userService.CreateUserAsync(request);
            
            _logger.LogInformation("Successfully created user {UserId} with name {UserName}", 
                user.Id, user.Name);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User creation failed due to business rule violation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with request {@Request}", request);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(int id, UpdateUserRequest request)
    {
        using (LogContext.PushProperty("UserId", id))
        {
            _logger.LogInformation("Updating user {UserId} with request {@Request}", id, request);

            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to update non-existent user {UserId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully updated user {UserId}", id);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User update failed due to business rule violation for user {UserId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId} with request {@Request}", id, request);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        using (LogContext.PushProperty("UserId", id))
        {
            _logger.LogInformation("Deleting user {UserId}", id);

            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                {
                    _logger.LogWarning("Attempted to delete non-existent user {UserId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully deleted user {UserId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User deletion failed due to business rule violation for user {UserId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}