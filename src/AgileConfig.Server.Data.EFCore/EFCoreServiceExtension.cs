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
        // The provider parameter now contains the database type (e.g., "mysql", "sqlserver", "postgresql", "sqlite")
        // The ORMProvider field in configuration determines which ORM to use (freesql, efcore, mongodb)

        string dbType = provider.ToLower();

        switch (dbType)
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
            default:
                throw new NotSupportedException($"Database type '{dbType}' is not supported by EF Core. Supported types: sqlserver, mysql, postgresql (npgsql), sqlite.");
        }
    }
}
