using AgileConfig.Server.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace AgileConfig.Server.Data.EFCore;

public static class AgileConfigDbSeedData
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default admin user
        var adminId = "admin";
        var adminSalt = Guid.NewGuid().ToString("N");
        var adminPassword = GeneratePasswordHash("123456", adminSalt);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            UserName = "admin",
            Password = adminPassword,
            Salt = adminSalt,
            CreateTime = DateTime.Now,
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
                CreateTime = DateTime.Now
            },
            new Role
            {
                Id = operatorRoleId,
                Name = "Operator",
                Description = "System Operator",
                IsSystem = true,
                CreateTime = DateTime.Now
            }
        );

        // Seed user-role mapping
        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            Id = Guid.NewGuid().ToString(),
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

        // Seed role-function mappings for admin
        var roleFunctions = functions.Select(f => new RoleFunction
        {
            Id = Guid.NewGuid().ToString(),
            RoleId = adminRoleId,
            FunctionId = f.Id
        }).ToArray();
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
