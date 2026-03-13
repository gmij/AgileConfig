using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using FreeSql.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Tools;

/// <summary>
/// Compares database schemas between FreeSql and EF Core to ensure data compatibility during migration.
/// This tool analyzes entity mappings, column names, types, and constraints to identify potential issues.
/// </summary>
public class DataStructureComparisonTool
{
    private readonly List<ComparisonResult> _results = new();

    public class ComparisonResult
    {
        public string EntityName { get; set; }
        public string PropertyName { get; set; }
        public ComparisonStatus Status { get; set; }
        public string FreeSqlInfo { get; set; }
        public string EFCoreInfo { get; set; }
        public string Description { get; set; }
    }

    public enum ComparisonStatus
    {
        Match,
        Warning,
        Error
    }

    /// <summary>
    /// Compare all entity mappings between FreeSql and EF Core.
    /// </summary>
    public List<ComparisonResult> CompareSchemas()
    {
        _results.Clear();

        // Get EF Core context
        var optionsBuilder = new DbContextOptionsBuilder<AgileConfigDbContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:"); // Use in-memory for comparison
        using var context = new AgileConfigDbContext(optionsBuilder.Options);

        // Get all entity types from EF Core model
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // Get all entity classes from assembly
        var assembly = typeof(Config).Assembly;
        var entityClasses = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEntity<string>).IsAssignableFrom(t))
            .ToList();

        Console.WriteLine($"Found {entityClasses.Count} entity classes");
        Console.WriteLine($"Found {entityTypes.Count} EF Core entity types");

        foreach (var entityClass in entityClasses)
        {
            CompareEntity(entityClass, entityTypes, context.Model);
        }

        return _results;
    }

    private void CompareEntity(Type entityClass, List<IEntityType> efEntityTypes, IModel model)
    {
        var entityName = entityClass.Name;
        var efEntityType = efEntityTypes.FirstOrDefault(e => e.ClrType == entityClass);

        if (efEntityType == null)
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = "Entity",
                Status = ComparisonStatus.Error,
                FreeSqlInfo = $"Exists in Entity assembly",
                EFCoreInfo = "Not mapped in EF Core",
                Description = $"Entity {entityName} is not mapped in EF Core DbContext"
            });
            return;
        }

        // Compare table name
        var freeSqlTableAttr = entityClass.GetCustomAttribute<TableAttribute>();
        var efTableName = efEntityType.GetTableName();
        var freeSqlTableName = freeSqlTableAttr?.Name ?? entityClass.Name;

        if (freeSqlTableName != efTableName)
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = "TableName",
                Status = ComparisonStatus.Error,
                FreeSqlInfo = freeSqlTableName,
                EFCoreInfo = efTableName,
                Description = "Table names do not match"
            });
        }
        else
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = "TableName",
                Status = ComparisonStatus.Match,
                FreeSqlInfo = freeSqlTableName,
                EFCoreInfo = efTableName,
                Description = "Table names match"
            });
        }

        // Compare properties/columns
        var properties = entityClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            CompareProperty(entityName, property, efEntityType);
        }
    }

    private void CompareProperty(string entityName, PropertyInfo property, IEntityType efEntityType)
    {
        var propertyName = property.Name;

        // Get FreeSql column info
        var freeSqlColumnAttr = property.GetCustomAttribute<ColumnAttribute>();
        var freeSqlColumnName = freeSqlColumnAttr?.Name ?? propertyName;

        // Get EF Core property info
        var efProperty = efEntityType.FindProperty(propertyName);

        if (efProperty == null)
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = propertyName,
                Status = ComparisonStatus.Warning,
                FreeSqlInfo = $"Column: {freeSqlColumnName}",
                EFCoreInfo = "Not mapped",
                Description = $"Property {propertyName} not found in EF Core mapping"
            });
            return;
        }

        // Compare column names
        var efColumnName = efProperty.GetColumnName();
        if (freeSqlColumnName != efColumnName)
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = propertyName,
                Status = ComparisonStatus.Error,
                FreeSqlInfo = freeSqlColumnName,
                EFCoreInfo = efColumnName,
                Description = "Column names do not match"
            });
        }
        else
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = propertyName,
                Status = ComparisonStatus.Match,
                FreeSqlInfo = freeSqlColumnName,
                EFCoreInfo = efColumnName,
                Description = "Column names match"
            });
        }

        // Compare nullability
        var freeSqlIsNullable = !property.GetCustomAttributes<ColumnAttribute>().Any(a => a.IsNullable == false) &&
                                Nullable.GetUnderlyingType(property.PropertyType) != null || !property.PropertyType.IsValueType;
        var efIsNullable = efProperty.IsNullable;

        if (freeSqlIsNullable != efIsNullable)
        {
            _results.Add(new ComparisonResult
            {
                EntityName = entityName,
                PropertyName = propertyName,
                Status = ComparisonStatus.Warning,
                FreeSqlInfo = freeSqlIsNullable ? "Nullable" : "Not Nullable",
                EFCoreInfo = efIsNullable ? "Nullable" : "Not Nullable",
                Description = "Nullability differs between FreeSql and EF Core"
            });
        }

        // Compare string length (if string property)
        if (property.PropertyType == typeof(string) && freeSqlColumnAttr != null)
        {
            var freeSqlMaxLength = freeSqlColumnAttr.StringLength;
            var efMaxLength = efProperty.GetMaxLength();

            if (freeSqlMaxLength > 0 && efMaxLength.HasValue && freeSqlMaxLength != efMaxLength.Value)
            {
                _results.Add(new ComparisonResult
                {
                    EntityName = entityName,
                    PropertyName = propertyName,
                    Status = ComparisonStatus.Warning,
                    FreeSqlInfo = $"MaxLength: {freeSqlMaxLength}",
                    EFCoreInfo = $"MaxLength: {efMaxLength}",
                    Description = "String length constraints differ"
                });
            }
        }
    }

    /// <summary>
    /// Print a formatted comparison report.
    /// </summary>
    public void PrintReport()
    {
        Console.WriteLine("\n=== Data Structure Comparison Report ===\n");

        var grouped = _results.GroupBy(r => r.Status);

        foreach (var statusGroup in grouped.OrderBy(g => g.Key))
        {
            Console.WriteLine($"\n{statusGroup.Key} ({statusGroup.Count()} items):");
            Console.WriteLine(new string('-', 80));

            foreach (var result in statusGroup)
            {
                Console.WriteLine($"Entity: {result.EntityName}, Property: {result.PropertyName}");
                Console.WriteLine($"  FreeSql : {result.FreeSqlInfo}");
                Console.WriteLine($"  EF Core : {result.EFCoreInfo}");
                Console.WriteLine($"  {result.Description}");
                Console.WriteLine();
            }
        }

        // Summary
        var matches = _results.Count(r => r.Status == ComparisonStatus.Match);
        var warnings = _results.Count(r => r.Status == ComparisonStatus.Warning);
        var errors = _results.Count(r => r.Status == ComparisonStatus.Error);

        Console.WriteLine("\n=== Summary ===");
        Console.WriteLine($"Total Comparisons: {_results.Count}");
        Console.WriteLine($"Matches: {matches} ({(matches * 100.0 / _results.Count):F1}%)");
        Console.WriteLine($"Warnings: {warnings} ({(warnings * 100.0 / _results.Count):F1}%)");
        Console.WriteLine($"Errors: {errors} ({(errors * 100.0 / _results.Count):F1}%)");

        if (errors > 0)
        {
            Console.WriteLine("\n⚠️  CRITICAL: Errors found! Data migration may fail or lose data.");
        }
        else if (warnings > 0)
        {
            Console.WriteLine("\n⚠️  CAUTION: Warnings found. Review before migrating production data.");
        }
        else
        {
            Console.WriteLine("\n✅ All checks passed! Schemas are compatible.");
        }
    }

    public static void Main(string[] args)
    {
        var tool = new DataStructureComparisonTool();
        var results = tool.CompareSchemas();
        tool.PrintReport();

        // Exit with error code if there are errors
        if (results.Any(r => r.Status == ComparisonStatus.Error))
        {
            Environment.Exit(1);
        }
    }
}
