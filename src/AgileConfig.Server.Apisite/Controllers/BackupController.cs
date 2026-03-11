using System;
using System.IO;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

/// <summary>
/// Controller for backup and restore operations
/// </summary>
[Authorize]
[ModelVaildate]
public class BackupController : Controller
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    /// <summary>
    /// Create a backup of all applications and configurations
    /// </summary>
    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Create })]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var result = await _backupService.CreateBackupAsync();

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = "Backup created successfully",
                    data = new
                    {
                        fileName = result.fileName,
                        filePath = result.filePath
                    }
                });
            }

            return Json(new
            {
                success = false,
                message = "Failed to create backup"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error creating backup: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// List all available backup files
    /// </summary>
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Read })]
    public async Task<IActionResult> List()
    {
        try
        {
            var backups = await _backupService.ListBackupsAsync();

            return Json(new
            {
                success = true,
                data = backups
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error listing backups: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Download a backup file
    /// </summary>
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Read })]
    public IActionResult Download(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required");
            }

            var filePath = _backupService.GetBackupFilePath(fileName);
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound("Backup file not found");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error downloading backup: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Restore from an uploaded backup file
    /// </summary>
    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Restore })]
    public async Task<IActionResult> Restore(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded"
                });
            }

            await using var stream = file.OpenReadStream();
            var result = await _backupService.RestoreFromFileAsync(stream);

            return Json(new
            {
                success = result.success,
                message = result.message,
                data = result.stats
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error restoring backup: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Restore from a historical backup by filename
    /// </summary>
    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Restore })]
    public async Task<IActionResult> RestoreFromHistory(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Json(new
                {
                    success = false,
                    message = "File name is required"
                });
            }

            var result = await _backupService.RestoreFromHistoryAsync(fileName);

            return Json(new
            {
                success = result.success,
                message = result.message,
                data = result.stats
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error restoring from history: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Delete a backup file
    /// </summary>
    [HttpPost]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.Backup_Delete })]
    public async Task<IActionResult> Delete(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Json(new
                {
                    success = false,
                    message = "File name is required"
                });
            }

            var result = await _backupService.DeleteBackupAsync(fileName);

            if (result)
            {
                return Json(new
                {
                    success = true,
                    message = "Backup deleted successfully"
                });
            }

            return Json(new
            {
                success = false,
                message = "Failed to delete backup"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = $"Error deleting backup: {ex.Message}"
            });
        }
    }
}
