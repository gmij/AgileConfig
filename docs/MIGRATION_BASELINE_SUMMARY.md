# EF Core Migration Baseline Setup - Summary

## Overview

This document summarizes the work done to establish the v1.10 baseline for Entity Framework Core migrations in the AgileConfig project.

## Completed Tasks

### 1. Created v1.10 Baseline Migration

**Migration Name**: `V1_10_Baseline`
**Timestamp**: `20260313073333`
**Location**: `src/AgileConfig.Server.Data.EFCore/Migrations/`

The baseline includes:
- **15 database tables** for the complete AgileConfig schema
- **Default admin user** (username: admin, password: 123456)
- **2 default roles** (Administrator, Operator)
- **14 system functions** (permissions)
- **Role-function mappings** for admin role

### 2. Fixed Seed Data Issues

**Problem**: Original seed data used dynamic values (`DateTime.Now`, `Guid.NewGuid()`) which caused EF Core to detect model changes on every build.

**Solution**: Replaced dynamic values with static constants:
- `DateTime.Now` → `new DateTime(2026, 3, 11, 9, 36, 21, DateTimeKind.Utc)`
- `Guid.NewGuid()` → Fixed string IDs like "admin-role-001", "rf-001", etc.

This ensures the migration model is deterministic and stable.

### 3. Created Migration Generation Script

**File**: `scripts/generate-migration-from-tag.sh`

**Features**:
- Automates migration generation from git tags
- Checks out specified tag
- Generates migration with version-based naming (e.g., V1_11_0)
- Returns to original branch
- Provides clear status messages

**Usage**:
```bash
./scripts/generate-migration-from-tag.sh v1.11.0
```

### 4. Comprehensive Documentation

**File**: `docs/EF_CORE_MIGRATIONS.md`

**Contents**:
- Migration strategy overview
- Directory structure
- Database schema reference
- Supported database providers (SQLite, SQL Server, MySQL, PostgreSQL)
- Step-by-step guides for:
  - Creating new migrations
  - Applying migrations
  - Testing migrations
  - Rolling back migrations
- Production deployment checklist
- Troubleshooting guide
- Best practices

## Database Schema (v1.10)

### Tables Created

| # | Table Name | Purpose |
|---|------------|---------|
| 1 | agc_app | Application definitions |
| 2 | agc_appInheritanced | App inheritance relationships |
| 3 | agc_config | Configuration items |
| 4 | agc_config_published | Published config snapshots |
| 5 | agc_function | System permissions |
| 6 | agc_publish_detail | Publish details |
| 7 | agc_publish_timeline | Publish history |
| 8 | agc_role | Role definitions |
| 9 | agc_role_function | Role-permission mappings |
| 10 | agc_server_node | Server node registry |
| 11 | agc_service_info | Service information |
| 12 | agc_setting | System settings |
| 13 | agc_sys_log | System logs |
| 14 | agc_user | User accounts |
| 15 | agc_user_app_auth | User-app authorizations |
| 16 | agc_user_role | User-role mappings |

### Seed Data

**Admin User**:
- Username: `admin`
- Password: `123456` (hashed)
- Salt: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`

**Roles**:
- Administrator (ID: 001) - Full access
- Operator (ID: 002) - Limited access

**Functions** (14 permissions):
- App: Add, Edit, Delete
- Config: Add, Edit, Delete, Publish, Rollback
- User: Add, Edit, Delete
- Node: Add, Delete
- SysLog: View

## Migration Testing

Successfully tested the baseline migration:
- ✅ Build succeeds with no warnings about model changes
- ✅ Migration applies successfully to SQLite
- ✅ All 15 tables created correctly
- ✅ Seed data inserted (admin user, roles, functions)
- ✅ No determinism warnings from EF Core

## Next Steps for Future Versions

When a new version is ready for release:

1. **Tag the release**:
   ```bash
   git tag -a v1.11.0 -m "Release version 1.11.0"
   git push origin v1.11.0
   ```

2. **Generate migration**:
   ```bash
   ./scripts/generate-migration-from-tag.sh v1.11.0
   ```

3. **Review and test** the generated migration

4. **Commit migration files**:
   ```bash
   git add src/AgileConfig.Server.Data.EFCore/Migrations/
   git commit -m "Add EF Core migration for v1.11.0"
   git push
   ```

## Migration Naming Convention

- **Baseline**: `V1_10_Baseline`
- **Version updates**: `V{Major}_{Minor}_{Patch}` (e.g., V1_11_0, V1_12_3)
- **Feature updates**: `V{Version}_{Feature}` (e.g., V1_11_0_AddIndexes)

## File Structure

```
AgileConfig/
├── docs/
│   ├── EF_CORE_MIGRATIONS.md          # Main migration guide
│   └── MIGRATION_BASELINE_SUMMARY.md  # This file
├── scripts/
│   └── generate-migration-from-tag.sh # Migration generator
└── src/AgileConfig.Server.Data.EFCore/
    ├── Migrations/
    │   ├── 20260313073333_V1_10_Baseline.cs
    │   ├── 20260313073333_V1_10_Baseline.Designer.cs
    │   └── AgileConfigDbContextModelSnapshot.cs
    ├── AgileConfigDbContext.cs
    ├── AgileConfigDbContextFactory.cs
    └── AgileConfigDbSeedData.cs
```

## Technical Details

- **EF Core Version**: 10.0.0
- **Target Framework**: .NET 10.0
- **Migration Provider**: SQLite (default), SQL Server, MySQL, PostgreSQL
- **Seed Data**: Static values to ensure deterministic model

## Verification Commands

```bash
# List migrations
dotnet ef migrations list

# Apply migration
dotnet ef database update

# Test on clean database
dotnet ef database drop --force
dotnet ef database update

# Verify tables
sqlite3 agile_config.db ".tables"

# Verify admin user
sqlite3 agile_config.db "SELECT UserName FROM agc_user;"
```

## Important Notes

1. **Never modify released migrations** - Always create new ones
2. **Test on all database providers** before releasing
3. **Always implement Down() methods** for rollback capability
4. **Keep migrations small** - One logical change per migration
5. **Document breaking changes** in migration comments

## Version History

| Date | Version | Migration | Notes |
|------|---------|-----------|-------|
| 2026-03-13 | v1.10 | V1_10_Baseline | Initial EF Core baseline |
| TBD | v1.11 | V1_11_0 | Next version (future) |

---

**Created**: 2026-03-13
**Project**: AgileConfig
**Repository**: https://github.com/dotnetcore/AgileConfig
