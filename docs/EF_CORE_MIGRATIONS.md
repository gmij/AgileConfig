# EF Core Migration Management Guide

## Overview

This document describes how to manage Entity Framework Core migrations for AgileConfig, starting from the v1.10 baseline.

## Migration Strategy

AgileConfig uses a version-based migration strategy where:
- **v1.10** serves as the baseline migration
- Future migrations are generated based on git tags
- Each version tag corresponds to a specific database schema state

## Directory Structure

```
src/AgileConfig.Server.Data.EFCore/
├── Migrations/
│   ├── 20260311093621_V1_10_Baseline.cs          # v1.10 baseline
│   ├── 20260311093621_V1_10_Baseline.Designer.cs
│   └── AgileConfigDbContextModelSnapshot.cs       # Current model
├── AgileConfigDbContext.cs                        # DbContext definition
└── AgileConfigDbContextFactory.cs                 # Design-time factory
```

## Database Schema (v1.10 Baseline)

The baseline includes 15 tables:

| Table Name | Description |
|------------|-------------|
| agc_app | Application definitions |
| agc_appInheritanced | Application inheritance relationships |
| agc_config | Configuration items |
| agc_config_published | Published configuration snapshots |
| agc_function | System functions/permissions |
| agc_publish_detail | Configuration publish details |
| agc_publish_timeline | Publish history timeline |
| agc_role | Role definitions |
| agc_role_function | Role-function mappings |
| agc_server_node | Server node registrations |
| agc_service_info | Service information |
| agc_setting | System settings |
| agc_sys_log | System logs |
| agc_user | User accounts |
| agc_user_app_auth | User-application authorizations |
| agc_user_role | User-role mappings |

## Supported Database Providers

AgileConfig supports multiple database providers:

- **SQLite** - Default for development and testing
- **SQL Server** - Enterprise deployments
- **MySQL** - Popular open-source option
- **PostgreSQL** - Advanced open-source option

## Creating New Migrations

### Automatic Generation from Git Tags

Use the provided script to generate migrations from git tags:

```bash
# Generate migration for a specific tag
./scripts/generate-migration-from-tag.sh v1.11.0

# Generate migration for v1.12.0
./scripts/generate-migration-from-tag.sh v1.12.0
```

The script will:
1. Checkout the specified tag
2. Generate a migration with the version name (e.g., V1_11_0)
3. Return to your current branch
4. Display the generated files

### Manual Migration Generation

If you need to create a migration manually:

```bash
cd src/AgileConfig.Server.Data.EFCore

# Add a new migration
dotnet ef migrations add <MigrationName> --output-dir Migrations

# Example with version name
dotnet ef migrations add V1_11_0 --output-dir Migrations
```

### Migration Naming Convention

Follow this naming pattern for consistency:
- **Version migrations**: `V{Major}_{Minor}_{Patch}` (e.g., V1_11_0, V1_12_3)
- **Feature migrations**: `V{Version}_{Feature}` (e.g., V1_11_0_AddIndexes)
- **Baseline**: `V1_10_Baseline` (special case for initial schema)

## Applying Migrations

### Update Database to Latest Version

```bash
cd src/AgileConfig.Server.Data.EFCore

# Update to latest migration
dotnet ef database update

# Update to specific migration
dotnet ef database update V1_11_0
```

### Rollback to Previous Version

```bash
# Rollback to specific migration
dotnet ef database update V1_10_Baseline

# Rollback all migrations (drop all tables)
dotnet ef database update 0
```

## Testing Migrations

### Test Migration Up/Down

```bash
cd src/AgileConfig.Server.Data.EFCore

# Apply migration
dotnet ef database update V1_11_0

# Test rollback
dotnet ef database update V1_10_Baseline

# Re-apply to latest
dotnet ef database update
```

### Test with Different Providers

Create test connection strings in `appsettings.Development.json`:

```json
{
  "db": {
    "provider": "sqlite",
    "conn": "Data Source=test_agile_config.db"
  }
}
```

Test each provider:
```bash
# SQLite
dotnet ef database update --connection "Data Source=test_sqlite.db"

# SQL Server
dotnet ef database update --connection "Server=localhost;Database=AgileConfigTest;..."

# MySQL
dotnet ef database update --connection "Server=localhost;Database=AgileConfigTest;..."

# PostgreSQL
dotnet ef database update --connection "Host=localhost;Database=AgileConfigTest;..."
```

