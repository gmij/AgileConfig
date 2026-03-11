using Microsoft.Extensions.DependencyInjection;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class EFCoreRepositoryServiceRegister : IRepositoryServiceRegister
{
    public void Register(IServiceCollection services)
    {
        // Register DbContext
        services.AddScoped<AgileConfigDbContext>();

        // Register Unit of Work
        services.AddScoped<IUow, EFCoreUow>();

        // Register repositories
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
