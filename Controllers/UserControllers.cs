using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserControllers : ControllerBase
{
    private static readonly List<User> Users = new()
    {
        new() { Id = 1, Name = "Alice Johnson", Email = "alice@example.com" },
        new() { Id = 2, Name = "Bob Smith", Email = "bob@example.com" }
    };

    // Validation Fix: ensure only valid user data is processed before creating or updating records.
    private static bool IsValidUser(User? user)
    {
        if (user is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(user.Name) || user.Name.Trim().Length < 2)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(user.Email) && new EmailAddressAttribute().IsValid(user.Email);
    }

    // Performance Fix: page list results to avoid returning the entire collection at once when the dataset grows.
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than zero.");
            }

            var result = Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving users. {ex.Message}");
        }
    }

    // Error Handling Fix: wrap the lookup in try/catch so unexpected failures do not crash the API.
    [HttpGet("{id:int}")]
    public ActionResult<User> GetUserById(int id)
    {
        try
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving the user. {ex.Message}");
        }
    }

    // Validation Fix: reject invalid payloads before mutating the in-memory data store.
    [HttpPost]
    public ActionResult<User> CreateUser([FromBody] User user)
    {
        try
        {
            if (!IsValidUser(user))
            {
                return BadRequest("User name must be at least 2 characters and email must be valid.");
            }

            var newUser = new User
            {
                Id = Users.Count == 0 ? 1 : Users.Max(u => u.Id) + 1,
                Name = user.Name.Trim(),
                Email = user.Email.Trim()
            };

            if (Users.Any(u => string.Equals(u.Email, newUser.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict("A user with this email already exists.");
            }

            Users.Add(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating the user. {ex.Message}");
        }
    }

    // Validation Fix + Error Handling Fix: validate the request and protect the update logic from runtime exceptions.
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] User user)
    {
        try
        {
            if (!IsValidUser(user))
            {
                return BadRequest("User name must be at least 2 characters and email must be valid.");
            }

            var existingUser = Users.FirstOrDefault(u => u.Id == id);
            if (existingUser is null)
            {
                return NotFound();
            }

            if (Users.Any(u => u.Id != id && string.Equals(u.Email, user.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict("Another user already exists with this email.");
            }

            existingUser.Name = user.Name.Trim();
            existingUser.Email = user.Email.Trim();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating the user. {ex.Message}");
        }
    }

    // Error Handling Fix: safely handle failed lookup and deletion attempts without throwing unhandled exceptions.
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            Users.Remove(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while deleting the user. {ex.Message}");
        }
    }
}

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
