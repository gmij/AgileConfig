using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.IService;

/// <summary>
/// Service for creating and restoring backups of application configurations
/// </summary>
public interface IBackupService : IDisposable
{
    /// <summary>
    /// Create a backup of all applications and configurations
    /// </summary>
    /// <returns>Success status, file name, and file path</returns>
    Task<(bool success, string fileName, string filePath)> CreateBackupAsync();

    /// <summary>
    /// List all available backup files
    /// </summary>
    /// <returns>List of backup information</returns>
    Task<List<BackupInfo>> ListBackupsAsync();

    /// <summary>
    /// Restore from an uploaded backup file
    /// </summary>
    /// <param name="fileStream">The backup file stream</param>
    /// <returns>Success status, message, and statistics</returns>
    Task<(bool success, string message, RestoreStatistics stats)> RestoreFromFileAsync(Stream fileStream);

    /// <summary>
    /// Restore from a historical backup by filename
    /// </summary>
    /// <param name="fileName">The backup file name</param>
    /// <returns>Success status, message, and statistics</returns>
    Task<(bool success, string message, RestoreStatistics stats)> RestoreFromHistoryAsync(string fileName);

    /// <summary>
    /// Delete a backup file
    /// </summary>
    /// <param name="fileName">The backup file name to delete</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteBackupAsync(string fileName);

    /// <summary>
    /// Get the physical path of a backup file
    /// </summary>
    /// <param name="fileName">The backup file name</param>
    /// <returns>Full file path</returns>
    string GetBackupFilePath(string fileName);
}
