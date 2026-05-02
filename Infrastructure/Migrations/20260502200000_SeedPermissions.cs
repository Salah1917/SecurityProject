using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 SEED PERMISSIONS
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "read" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "write" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "delete" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "manage_users" }
                });

            // 🔥 MAP PERMISSIONS TO ROLES
            // Admin has all permissions
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }, // Admin - read
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }, // Admin - write
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") }, // Admin - delete
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") }, // Admin - manage_users

                    // Manager has read, write, and manage_users
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }, // Manager - read
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }, // Manager - write
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") }, // Manager - manage_users

                    // User has only read permission
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") } // User - read
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "RoleId", "PermissionId" },
                keyValues: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));
        }
    }
}
