using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class VersionHistoryBase : ComponentBase
{
    [Inject] protected ConfigApiClient ConfigApi { get; set; } = default!;
    [Inject] protected ModalService ModalService { get; set; } = default!;

    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string AppName { get; set; } = "";
    [Parameter] public string Env { get; set; } = "";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnSuccess { get; set; }

    protected List<PublishHistoryEntry> historyData = new();
    protected bool loading = false;

    protected override async Task OnParametersSetAsync()
    {
        if (Visible && historyData.Count == 0)
            await LoadHistory();
    }

    protected async Task LoadHistory()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await ConfigApi.GetPublishHistoryAsync(AppId, Env);
            if (response?.Success == true && response.Data != null)
                historyData = response.Data;
        }
        catch (Exception ex) { Console.WriteLine($"Error loading version history: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected async Task ShowRollbackConfirm(PublishHistoryEntry entry)
    {
        if (entry.TimelineNode == null) return;

        var confirmResult = await ModalService.ConfirmAsync(new ConfirmOptions
        {
            Title = "Rollback Confirmation",
            Content = $"Are you sure to rollback to version published at {entry.TimelineNode.PublishTime:yyyy-MM-dd HH:mm:ss}?\n\nThis operation cannot be undone.",
            OkText = "Rollback",
            CancelText = "Cancel",
            OkButtonProps = new ButtonProps { Danger = true }
        });

        if (confirmResult)
            await HandleRollback(entry.TimelineNode.Id);
    }

    protected async Task HandleRollback(string timelineId)
    {
        try
        {
            var response = await ConfigApi.RollbackAsync(timelineId, Env);
            if (response?.Success == true)
            {
                await OnSuccess.InvokeAsync();
                await VisibleChanged.InvokeAsync(false);
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error rolling back: {ex.Message}"); }
    }

    protected async Task HandleCancel() => await VisibleChanged.InvokeAsync(false);
}
