using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class VersionHistoryBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;
    [Inject] protected ModalService ModalService { get; set; } = default!;

    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string AppName { get; set; } = "";
    [Parameter] public string Env { get; set; } = "";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnSuccess { get; set; }

    protected List<PublishDetailNode> historyData = new();
    protected bool loading = false;

    protected override async Task OnParametersSetAsync()
    {
        if (Visible && historyData.Count == 0)
        {
            await LoadHistory();
        }
    }

    protected async Task LoadHistory()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<PublishDetailNode>>>($"/api/config/{AppId}/publishHistory?env={Env}");
            if (response?.Success == true && response.Data != null)
            {
                historyData = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading version history: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    protected async Task ShowRollbackConfirm(PublishDetailNode publishNode)
    {
        var confirmResult = await ModalService.ConfirmAsync(new ConfirmOptions
        {
            Title = "Rollback Confirmation",
            Content = $"Are you sure to rollback to version published at {publishNode.TimelineNode.PublishTime:yyyy-MM-dd HH:mm:ss}?\n\nThis operation cannot be undone. All current configurations will be replaced with this version.",
            OkText = "Rollback",
            CancelText = "Cancel",
            OkButtonProps = new ButtonProps { Danger = true }
        });

        if (confirmResult)
        {
            await HandleRollback(publishNode.TimelineNode.Id);
        }
    }

    protected async Task HandleRollback(string timelineId)
    {
        try
        {
            var response = await ApiClient.PostAsync($"/api/config/{AppId}/rollback?env={Env}&timelineId={timelineId}", new { });
            if (response.IsSuccessStatusCode)
            {
                await OnSuccess.InvokeAsync();
                await VisibleChanged.InvokeAsync(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rolling back: {ex.Message}");
        }
    }

    protected async Task HandleCancel()
    {
        await VisibleChanged.InvokeAsync(false);
    }
}
