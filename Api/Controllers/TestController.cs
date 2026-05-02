using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok("This is public");
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst("email")?.Value;
        return Ok(new { message = "You are authenticated", userId, email });
    }

    // 🔥 ONLY ADMIN CAN ACCESS
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Hello Admin 👑");
    }

    // 🔥 MULTIPLE ROLES
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("management")]
    public IActionResult Management()
    {
        return Ok("Admin or Manager allowed");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("secure-admin")]
    public IActionResult SecureAdmin()
    {
        return Ok("Policy-based Admin access");
    }

    // 🔥 PERMISSION-BASED ENDPOINTS
    [Authorize(Policy = "CanRead")]
    [HttpGet("read-data")]
    public IActionResult ReadData()
    {
        return Ok(new { message = "You have read permission", data = "Sensitive Data" });
    }

    [Authorize(Policy = "CanWrite")]
    [HttpPost("write-data")]
    public IActionResult WriteData([FromBody] object data)
    {
        return Ok(new { message = "You have write permission", savedData = data });
    }

    [Authorize(Policy = "CanDelete")]
    [HttpDelete("delete-data/{id}")]
    public IActionResult DeleteData(string id)
    {
        return Ok(new { message = "You have delete permission", deletedId = id });
    }

    [Authorize(Policy = "CanManageUsers")]
    [HttpGet("manage-users")]
    public IActionResult ManageUsers()
    {
        return Ok(new { message = "You can manage users", users = "List of users" });
    }

    // 🔥 GET CURRENT USER INFO
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst("email")?.Value;
        var username = User.FindFirst("username")?.Value;
        var roles = User.FindAll(ClaimTypes.Role);
        var permissions = User.FindAll("permission");

        return Ok(new
        {
            userId,
            email,
            username,
            roles = roles.Select(c => c.Value),
            permissions = permissions.Select(c => c.Value)
        });
    }
}