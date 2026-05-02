using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;

    public AdminController(AppDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Assign a role to a user (Admin only)
    /// </summary>
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return NotFound("User not found");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId);

        if (role == null)
            return NotFound("Role not found");

        // Check if user already has this role
        if (user.UserRoles.Any(ur => ur.RoleId == request.RoleId))
            return BadRequest("User already has this role");

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Role '{role.Name}' assigned to user '{user.Username}'" });
    }

    /// <summary>
    /// Remove a role from a user (Admin only)
    /// </summary>
    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);

        if (userRole == null)
            return NotFound("User does not have this role");

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Role removed from user" });
    }

    /// <summary>
    /// Get all users with their roles
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _context.Roles
            .Select(r => new
            {
                r.Id,
                r.Name,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
            })
            .ToListAsync();

        return Ok(roles);
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _context.Permissions
            .Select(p => new
            {
                p.Id,
                p.Name
            })
            .ToListAsync();

        return Ok(permissions);
    }
}

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