## Migration Workflow

### For a New Version Release

1. **Create and tag the release**:
   ```bash
   git tag -a v1.11.0 -m "Release v1.11.0"
   git push origin v1.11.0
   ```

2. **Generate migration**:
   ```bash
   ./scripts/generate-migration-from-tag.sh v1.11.0
   ```

3. **Review generated files**:
   - Check `Migrations/*_V1_11_0.cs` for schema changes
   - Verify `Up()` and `Down()` methods are correct
   - Review seed data if any

4. **Test migration**:
   ```bash
   cd src/AgileConfig.Server.Data.EFCore

   # Test on clean database
   dotnet ef database drop --force
   dotnet ef database update

   # Verify all tables created
   # Run application tests
   ```

5. **Commit migration files**:
   ```bash
   git add src/AgileConfig.Server.Data.EFCore/Migrations/
   git commit -m "Add EF Core migration for v1.11.0"
   git push origin main
   ```

### For Schema Changes During Development

1. **Modify entities** in `AgileConfig.Server.Data.Entity`

2. **Generate migration**:
   ```bash
   cd src/AgileConfig.Server.Data.EFCore
   dotnet ef migrations add DescriptiveFeatureName
   ```

3. **Review and test** the migration

4. **When ready for release**, rename to version-based name:
   - Rename files to match version
   - Update class names in code
   - Update `[Migration]` attribute

## Troubleshooting

### Migration Already Exists

If you see "A migration with the name 'X' already exists":
```bash
# List all migrations
dotnet ef migrations list

# Remove the last migration
dotnet ef migrations remove
```

### Database Out of Sync

If the database schema doesn't match migrations:
```bash
# Reset database to clean state
dotnet ef database drop --force
dotnet ef database update
```

### Design-Time Factory Issues

The `AgileConfigDbContextFactory` provides a default SQLite connection for design-time operations. If you need to use a different provider during migration generation:

1. Temporarily modify `AgileConfigDbContextFactory.cs`
2. Generate the migration
3. Revert the factory changes

### Provider-Specific SQL

Some migrations might need provider-specific SQL. Use conditional logic:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Check provider type
    var provider = migrationBuilder.ActiveProvider;

    if (provider.Contains("Sqlite"))
    {
        // SQLite-specific SQL
    }
    else if (provider.Contains("SqlServer"))
    {
        // SQL Server-specific SQL
    }
}
```

## Best Practices

1. **Always test migrations** on all supported database providers before release
2. **Keep migrations small** - one logical change per migration when possible
3. **Write reversible migrations** - always implement proper `Down()` methods
4. **Document breaking changes** in migration comments
5. **Never modify existing migrations** that have been released
6. **Use version tags** to mark stable database schema states
7. **Test rollback scenarios** to ensure `Down()` methods work correctly
8. **Review generated SQL** - check what EF Core generates for each provider

## Production Deployment

### Pre-Deployment Checklist

- [ ] Migration tested on all supported database providers
- [ ] Migration tested with rollback (`Down()` method)
- [ ] Database backup created
- [ ] Rollback plan documented
- [ ] Application downtime communicated (if required)

### Deployment Process

1. **Create database backup**:
   ```bash
   # Example for PostgreSQL
   pg_dump agileconfig_prod > backup_before_v1_11_0.sql
   ```

2. **Apply migration**:
   ```bash
   # In production environment
   cd /path/to/AgileConfig
   dotnet ef database update --project src/AgileConfig.Server.Data.EFCore
   ```

3. **Verify migration**:
   ```bash
   dotnet ef migrations list
   # Check that latest migration is applied
   ```

4. **Monitor application** startup and logs

5. **If issues occur**, rollback:
   ```bash
   # Rollback to previous version
   dotnet ef database update V1_10_Baseline

   # Or restore from backup
   psql agileconfig_prod < backup_before_v1_11_0.sql
   ```

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [AgileConfig Architecture Guide](./MIGRATION_GUIDE.md)
- Project repository: [AgileConfig GitHub](https://github.com/dotnetcore/AgileConfig)

## Version History

| Version | Migration Name | Date | Description |
|---------|---------------|------|-------------|
| v1.10 | V1_10_Baseline | 2026-03-11 | Initial EF Core baseline schema |
| v1.11 | (future) | TBD | Next version updates |

---

**Last Updated**: 2026-03-13
**Maintained By**: AgileConfig Development Team
