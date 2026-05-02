using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;

    public AuthService(
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IUserRepository userRepository)
    {
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _userRepository = userRepository;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var userRole = await _userRepository.GetRoleByNameAsync("User");

        if (userRole == null)
            throw new InvalidOperationException("Default 'User' role not found in database. Please ensure roles are seeded.");

        await _userRepository.AddUserRoleAsync(user.Id, userRole.Id);

        // Reload user with roles and permissions for token generation
        var userWithRoles = await _userRepository.GetByEmailAsync(user.Email);
        if (userWithRoles == null)
            throw new InvalidOperationException("Failed to retrieve registered user");

        var accessToken = _jwtService.GenerateAccessToken(userWithRoles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = user.Id
        };

        await _userRepository.AddRefreshTokenAsync(newToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = user.Id
        };

        await _userRepository.AddRefreshTokenAsync(newToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty");

        var tokenEntity = await _userRepository.GetRefreshTokenAsync(refreshToken);

        if (tokenEntity == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (tokenEntity.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked");

        if (tokenEntity.Expires < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired");

        // revoke old token
        tokenEntity.IsRevoked = true;
        await _userRepository.UpdateRefreshTokenAsync(tokenEntity);

        var user = tokenEntity.User;

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserId = user.Id
        };

        await _userRepository.AddRefreshTokenAsync(newToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}