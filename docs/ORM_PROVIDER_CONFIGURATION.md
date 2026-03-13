# ORM Provider Configuration Guide

## Overview

AgileConfig now supports explicit ORM provider selection through a dedicated `ormProvider` configuration field. This makes it clear which ORM framework (FreeSql, EF Core, or MongoDB) is being used, while the `provider` field specifies the database type.

## Configuration Structure

### Previous Approach (Before this change)

Previously, the ORM was inferred from the `provider` field:
- Standard database names (sqlite, mysql, sqlserver, etc.) → FreeSql
- "efcore:dbtype" format → EF Core
- "mongodb" → MongoDB

This approach had several issues:
1. Not intuitive - users had to know the inference rules
2. Confusing format - "efcore:mysql" mixed ORM and database type
3. Easy to misconfigure - forgetting the "efcore:" prefix would silently use FreeSql

### New Approach (Current)

Now there are two separate fields:
- **`ormProvider`**: Specifies which ORM framework to use
- **`provider`**: Specifies the database type

```json
{
  "db": {
    "ormProvider": "efcore",  // Which ORM: freesql (default), efcore, mongodb
    "provider": "mysql",      // Which database: sqlite, mysql, sqlserver, etc.
    "conn": "Server=localhost;Database=agileconfig;..."
  }
}
```

## Configuration Examples

### Example 1: FreeSql with SQLite (Default)

```json
{
  "db": {
    "provider": "sqlite",
    "conn": "Data Source=agile_config.db"
  }
}
```

Note: `ormProvider` defaults to "freesql" if not specified.

### Example 2: EF Core with MySQL

```json
{
  "db": {
    "ormProvider": "efcore",
    "provider": "mysql",
    "conn": "Server=localhost;Database=agileconfig;User=root;Password=pass;"
  }
}
```

### Example 3: EF Core with SQL Server

```json
{
  "db": {
    "ormProvider": "efcore",
    "provider": "sqlserver",
    "conn": "Server=localhost;Database=AgileConfig;Trusted_Connection=true;"
  }
}
```

### Example 4: MongoDB

```json
{
  "db": {
    "ormProvider": "mongodb",
    "provider": "mongodb",
    "conn": "mongodb://localhost:27017/agileconfig"
  }
}
```

### Example 5: Environment-Specific Configuration

```json
{
  "db": {
    "ormProvider": "freesql",
    "provider": "sqlite",
    "conn": "Data Source=agile_config.db",
    "env": {
      "TEST": {
        "ormProvider": "efcore",
        "provider": "sqlserver",
        "conn": "Server=test-server;Database=AgileConfig_Test;..."
      },
      "PROD": {
        "ormProvider": "freesql",
        "provider": "postgresql",
        "conn": "Host=prod-server;Database=AgileConfig_Prod;..."
      }
    }
  }
}
```

## Supported Values

### ormProvider

- **`freesql`** (default) - Use FreeSql ORM
- **`efcore`** - Use Entity Framework Core
- **`mongodb`** - Use MongoDB driver

### provider (Database Types)

For FreeSql and EF Core:
- `sqlite` - SQLite database
- `mysql` - MySQL database
- `sqlserver` - Microsoft SQL Server
- `npgsql` or `postgresql` - PostgreSQL database
- `oracle` - Oracle database (FreeSql only)

For MongoDB:
- `mongodb` - MongoDB database

## Environment Variables

Configuration can also be set via environment variables:

```bash
# Set ORM provider
export DB__ORMPROVIDER=efcore

# Set database type
export DB__PROVIDER=mysql

# Set connection string
export DB__CONN="Server=localhost;Database=agileconfig;User=root;Password=pass;"

# Environment-specific
export DB__ENV__PROD__ORMPROVIDER=freesql
export DB__ENV__PROD__PROVIDER=postgresql
export DB__ENV__PROD__CONN="Host=prod-server;..."
```

## Migration Guide

### If you were using FreeSql (standard database names)

**Before:**
```json
{
  "db": {
    "provider": "mysql",
    "conn": "..."
  }
}
```

**After (explicit, but optional):**
```json
{
  "db": {
    "ormProvider": "freesql",  // Optional, this is the default
    "provider": "mysql",
    "conn": "..."
  }
}
```

