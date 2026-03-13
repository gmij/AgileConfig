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
        // Extract the actual database type from the provider string
        // Supports formats:
        // 1. "efcore:mysql" - explicit EF Core with MySQL
        // 2. "efcore:sqlserver" - explicit EF Core with SQL Server
        // 3. "efcore:postgresql" or "efcore:npgsql" - explicit EF Core with PostgreSQL
        // 4. "efcore:sqlite" - explicit EF Core with SQLite
        // 5. "efcore" - defaults to SQLite

        string dbType = "sqlite"; // default
        if (provider.Contains(":"))
        {
            var parts = provider.Split(':', 2);
            if (parts.Length == 2)
            {
                dbType = parts[1].Trim().ToLower();
            }
        }
        else if (provider.Equals("efcore", StringComparison.OrdinalIgnoreCase))
        {
            dbType = "sqlite"; // default when just "efcore"
        }
        else
        {
            dbType = provider.ToLower();
        }

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
                throw new NotSupportedException($"Database type '{dbType}' from provider '{provider}' is not supported by EF Core. Supported types: sqlserver, mysql, postgresql (npgsql), sqlite. Use format 'efcore:dbtype' like 'efcore:mysql' or 'efcore:sqlserver'.");
        }
    }
}
