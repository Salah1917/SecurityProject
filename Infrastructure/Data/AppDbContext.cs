using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(x => new { x.RoleId, x.PermissionId });

        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var managerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin" },
            new Role { Id = userRoleId, Name = "User" },
            new Role { Id = managerRoleId, Name = "Manager" }
        );

        var readPermissionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var writePermissionId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var deletePermissionId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var manageUsersPermissionId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = readPermissionId, Name = "read" },
            new Permission { Id = writePermissionId, Name = "write" },
            new Permission { Id = deletePermissionId, Name = "delete" },
            new Permission { Id = manageUsersPermissionId, Name = "manage_users" }
        );

        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = adminRoleId, PermissionId = readPermissionId },
            new RolePermission { RoleId = adminRoleId, PermissionId = writePermissionId },
            new RolePermission { RoleId = adminRoleId, PermissionId = deletePermissionId },
            new RolePermission { RoleId = adminRoleId, PermissionId = manageUsersPermissionId },

            new RolePermission { RoleId = managerRoleId, PermissionId = readPermissionId },
            new RolePermission { RoleId = managerRoleId, PermissionId = writePermissionId },

            new RolePermission { RoleId = userRoleId, PermissionId = readPermissionId }
        );
    }
}