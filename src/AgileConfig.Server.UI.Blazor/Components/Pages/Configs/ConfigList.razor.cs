using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class ConfigListBase : ComponentBase
{
    [Inject] protected ConfigApiClient ConfigApi { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string AppName { get; set; } = "";

    protected List<ConfigInfo> configs = new();
    protected IEnumerable<ConfigInfo> selectedRows = Array.Empty<ConfigInfo>();
    protected List<string> envList = new() { "DEV", "TEST", "STAGING", "PROD" };
    private string _currentEnv = "DEV";
    protected string currentEnv
    {
        get => _currentEnv;
        set
        {
            if (_currentEnv != value)
            {
                _currentEnv = value;
                _ = OnEnvChange();
            }
        }
    }
    protected bool loading = false;
    protected bool saving = false;
    protected bool publishing = false;

    protected WaitPublishStatus waitPublishStatus = new();

    protected bool modalVisible = false;
    protected bool publishModalVisible = false;
    protected bool isEditMode = false;
    protected AddEditConfigRequest currentConfig = new();
    protected string publishLog = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadConfigs();
        await LoadWaitPublishStatus();
    }

    protected async Task LoadConfigs()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await ConfigApi.SearchAsync(AppId, currentEnv);
            if (response != null)
                configs = response.Data;
        }
        catch (Exception ex) { Console.WriteLine($"Error loading configs: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected async Task LoadWaitPublishStatus()
    {
        try
        {
            var response = await ConfigApi.GetWaitPublishStatusAsync(AppId, currentEnv);
            if (response?.Success == true && response.Data != null)
                waitPublishStatus = response.Data;
        }
        catch (Exception ex) { Console.WriteLine($"Error loading wait publish status: {ex.Message}"); }
    }

    protected async Task OnEnvChange()
    {
        await LoadConfigs();
        await LoadWaitPublishStatus();
    }

    protected bool HasWaitPublish() =>
        waitPublishStatus.AddCount + waitPublishStatus.EditCount + waitPublishStatus.DeleteCount > 0;

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentConfig = new AddEditConfigRequest { AppId = AppId };
        modalVisible = true;
    }

    protected void ShowEditModal(ConfigInfo config)
    {
        isEditMode = true;
        currentConfig = new AddEditConfigRequest
        {
            Id = config.Id,
            AppId = config.AppId,
            Group = config.Group,
            Key = config.Key,
            Value = config.Value,
            Description = config.Description
        };
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();
        try
        {
            AgileResponse? response = isEditMode
                ? await ConfigApi.EditAsync(currentConfig, currentEnv)
                : await ConfigApi.AddAsync(currentConfig, currentEnv);

            if (response?.Success == true)
            {
                modalVisible = false;
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error saving config: {ex.Message}"); }
        finally { saving = false; StateHasChanged(); }
    }

    protected void ShowPublishModal()
    {
        publishLog = "";
        publishModalVisible = true;
    }

    protected async Task HandlePublish()
    {
        publishing = true;
        StateHasChanged();
        try
        {
            var ids = selectedRows.Where(x => x.EditStatus != 10).Select(x => x.Id).ToArray();
            var response = await ConfigApi.PublishAsync(
                new PublishRequest { AppId = AppId, Ids = ids, Log = publishLog },
                currentEnv);

            if (response?.Success == true)
            {
                publishModalVisible = false;
                selectedRows = Array.Empty<ConfigInfo>();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error publishing configs: {ex.Message}"); }
        finally { publishing = false; StateHasChanged(); }
    }

    protected async Task DeleteConfig(ConfigInfo config)
    {
        try
        {
            var response = await ConfigApi.DeleteAsync(config.Id, currentEnv);
            if (response?.Success == true)
            {
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error deleting config: {ex.Message}"); }
    }

    protected async Task DeleteSelected()
    {
        try
        {
            var ids = selectedRows.Select(x => x.Id).ToList();
            var response = await ConfigApi.DeleteSomeAsync(ids, currentEnv);
            if (response?.Success == true)
            {
                selectedRows = Array.Empty<ConfigInfo>();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error deleting selected configs: {ex.Message}"); }
    }

    protected async Task CancelEdit(ConfigInfo config)
    {
        try
        {
            var response = await ConfigApi.CancelEditAsync(config.Id, currentEnv);
            if (response?.Success == true)
            {
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error canceling edit: {ex.Message}"); }
    }

    protected async Task CancelEditSelected()
    {
        try
        {
            var ids = selectedRows.Where(x => x.EditStatus != 10).Select(x => x.Id).ToList();
            var response = await ConfigApi.CancelSomeEditAsync(ids, currentEnv);
            if (response?.Success == true)
            {
                selectedRows = Array.Empty<ConfigInfo>();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error canceling selected edits: {ex.Message}"); }
    }

    protected bool versionHistoryVisible = false;
    protected bool envSyncVisible = false;
    protected bool jsonImportVisible = false;
    protected ConfigInfo? historyConfig = null;
    protected bool configHistoryVisible = false;

    protected void ShowVersionHistory() => versionHistoryVisible = true;
    protected void ShowEnvSync() => envSyncVisible = true;
    protected void ShowJsonImport() => jsonImportVisible = true;
    protected void ShowHistory(ConfigInfo config) { historyConfig = config; configHistoryVisible = true; }

    protected void ExportJson() =>
        Navigation.NavigateTo(ConfigApi.GetExportUrl(AppId, currentEnv), forceLoad: true);
}
