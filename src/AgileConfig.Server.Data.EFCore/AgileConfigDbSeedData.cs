using AgileConfig.Server.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace AgileConfig.Server.Data.EFCore;

public static class AgileConfigDbSeedData
{
    // Use fixed values for seed data to avoid non-deterministic model changes
    private static readonly DateTime SeedDataTimestamp = new DateTime(2026, 3, 11, 9, 36, 21, DateTimeKind.Utc);
    private static readonly string AdminSalt = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6";
    private static readonly string AdminUserRoleId = "admin-role-001";

    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default admin user
        var adminId = "admin";
        var adminPassword = GeneratePasswordHash("123456", AdminSalt);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            UserName = "admin",
            Password = adminPassword,
            Salt = AdminSalt,
            CreateTime = SeedDataTimestamp,
            Status = UserStatus.Normal,
            Source = UserSource.Normal,
            Team = ""
        });

        // Seed default roles
        var adminRoleId = "001";
        var operatorRoleId = "002";

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = adminRoleId,
                Name = "Administrator",
                Description = "System Administrator",
                IsSystem = true,
                CreateTime = SeedDataTimestamp
            },
            new Role
            {
                Id = operatorRoleId,
                Name = "Operator",
                Description = "System Operator",
                IsSystem = true,
                CreateTime = SeedDataTimestamp
            }
        );

        // Seed user-role mapping
        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            Id = AdminUserRoleId,
            UserId = adminId,
            RoleId = adminRoleId
        });

        // Seed default functions
        var functions = new[]
        {
            new Function { Id = "001", Name = "App.Add", Description = "Add Application" },
            new Function { Id = "002", Name = "App.Edit", Description = "Edit Application" },
            new Function { Id = "003", Name = "App.Delete", Description = "Delete Application" },
            new Function { Id = "004", Name = "Config.Add", Description = "Add Configuration" },
            new Function { Id = "005", Name = "Config.Edit", Description = "Edit Configuration" },
            new Function { Id = "006", Name = "Config.Delete", Description = "Delete Configuration" },
            new Function { Id = "007", Name = "Config.Publish", Description = "Publish Configuration" },
            new Function { Id = "008", Name = "Config.Rollback", Description = "Rollback Configuration" },
            new Function { Id = "009", Name = "User.Add", Description = "Add User" },
            new Function { Id = "010", Name = "User.Edit", Description = "Edit User" },
            new Function { Id = "011", Name = "User.Delete", Description = "Delete User" },
            new Function { Id = "012", Name = "Node.Add", Description = "Add Node" },
            new Function { Id = "013", Name = "Node.Delete", Description = "Delete Node" },
            new Function { Id = "014", Name = "SysLog.View", Description = "View System Log" }
        };
        modelBuilder.Entity<Function>().HasData(functions);

        // Seed role-function mappings for admin with static IDs
        var roleFunctions = new[]
        {
            new RoleFunction { Id = "rf-001", RoleId = adminRoleId, FunctionId = "001" },
            new RoleFunction { Id = "rf-002", RoleId = adminRoleId, FunctionId = "002" },
            new RoleFunction { Id = "rf-003", RoleId = adminRoleId, FunctionId = "003" },
            new RoleFunction { Id = "rf-004", RoleId = adminRoleId, FunctionId = "004" },
            new RoleFunction { Id = "rf-005", RoleId = adminRoleId, FunctionId = "005" },
            new RoleFunction { Id = "rf-006", RoleId = adminRoleId, FunctionId = "006" },
            new RoleFunction { Id = "rf-007", RoleId = adminRoleId, FunctionId = "007" },
            new RoleFunction { Id = "rf-008", RoleId = adminRoleId, FunctionId = "008" },
            new RoleFunction { Id = "rf-009", RoleId = adminRoleId, FunctionId = "009" },
            new RoleFunction { Id = "rf-010", RoleId = adminRoleId, FunctionId = "010" },
            new RoleFunction { Id = "rf-011", RoleId = adminRoleId, FunctionId = "011" },
            new RoleFunction { Id = "rf-012", RoleId = adminRoleId, FunctionId = "012" },
            new RoleFunction { Id = "rf-013", RoleId = adminRoleId, FunctionId = "013" },
            new RoleFunction { Id = "rf-014", RoleId = adminRoleId, FunctionId = "014" }
        };
        modelBuilder.Entity<RoleFunction>().HasData(roleFunctions);
    }

    private static string GeneratePasswordHash(string password, string salt)
    {
        // Simple hash generation matching the existing system
        // This is a placeholder - should match the actual hash algorithm used
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
