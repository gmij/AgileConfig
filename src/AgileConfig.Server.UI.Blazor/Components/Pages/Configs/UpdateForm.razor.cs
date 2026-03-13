using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class UpdateFormBase : ComponentBase
{
    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string AppName { get; set; } = "";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public ConfigModel Model { get; set; } = new();
    [Parameter] public EventCallback<ConfigModel> OnSubmit { get; set; }

    protected bool saving = false;

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();

        try
        {
            await OnSubmit.InvokeAsync(Model);
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }

    protected async Task HandleCancel()
    {
        await VisibleChanged.InvokeAsync(false);
    }
}
