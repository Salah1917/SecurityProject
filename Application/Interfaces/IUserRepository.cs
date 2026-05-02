using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task UpdateRefreshTokenAsync(RefreshToken token);
    Task AddRefreshTokenAsync(RefreshToken token);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task AddUserRoleAsync(Guid userId, Guid roleId);
}