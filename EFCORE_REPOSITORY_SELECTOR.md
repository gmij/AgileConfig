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
   - Recognizes ORM provider: `"efcore"` (case-insensitive)
   - Registers all repositories and Unit of Work

3. **RepositoryExtension** (`AgileConfig.Server.Data.Repository.Selector`)
   - Central registration point
   - Maintains list of available repository service registers
   - Routes to appropriate ORM provider based on configuration

4. **EFCoreServiceExtension** (`AgileConfig.Server.Data.EFCore`)
   - Provides `AddEFCoreDbContext()` extension method
   - Configures DbContext with appropriate database provider
   - Supports: SQL Server, MySQL, PostgreSQL, SQLite

## Configuration

### appsettings.json

```json
{
  "db": {
    "ormProvider": "efcore",  // ORM provider: "freesql" (default), "efcore", or "mongodb"
    "provider": "mysql",      // Database type: sqlite, mysql, sqlserver, npgsql, postgresql, oracle
    "conn": "Server=localhost;Database=agileconfig;User=root;Password=pass;",
    "env": {
      "TEST": {
        "ormProvider": "efcore",  // Use EF Core for TEST env
        "provider": "sqlserver",  // SQL Server database
        "conn": "Server=localhost;Database=AgileConfig_Test;..."
      },
      "PROD": {
        "ormProvider": "freesql", // Use FreeSql for PROD env
        "provider": "postgresql", // PostgreSQL database
        "conn": "Host=localhost;Database=AgileConfig_Prod;..."
      }
    }
  }
}
```

### Configuration Fields

**ormProvider**: Specifies which ORM framework to use
- `"freesql"` - Use FreeSql ORM (default if not specified)
- `"efcore"` - Use Entity Framework Core
- `"mongodb"` - Use MongoDB driver

**provider**: Specifies the database type
- For FreeSql/EF Core: `sqlite`, `mysql`, `sqlserver`, `npgsql`, `postgresql`, `oracle`
- For MongoDB: `mongodb`

## How It Works

### Startup Flow

1. **Startup.cs** calls `services.AddRepositories()`
2. **RepositoryExtension.AddRepositories()**:
   - Reads database configuration from `IDbConfigInfoFactory`
   - Calls `GetRepositoryServiceRegister(ormProvider)` to find matching register based on `ormProvider` field
   - If EF Core is selected:
     - Calls `sc.AddEFCoreDbContext()` to register DbContext
     - Calls `register.Register(sc)` to register repositories
   - Calls `register.AddFixedRepositories(sc)` for environment-independent repositories
   - Registers factory functions for environment-specific repositories

3. **EFCoreServiceExtension.AddEFCoreDbContext()**:
   - Gets database provider (database type) and connection string from configuration
   - Configures `DbContextOptionsBuilder` based on provider:
     ```csharp
     case "sqlserver":
         options.UseSqlServer(connectionString);
     case "mysql":
         options.UseMySQL(connectionString);
     case "npgsql":
     case "postgresql":
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
    // Check if ORM provider is "efcore"
    return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase);
}
```

The `EFCoreRepositoryServiceRegister` matches when `ormProvider` is exactly `"efcore"` (case-insensitive).

### ORM Provider to Repository Register Mapping

- `ormProvider: "freesql"` → `FreesqlRepositoryServiceRegister`
- `ormProvider: "efcore"` → `EFCoreRepositoryServiceRegister`
- `ormProvider: "mongodb"` → `MongodbRepositoryServiceRegister`

The `provider` field specifies the database type (mysql, sqlite, sqlserver, etc.) and is used by the selected ORM to configure the appropriate database driver.

## Adding a New Repository Provider

To add a new repository provider (e.g., Dapper, LiteDB):

1. Create `IRepositoryServiceRegister` implementation
2. Add to `_repositoryServiceRegisters` list in `RepositoryExtension.cs`
3. Implement `IsSuit4Provider()` to match provider name
4. Implement `AddFixedRepositories()` for environment-independent repositories
5. Implement `GetServiceByEnv<T>()` for environment-specific repositories

## Testing

Test the repository selector by setting different configurations in appsettings:

```bash
# Test with SQLite and FreeSql (default)
dotnet run --project src/AgileConfig.Server.Apisite -- --db:provider=sqlite --db:conn="Data Source=test.db"

# Test with SQLite and EF Core
dotnet run --project src/AgileConfig.Server.Apisite -- --db:ormProvider=efcore --db:provider=sqlite --db:conn="Data Source=test.db"

# Test with SQL Server and EF Core
dotnet run --project src/AgileConfig.Server.Apisite -- --db:ormProvider=efcore --db:provider=sqlserver --db:conn="Server=localhost;..."

# Test with MySQL and EF Core
dotnet run --project src/AgileConfig.Server.Apisite -- --db:ormProvider=efcore --db:provider=mysql --db:conn="Server=localhost;Database=agileconfig;..."

# Test with PostgreSQL and FreeSql
dotnet run --project src/AgileConfig.Server.Apisite -- --db:ormProvider=freesql --db:provider=npgsql --db:conn="Host=localhost;..."

# Test with MongoDB
dotnet run --project src/AgileConfig.Server.Apisite -- --db:ormProvider=mongodb --db:provider=mongodb --db:conn="mongodb://localhost:27017/agileconfig"
```

**Configuration via Environment Variables:**
```bash
export DB__ORMPROVIDER=efcore
export DB__PROVIDER=mysql
export DB__CONN="Server=localhost;Database=agileconfig;User=root;Password=pass;"
dotnet run --project src/AgileConfig.Server.Apisite
```

## Troubleshooting

**Error: "[ormProvider] is not a supported ORM provider"**
- Ensure the `ormProvider` field is set to "freesql", "efcore", or "mongodb"
- Check `IsSuit4Provider()` implementation in the repository service register

**Error: "Database type '[dbType]' is not supported by EF Core"**
- Ensure the `provider` field is set to a supported database type (sqlserver, mysql, npgsql, postgresql, sqlite)
- Verify the database provider package is installed (Microsoft.EntityFrameworkCore.SqlServer, MySql.EntityFrameworkCore, etc.)

**DbContext not configured correctly**
- Check connection string format for your database
- Verify NuGet packages are installed
- Ensure `AddEFCoreDbContext()` is called before `Register()`

**ORM Provider not being used**
- Verify `ormProvider` field is set correctly in appsettings.json
- Check that `ormProvider` defaults to "freesql" if not specified
- Ensure the configuration is loaded properly (check startup logs: "default db provider: ..., ORM provider: ...")

## References

- [IRepositoryServiceRegister.cs](src/AgileConfig.Server.Data.Abstraction/IRepositoryServiceRegister.cs)
- [EFCoreRepositoryServiceRegister.cs](src/AgileConfig.Server.Data.Repository.EFCore/EFCoreRepositoryServiceRegister.cs)
- [RepositoryExtension.cs](src/AgileConfig.Server.Data.Repository.Selector/RepositoryExtension.cs)
- [EFCoreServiceExtension.cs](src/AgileConfig.Server.Data.EFCore/EFCoreServiceExtension.cs)
