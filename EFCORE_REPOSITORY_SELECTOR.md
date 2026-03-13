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
   - Recognizes provider formats: `"efcore"` or `"efcore:dbtype"` (case-insensitive)
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
    "provider": "efcore:mysql",  // Use EF Core with MySQL
    "conn": "Server=localhost;Database=agileconfig;User=root;Password=pass;",
    "env": {
      "TEST": {
        "provider": "efcore:sqlserver",  // SQL Server for TEST env
        "conn": "Server=localhost;Database=AgileConfig_Test;..."
      },
      "PROD": {
        "provider": "efcore:postgresql",  // PostgreSQL for PROD env
        "conn": "Host=localhost;Database=AgileConfig_Prod;..."
      }
    }
  }
}
```

**Note**: You can also use just `"efcore"` which will default to SQLite.

### Supported Provider Formats

When using EF Core, you can specify the provider in two formats:

**Format 1: Colon-separated format (Recommended)**
- `"efcore:mysql"` - EF Core with MySQL
- `"efcore:sqlserver"` - EF Core with SQL Server
- `"efcore:postgresql"` or `"efcore:npgsql"` - EF Core with PostgreSQL
- `"efcore:sqlite"` - EF Core with SQLite

**Format 2: Plain format**
- `"efcore"` - EF Core with default (SQLite)

**Why use the colon-separated format?**
The colon-separated format explicitly specifies which database type to use with EF Core, avoiding confusion with FreeSql provider names. This ensures that the correct ORM and database combination is selected.

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
    // Support both "efcore" and "efcore:dbtype" formats
    // Examples: "efcore", "efcore:mysql", "efcore:sqlserver", "efcore:postgresql", "efcore:sqlite"
    return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase) ||
           provider.StartsWith("efcore:", StringComparison.OrdinalIgnoreCase);
}
```

The `EFCoreRepositoryServiceRegister` matches when:
- Provider is exactly `"efcore"` (case-insensitive) - defaults to SQLite
- Provider starts with `"efcore:"` (case-insensitive) - uses specified database type

### Database Type Extraction

The database type is extracted in `EFCoreServiceExtension.ConfigureDbContext()`:

```csharp
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
        throw new NotSupportedException($"Database type '{dbType}' from provider '{provider}' is not supported...");
}
```

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
# Test with SQLite (default)
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore --db:conn="Data Source=test.db"

# Test with SQLite (explicit)
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore:sqlite --db:conn="Data Source=test.db"

# Test with SQL Server
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore:sqlserver --db:conn="Server=localhost;..."

# Test with MySQL
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore:mysql --db:conn="Server=localhost;Database=agileconfig;..."

# Test with PostgreSQL
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=efcore:postgresql --db:conn="Host=localhost;..."
```

**Important**: Do NOT use plain database names like `"mysql"` or `"sqlserver"` when you want to use EF Core, as these will be matched by FreeSql first. Always use the `"efcore:dbtype"` format for EF Core.

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
