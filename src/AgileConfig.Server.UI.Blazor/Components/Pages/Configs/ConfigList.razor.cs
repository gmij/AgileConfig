using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class ConfigListBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public string AppId { get; set; } = "";

    [Parameter]
    public string AppName { get; set; } = "";

    protected List<ConfigModel> configs = new();
    protected List<ConfigModel> selectedRows = new();
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
    protected ConfigModel currentConfig = new();
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
            var response = await ApiClient.GetAsync<ApiResponse<List<ConfigModel>>>($"/api/config/search?appId={AppId}&env={currentEnv}");
            if (response?.Success == true && response.Data != null)
            {
                configs = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configs: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    protected async Task LoadWaitPublishStatus()
    {
        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<WaitPublishStatus>>($"/api/config/{AppId}/waitPublishStatus?env={currentEnv}");
            if (response?.Success == true && response.Data != null)
            {
                waitPublishStatus = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading wait publish status: {ex.Message}");
        }
    }

    protected async Task OnEnvChange()
    {
        await LoadConfigs();
        await LoadWaitPublishStatus();
    }

    protected bool HasWaitPublish()
    {
        return waitPublishStatus.AddCount + waitPublishStatus.EditCount + waitPublishStatus.DeleteCount > 0;
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentConfig = new ConfigModel { AppId = AppId, Env = currentEnv };
        modalVisible = true;
    }

    protected void ShowEditModal(ConfigModel config)
    {
        isEditMode = true;
        currentConfig = new ConfigModel
        {
            Id = config.Id,
            AppId = config.AppId,
            Group = config.Group,
            Key = config.Key,
            Value = config.Value,
            Description = config.Description,
            Env = currentEnv
        };
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();

        try
        {
            var response = isEditMode
                ? await ApiClient.PutAsync($"/api/config?env={currentEnv}", currentConfig)
                : await ApiClient.PostAsync($"/api/config?env={currentEnv}", currentConfig);

            if (response.IsSuccessStatusCode)
            {
                modalVisible = false;
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
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
            var ids = selectedRows.Where(x => x.EditStatus != 10).Select(x => x.Id).ToList();
            var response = await ApiClient.PostAsync($"/api/config/{AppId}/publish?env={currentEnv}", new
            {
                ids = ids,
                log = publishLog
            });

            if (response.IsSuccessStatusCode)
            {
                publishModalVisible = false;
                selectedRows.Clear();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing configs: {ex.Message}");
        }
        finally
        {
            publishing = false;
            StateHasChanged();
        }
    }

    protected async Task DeleteConfig(ConfigModel config)
    {
        try
        {
            var response = await ApiClient.DeleteAsync($"/api/config/{config.Id}?env={currentEnv}");
            if (response.IsSuccessStatusCode)
            {
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting config: {ex.Message}");
        }
    }

    protected async Task DeleteSelected()
    {
        try
        {
            var ids = selectedRows.Select(x => x.Id).ToList();
            var response = await ApiClient.PostAsync($"/api/config/delete?env={currentEnv}", ids);
            if (response.IsSuccessStatusCode)
            {
                selectedRows.Clear();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting selected configs: {ex.Message}");
        }
    }

    protected async Task CancelEdit(ConfigModel config)
    {
        try
        {
            var response = await ApiClient.PostAsync($"/api/config/{config.Id}/cancelEdit?env={currentEnv}", new { });
            if (response.IsSuccessStatusCode)
            {
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling edit: {ex.Message}");
        }
    }

    protected async Task CancelEditSelected()
    {
        try
        {
            var ids = selectedRows.Where(x => x.EditStatus != 10).Select(x => x.Id).ToList();
            var response = await ApiClient.PostAsync($"/api/config/cancelEdit?env={currentEnv}", ids);
            if (response.IsSuccessStatusCode)
            {
                selectedRows.Clear();
                await LoadConfigs();
                await LoadWaitPublishStatus();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling selected edits: {ex.Message}");
        }
    }

    protected void ShowHistory(ConfigModel config)
    {
        // TODO: Implement history modal
    }

    protected void ShowVersionHistory()
    {
        // TODO: Implement version history modal
    }

    protected void ShowEnvSync()
    {
        // TODO: Implement env sync modal
    }

    protected void ShowJsonImport()
    {
        // TODO: Implement JSON import modal
    }

    protected async Task ExportJson()
    {
        try
        {
            Navigation.NavigateTo($"/api/config/{AppId}/export?env={currentEnv}", true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting JSON: {ex.Message}");
        }
    }
}