**No changes required** - the default behavior remains FreeSql if `ormProvider` is not specified.

### If you were using EF Core (colon-separated format)

**Before:**
```json
{
  "db": {
    "provider": "efcore:mysql",
    "conn": "..."
  }
}
```

**After:**
```json
{
  "db": {
    "ormProvider": "efcore",
    "provider": "mysql",
    "conn": "..."
  }
}
```

**Action required**: Update your configuration to use the new format.

### If you were using MongoDB

**Before:**
```json
{
  "db": {
    "provider": "mongodb",
    "conn": "mongodb://..."
  }
}
```

**After:**
```json
{
  "db": {
    "ormProvider": "mongodb",
    "provider": "mongodb",
    "conn": "mongodb://..."
  }
}
```

**Action required**: Add the `ormProvider` field.

## Benefits of the New Approach

1. **Explicit and Clear**: The ORM choice is now explicit in the configuration
2. **Easier to Understand**: Separates concerns - ORM selection vs database type
3. **Better Defaults**: FreeSql remains the default, but now it's clear and documented
4. **Less Error-Prone**: Can't accidentally use wrong ORM by forgetting format rules
5. **Consistent Format**: No more mixing ORM and database type in one field

## Technical Implementation

### Architecture Changes

1. **IDbConfigInfo Interface**: Added `ORMProvider` property
2. **DbConfigInfo Class**: Added `ORMProvider` property with "freesql" default
3. **DbConfigInfoFactory**: Reads `db:ormProvider` from configuration
4. **RepositoryExtension**: Uses `ORMProvider` for ORM selection instead of pattern matching
5. **IRepositoryServiceRegister Implementations**: Simplified to check exact ORM name match

### Code Example

```csharp
// Reading configuration
public class DbConfigInfoFactory : IDbConfigInfoFactory
{
    public DbConfigInfoFactory(IConfiguration configuration)
    {
        var providerPath = "db:provider";
        var connPath = "db:conn";
        var ormProviderPath = "db:ormProvider";

        var providerValue = configuration[providerPath];
        var connValue = configuration[connPath];
        var ormProviderValue = configuration[ormProviderPath];

        var configInfo = new DbConfigInfo("", providerValue, connValue, ormProviderValue);
        // ormProviderValue defaults to "freesql" if null/empty
    }
}

// ORM selection
public static IServiceCollection AddRepositories(this IServiceCollection sc)
{
    var defaultProvider = dbConfigInfoFactory.GetConfigInfo();
    var register = GetRepositoryServiceRegister(defaultProvider.ORMProvider);
    // Uses ORMProvider field for selection
}

// Provider registration
public class EFCoreRepositoryServiceRegister : IRepositoryServiceRegister
{
    public bool IsSuit4Provider(string provider)
    {
        return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase);
    }
}
```

## Troubleshooting

### Error: "ArgumentNullException: db:provider"

**Cause**: The `provider` field is missing from configuration.

**Solution**: Add the `provider` field to your `db` configuration:
```json
{
  "db": {
    "provider": "sqlite",
    "conn": "..."
  }
}
```

### Error: "[ormProvider] is not a supported ORM provider"

**Cause**: Invalid value for `ormProvider` field.

**Solution**: Use one of the supported values: `freesql`, `efcore`, or `mongodb`.

### EF Core not being used even though configured

**Cause**: `ormProvider` field not set or set incorrectly.

**Solution**: Explicitly set `ormProvider` to `efcore`:
```json
{
  "db": {
    "ormProvider": "efcore",
    "provider": "mysql",
    "conn": "..."
  }
}
```

### Check which ORM is being used

Look for the startup log message:
```
default db provider: mysql, ORM provider: efcore
```

This shows both the database type (`mysql`) and ORM provider (`efcore`).

## References

- [EFCORE_REPOSITORY_SELECTOR.md](../EFCORE_REPOSITORY_SELECTOR.md) - Detailed EF Core configuration guide
- [IDbConfigInfo.cs](../src/AgileConfig.Server.Data.Abstraction/DbConfig/IDbConfigInfo.cs) - Configuration interface
- [RepositoryExtension.cs](../src/AgileConfig.Server.Data.Repository.Selector/RepositoryExtension.cs) - ORM selection logic
