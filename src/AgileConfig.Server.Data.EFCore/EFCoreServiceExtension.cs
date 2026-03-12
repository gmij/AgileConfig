using AgileConfig.Server.Data.Abstraction.DbProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.EFCore;

public static class EFCoreServiceExtension
{
    public static IServiceCollection AddEFCoreDbContext(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var dbConfigInfoFactory = serviceProvider.GetRequiredService<IDbConfigInfoFactory>();
        var defaultDbConfig = dbConfigInfoFactory.GetConfigInfo();

        services.AddDbContext<AgileConfigDbContext>(options =>
        {
            ConfigureDbContext(options, defaultDbConfig.Provider, defaultDbConfig.ConnectionString);
        });

        return services;
    }

    private static void ConfigureDbContext(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        switch (provider.ToLower())
        {
            case "sqlserver":
                options.UseSqlServer(connectionString);
                break;
            case "mysql":
                options.UseMySQL(connectionString);
                break;
            case "npgsql":
            case "postgresql":
                options.UseNpgsql(connectionString);
                break;
            case "sqlite":
                options.UseSqlite(connectionString);
                break;
            case "efcore": // Generic EF Core provider - auto-detect from connection string
                // Fallback to SQLite if provider is just "efcore"
                options.UseSqlite(connectionString);
                break;
            default:
                throw new NotSupportedException($"Database provider '{provider}' is not supported by EF Core in this project. Supported providers: sqlserver, mysql, postgresql (npgsql), sqlite");
        }
    }
}
