using Microsoft.EntityFrameworkCore;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.EFCore;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class SysInitRepository : ISysInitRepository
{
    private readonly AgileConfigDbContext _context;

    public SysInitRepository(AgileConfigDbContext context)
    {
        _context = context;
    }

    public string? GetDefaultEnvironmentFromDb()
    {
        var setting = _context.Settings
            .FirstOrDefault(x => x.Id == SystemSettings.DefaultEnvironmentKey);
        return setting?.Value;
    }

    public string? GetJwtTokenSecret()
    {
        var setting = _context.Settings
            .FirstOrDefault(x => x.Id == SystemSettings.DefaultJwtSecretKey);
        return setting?.Value;
    }

    public void SaveInitSetting(Setting setting)
    {
        _context.Settings.Add(setting);
        _context.SaveChanges();
    }

    public bool InitSa(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        var newSalt = Guid.NewGuid().ToString("N");
        password = Encrypt.Md5(password + newSalt);

        EnsureSystemRoles();

        var user = new User
        {
            Id = SystemSettings.SuperAdminId,
            Password = password,
            Salt = newSalt,
            Status = UserStatus.Normal,
            Team = "",
            CreateTime = DateTime.Now,
            UserName = SystemSettings.SuperAdminUserName
        };

        _context.Users.Add(user);

        var userRole = new UserRole
        {
            Id = Guid.NewGuid().ToString("N"),
            RoleId = SystemRoleConstants.SuperAdminId,
            UserId = SystemSettings.SuperAdminId
        };

        _context.UserRoles.Add(userRole);
        _context.SaveChanges();

        return true;
    }

    public bool HasSa()
    {
        return _context.Users.Any(x => x.Id == SystemSettings.SuperAdminId);
    }

    public bool InitDefaultApp(string appName)
    {
        if (string.IsNullOrEmpty(appName))
            throw new ArgumentNullException(nameof(appName));

        var anyDefaultApp = _context.Apps.Any(x => x.Id == appName);

        if (!anyDefaultApp)
        {
            _context.Apps.Add(new App
            {
                Id = appName,
                Name = appName,
                Group = "",
                Secret = "",
                CreateTime = DateTime.Now,
                Enabled = true,
                Type = AppType.PRIVATE,
                Creator = SystemSettings.SuperAdminId
            });
            _context.SaveChanges();
        }

        return true;
    }

    private void EnsureSystemRoles()
    {
        var superAdminPermissions = Functions.GetAllPermissions();
        EnsureRole(SystemRoleConstants.SuperAdminId, "Super Administrator", superAdminPermissions);
        EnsureRolePermissions(SystemRoleConstants.SuperAdminId, superAdminPermissions);

        var adminPermissions = Functions.GetAllPermissions();
        EnsureRole(SystemRoleConstants.AdminId, "Administrator", adminPermissions);
        EnsureRolePermissions(SystemRoleConstants.AdminId, adminPermissions);

        var operatorPermissions = GetOperatorPermissions();
        EnsureRole(SystemRoleConstants.OperatorId, "Operator", operatorPermissions);
        EnsureRolePermissions(SystemRoleConstants.OperatorId, operatorPermissions);
    }

    private static List<string> GetOperatorPermissions()
    {
        return new List<string>
        {
            Functions.App_Read,
            Functions.App_Add,
            Functions.App_Edit,
            Functions.App_Delete,
            Functions.App_Auth,
            Functions.Config_Read,
            Functions.Config_Add,
            Functions.Config_Edit,
            Functions.Config_Delete,
            Functions.Config_Publish,
            Functions.Config_Offline
        };
    }

    private void EnsureRole(string id, string name, List<string> functions)
    {
        var role = _context.Roles.FirstOrDefault(x => x.Id == id);

        if (role == null)
        {
            _context.Roles.Add(new Role
            {
                Id = id,
                Name = name,
                Description = name,
                IsSystem = true,
                CreateTime = DateTime.Now
            });
        }
        else
        {
            role.Name = name;
            role.Description = name;
            role.IsSystem = true;
            role.UpdateTime = DateTime.Now;
        }
        _context.SaveChanges();
    }

    private void EnsureRolePermissions(string roleId, List<string> functionCodes)
    {
        var allFunctions = _context.Functions.ToList();
        var existingRoleFunctions = _context.RoleFunctions
            .Where(x => x.RoleId == roleId)
            .ToList();

        var functionsToAssign = new List<RoleFunction>();
        foreach (var functionCode in functionCodes)
        {
            var function = allFunctions.FirstOrDefault(f => f.Code == functionCode);
            if (function != null && !existingRoleFunctions.Any(rf => rf.FunctionId == function.Id))
            {
                functionsToAssign.Add(new RoleFunction
                {
                    Id = Guid.NewGuid().ToString("N"),
                    RoleId = roleId,
                    FunctionId = function.Id
                });
            }
        }

        if (functionsToAssign.Count > 0)
        {
            _context.RoleFunctions.AddRange(functionsToAssign);
        }

        var functionIdsToKeep = allFunctions
            .Where(f => functionCodes.Contains(f.Code ?? ""))
            .Select(f => f.Id)
            .ToList();

        var roleFunctionsToRemove = existingRoleFunctions
            .Where(rf => !functionIdsToKeep.Contains(rf.FunctionId ?? ""))
            .ToList();

        if (roleFunctionsToRemove.Count > 0)
        {
            _context.RoleFunctions.RemoveRange(roleFunctionsToRemove);
        }

        _context.SaveChanges();
    }
}
