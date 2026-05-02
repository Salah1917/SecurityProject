using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public List<UserRole> UserRoles { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}