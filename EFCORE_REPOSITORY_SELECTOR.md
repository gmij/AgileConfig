# EF Core Repository Selector Configuration

## Overview
The repository selector pattern allows AgileConfig to support multiple data persistence providers (FreeSql, MongoDB, EF Core). This document describes how EF Core is integrated into the selector system.

## Architecture

### Key Components

1. **IRepositoryServiceRegister** (`AgileConfig.Server.Data.Abstraction`)
   - Defines the contract for repository provider registration
   - Methods: `IsSuit4Provider()`, `AddFixedRepositories()`, `GetServiceByEnv()`

2. **EFCoreRepositoryServiceRegister** (`AgileConfig.Server.Data.Repository.EFCore`)
   - Implements registration logic for EF Core repositories
   - Recognizes provider name: `"efcore"` (case-insensitive)
   - Registers all repositories and Unit of Work

3. **RepositoryExtension** (`AgileConfig.Server.Data.Repository.Selector`)
   - Central registration point
   - Maintains list of available repository service registers
   - Routes to appropriate provider based on configuration

4. **EFCoreServiceExtension** (`AgileConfig.Server.Data.EFCore`)
   - Provides `AddEFCoreDbContext()` extension method
   - Configures DbContext with appropriate database provider
   - Supports: SQL Server, MySQL, PostgreSQL, SQLite

## Configuration

### appsettings.json

```json
{
  "db": {
    "provider": "efcore",  // Use EF Core
    "conn": "Data Source=agile_config.db",  // Connection string
    "env": {
      "TEST": {
        "provider": "sqlserver",  // SQL Server for TEST env
        "conn": "Server=localhost;Database=AgileConfig_Test;..."
      },
      "PROD": {
        "provider": "npgsql",  // PostgreSQL for PROD env
        "conn": "Host=localhost;Database=AgileConfig_Prod;..."
      }
    }
  }
}
```

### Supported Provider Names

When using EF Core, you can specify these provider names:

- `"efcore"` - Generic EF Core (defaults to SQLite if connection string doesn't specify)
- `"sqlserver"` - Microsoft SQL Server
- `"mysql"` - MySQL (using MySql.EntityFrameworkCore 10.0.1)
- `"npgsql"` or `"postgresql"` - PostgreSQL
- `"sqlite"` - SQLite

## How It Works

### Startup Flow

1. **Startup.cs** calls `services.AddRepositories()`
2. **RepositoryExtension.AddRepositories()**:
   - Reads database configuration from `IDbConfigInfoFactory`
   - Calls `GetRepositoryServiceRegister(provider)` to find matching register
   - If EF Core is selected:
     - Calls `sc.AddEFCoreDbContext()` to register DbContext
     - Calls `register.Register(sc)` to register repositories
   - Calls `register.AddFixedRepositories(sc)` for environment-independent repositories
   - Registers factory functions for environment-specific repositories

3. **EFCoreServiceExtension.AddEFCoreDbContext()**:
   - Gets database provider and connection string from configuration
   - Configures `DbContextOptionsBuilder` based on provider:
     ```csharp
     case "sqlserver":
         options.UseSqlServer(connectionString);
     case "mysql":
         options.UseMySQL(connectionString);
     case "npgsql":
         options.UseNpgsql(connectionString);
     case "sqlite":
         options.UseSqlite(connectionString);
     ```

4. **EFCoreRepositoryServiceRegister.Register()**:
   - Registers `AgileConfigDbContext` (scoped)
   - Registers `IUow` → `EFCoreUow` (scoped)
   - Registers all repository interfaces with implementations

### Repository Resolution

**Fixed Repositories** (environment-independent):
- Resolved directly from DI container
- Example: `ISysInitRepository`

**Environment-Specific Repositories**:
- Resolved using factory functions: `Func<string, IRepository>`
- Factory receives environment name and returns appropriate repository instance
- Examples: `IConfigRepository`, `IConfigPublishedRepository`, `IUow`

## Provider Selection Logic

```csharp
public bool IsSuit4Provider(string provider)
{
    return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase);
}
```

The `EFCoreRepositoryServiceRegister` matches when the provider is `"efcore"` (case-insensitive).

For database-specific configuration, use the database name directly:
- `"sqlserver"` → Will match `EFCoreRepositoryServiceRegister` only if provider is exactly `"efcore"`
- Database type is determined in `EFCoreServiceExtension.ConfigureDbContext()`

## Adding a New Repository Provider

To add a new repository provider (e.g., Dapper, LiteDB):

1. Create `IRepositoryServiceRegister` implementation
2. Add to `_repositoryServiceRegisters` list in `RepositoryExtension.cs`
3. Implement `IsSuit4Provider()` to match provider name
4. Implement `AddFixedRepositories()` for environment-independent repositories
5. Implement `GetServiceByEnv<T>()` for environment-specific repositories

## Testing

Test the repository selector by setting different providers in appsettings:

```bash
# Test with SQLite
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore --db:conn="Data Source=test.db"

# Test with SQL Server
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=sqlserver --db:conn="Server=localhost;..."

# Test with MySQL
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=mysql --db:conn="Server=localhost;Database=agileconfig;..."

# Test with PostgreSQL
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=npgsql --db:conn="Host=localhost;..."
```

## Troubleshooting

**Error: "[provider] is not a supported provider"**
- Ensure the provider name matches one registered in `_repositoryServiceRegisters`
- Check `IsSuit4Provider()` implementation

**Error: "Database provider '[provider]' is not supported by EF Core"**
- Ensure the database type is supported (sqlserver, mysql, npgsql, sqlite)
- Verify the database provider package is installed

**DbContext not configured correctly**
- Check connection string format for your database
- Verify NuGet packages are installed (Microsoft.EntityFrameworkCore.SqlServer, MySql.EntityFrameworkCore, etc.)
- Ensure `AddEFCoreDbContext()` is called before `Register()`

## References

- [IRepositoryServiceRegister.cs](src/AgileConfig.Server.Data.Abstraction/IRepositoryServiceRegister.cs)
- [EFCoreRepositoryServiceRegister.cs](src/AgileConfig.Server.Data.Repository.EFCore/EFCoreRepositoryServiceRegister.cs)
- [RepositoryExtension.cs](src/AgileConfig.Server.Data.Repository.Selector/RepositoryExtension.cs)
- [EFCoreServiceExtension.cs](src/AgileConfig.Server.Data.EFCore/EFCoreServiceExtension.cs)
