using Microsoft.Extensions.DependencyInjection;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class EFCoreRepositoryServiceRegister : IRepositoryServiceRegister
{
    public bool IsSuit4Provider(string provider)
    {
        return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase);
    }

    public void AddFixedRepositories(IServiceCollection sc)
    {
        // Register repositories that don't depend on environment
        sc.AddScoped<ISysInitRepository, SysInitRepository>();
    }

    public T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class
    {
        // EF Core doesn't need environment-specific services
        // All repositories use the same DbContext
        return sp.GetRequiredService<T>();
    }

    public void Register(IServiceCollection services)
    {
        // Register DbContext (will be configured by caller with connection string)
        services.AddScoped<AgileConfigDbContext>();

        // Register Unit of Work
        services.AddScoped<IUow, EFCoreUow>();

        // Register all repositories
        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<IAppInheritancedRepository, AppInheritancedRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IConfigPublishedRepository, ConfigPublishedRepository>();
        services.AddScoped<IFunctionRepository, FunctionRepository>();
        services.AddScoped<IPublishDetailRepository, PublishDetailRepository>();
        services.AddScoped<IPublishTimelineRepository, PublishTimelineRepository>();
        services.AddScoped<IRoleDefinitionRepository, RoleDefinitionRepository>();
        services.AddScoped<IRoleFunctionRepository, RoleFunctionRepository>();
        services.AddScoped<IServerNodeRepository, ServerNodeRepository>();
        services.AddScoped<IServiceInfoRepository, ServiceInfoRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISysInitRepository, SysInitRepository>();
        services.AddScoped<ISysLogRepository, SysLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAppAuthRepository, UserAppAuthRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    }
}
