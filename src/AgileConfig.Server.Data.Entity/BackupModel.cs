using System;
using System.Collections.Generic;

namespace AgileConfig.Server.Data.Entity;

/// <summary>
/// Root backup model containing all applications and their configurations
/// </summary>
public class BackupModel
{
    public string Version { get; set; } = "1.0";
    public DateTime BackupTime { get; set; }
    public List<AppBackupModel> Apps { get; set; } = new();
}

/// <summary>
/// Backup model for a single application
/// </summary>
public class AppBackupModel
{
    public App AppInfo { get; set; }
    public List<string> InheritedAppIds { get; set; } = new();
    public Dictionary<string, EnvironmentBackupModel> Environments { get; set; } = new();
}

/// <summary>
/// Backup model for an environment's configurations
/// </summary>
public class EnvironmentBackupModel
{
    public List<Config> Configs { get; set; } = new();
}

/// <summary>
/// Information about a backup file
/// </summary>
public class BackupInfo
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime CreateTime { get; set; }
    public int AppCount { get; set; }
}

/// <summary>
/// Statistics from a restore operation
/// </summary>
public class RestoreStatistics
{
    public int AppsImported { get; set; }
    public int ConfigsImported { get; set; }
    public int AppsUpdated { get; set; }
    public int ConfigsUpdated { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
}
