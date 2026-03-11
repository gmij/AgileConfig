using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class BackupService : IBackupService
{
    private readonly IAppService _appService;
    private readonly string _backupDirectory;
    private readonly Func<string, IConfigRepository> _configRepositoryAccessor;
    private readonly ILogger<BackupService> _logger;
    private readonly ISettingService _settingService;
    private readonly Func<string, IUow> _uowAccessor;

    public BackupService(
        IAppService appService,
        ISettingService settingService,
        Func<string, IConfigRepository> configRepositoryAccessor,
        Func<string, IUow> uowAccessor,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _appService = appService;
        _settingService = settingService;
        _configRepositoryAccessor = configRepositoryAccessor;
        _uowAccessor = uowAccessor;
        _logger = logger;

        // Get backup directory from configuration, default to /backup
        _backupDirectory = configuration["Backup:BackupDirectory"] ?? "/backup";

        // Create backup directory if it doesn't exist
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    public async Task<(bool success, string fileName, string filePath)> CreateBackupAsync()
    {
        try
        {
            _logger.LogInformation("Starting backup creation...");

            // 1. Get all applications
            var apps = await _appService.GetAllAppsAsync();
            _logger.LogInformation($"Found {apps.Count} applications to backup");

            // 2. Create backup data model
            var backupData = new BackupModel
            {
                Version = "1.0",
                BackupTime = DateTime.UtcNow
            };

            // 3. Get all environments
            var environments = await _settingService.GetEnvironmentList();
            if (environments == null || environments.Length == 0)
            {
                _logger.LogWarning("No environments configured, using default");
                environments = new[] { "" }; // Empty string for default environment
            }

            // 4. Backup each application
            foreach (var app in apps)
            {
                var appBackup = new AppBackupModel
                {
                    AppInfo = app
                };

                // Get inherited apps
                var inheritedApps = await _appService.GetInheritancedAppsAsync(app.Id);
                if (inheritedApps != null && inheritedApps.Any())
                {
                    appBackup.InheritedAppIds = inheritedApps.Select(a => a.Id).ToList();
                }

                // Get configs for each environment
                foreach (var env in environments)
                {
                    using var configRepository = _configRepositoryAccessor(env);
                    var configs = await configRepository.QueryAsync(x =>
                        x.AppId == app.Id && x.Status == ConfigStatus.Enabled);

                    if (configs != null && configs.Any())
                    {
                        appBackup.Environments[env] = new EnvironmentBackupModel
                        {
                            Configs = configs.ToList()
                        };
                    }
                }

                backupData.Apps.Add(appBackup);
            }

            // 5. Serialize to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(backupData, options);

            // 6. Create directory structure: /backup/yyyy-MM-dd/
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var directory = Path.Combine(_backupDirectory, date);
            Directory.CreateDirectory(directory);

            // 7. Create ZIP file
            var fileName = $"backup_{timestamp}.zip";
            var filePath = Path.Combine(directory, fileName);

            using (var zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Create))
            {
                var entry = zipArchive.CreateEntry("backup.json");
                await using var entryStream = entry.Open();
                await using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(json);
            }

            _logger.LogInformation($"Backup created successfully: {filePath}");
            return (true, fileName, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup");
            return (false, null, null);
        }
    }

    public async Task<List<BackupInfo>> ListBackupsAsync()
    {
        try
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(_backupDirectory))
            {
                return backups;
            }

            // Search all subdirectories for backup files
            var backupFiles = Directory.GetFiles(_backupDirectory, "backup_*.zip", SearchOption.AllDirectories);

            foreach (var filePath in backupFiles.OrderByDescending(f => File.GetCreationTime(f)))
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name;

                // Try to read app count from the backup
                var appCount = 0;
                try
                {
                    using var zipArchive = ZipFile.OpenRead(filePath);
                    var entry = zipArchive.GetEntry("backup.json");
                    if (entry != null)
                    {
                        await using var stream = entry.Open();
                        using var reader = new StreamReader(stream);
                        var json = await reader.ReadToEndAsync();
                        var backupData = JsonSerializer.Deserialize<BackupModel>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        appCount = backupData?.Apps?.Count ?? 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error reading backup file {fileName}");
                }

                backups.Add(new BackupInfo
                {
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreateTime = fileInfo.CreationTime,
                    AppCount = appCount
                });
            }

            return backups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing backups");
            return new List<BackupInfo>();
        }
    }

    public async Task<(bool success, string message, RestoreStatistics stats)> RestoreFromFileAsync(Stream fileStream)
    {
        var stats = new RestoreStatistics();

        try
        {
            _logger.LogInformation("Starting restore from uploaded file...");

            // 1. Extract and read JSON from ZIP
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            var entry = zipArchive.GetEntry("backup.json");
            if (entry == null)
            {
                return (false, "Invalid backup file: backup.json not found", stats);
            }

            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var json = await reader.ReadToEndAsync();
            var backupData = JsonSerializer.Deserialize<BackupModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (backupData == null)
            {
                return (false, "Invalid backup file: cannot parse backup.json", stats);
            }

            // 2. Restore applications and configurations
            var result = await RestoreBackupDataAsync(backupData, stats);

            if (result)
            {
                _logger.LogInformation(
                    $"Restore completed: {stats.AppsImported} apps imported, {stats.ConfigsImported} configs imported");
                return (true, "Restore completed successfully", stats);
            }

            return (false, "Restore failed", stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring from file");
            stats.Errors++;
            stats.ErrorMessages.Add(ex.Message);
            return (false, $"Restore failed: {ex.Message}", stats);
        }
    }

    public async Task<(bool success, string message, RestoreStatistics stats)> RestoreFromHistoryAsync(string fileName)
    {
        var stats = new RestoreStatistics();

        try
        {
            var filePath = GetBackupFilePath(fileName);
            if (!File.Exists(filePath))
            {
                return (false, "Backup file not found", stats);
            }

            await using var fileStream = File.OpenRead(filePath);
            return await RestoreFromFileAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring from history");
            stats.Errors++;
            stats.ErrorMessages.Add(ex.Message);
            return (false, $"Restore failed: {ex.Message}", stats);
        }
    }

    public async Task<bool> DeleteBackupAsync(string fileName)
    {
        try
        {
            var filePath = GetBackupFilePath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation($"Backup deleted: {fileName}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting backup {fileName}");
            return false;
        }
    }

    public string GetBackupFilePath(string fileName)
    {
        // Search for the file in all date subdirectories
        if (!Directory.Exists(_backupDirectory))
        {
            return null;
        }

        var files = Directory.GetFiles(_backupDirectory, fileName, SearchOption.AllDirectories);
        return files.FirstOrDefault();
    }

    private async Task<bool> RestoreBackupDataAsync(BackupModel backupData, RestoreStatistics stats)
    {
        try
        {
            // Get all environments
            var environments = await _settingService.GetEnvironmentList();
            if (environments == null || environments.Length == 0)
            {
                environments = new[] { "" };
            }

            // Restore each application
            foreach (var appBackup in backupData.Apps)
            {
                try
                {
                    // Check if app exists
                    var existingApp = await _appService.GetAsync(appBackup.AppInfo.Id);

                    if (existingApp == null)
                    {
                        // Create new app
                        await _appService.AddAsync(appBackup.AppInfo);
                        stats.AppsImported++;
                        _logger.LogInformation($"Imported new app: {appBackup.AppInfo.Name}");
                    }
                    else
                    {
                        // Update existing app
                        await _appService.UpdateAsync(appBackup.AppInfo);
                        stats.AppsUpdated++;
                        _logger.LogInformation($"Updated existing app: {appBackup.AppInfo.Name}");
                    }

                    // Restore configurations for each environment
                    foreach (var envEntry in appBackup.Environments)
                    {
                        var env = envEntry.Key;
                        var envBackup = envEntry.Value;

                        using var configRepository = _configRepositoryAccessor(env);

                        foreach (var config in envBackup.Configs)
                        {
                            try
                            {
                                var existingConfig = await configRepository.GetAsync(config.Id);

                                if (existingConfig == null)
                                {
                                    // Create new config
                                    await configRepository.InsertAsync(config);
                                    stats.ConfigsImported++;
                                }
                                else
                                {
                                    // Update existing config
                                    await configRepository.UpdateAsync(config);
                                    stats.ConfigsUpdated++;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    $"Error restoring config {config.Key} for app {appBackup.AppInfo.Name}");
                                stats.Errors++;
                                stats.ErrorMessages.Add(
                                    $"Config {config.Key}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error restoring app {appBackup.AppInfo.Name}");
                    stats.Errors++;
                    stats.ErrorMessages.Add($"App {appBackup.AppInfo.Name}: {ex.Message}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RestoreBackupDataAsync");
            stats.Errors++;
            stats.ErrorMessages.Add(ex.Message);
            return false;
        }
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
