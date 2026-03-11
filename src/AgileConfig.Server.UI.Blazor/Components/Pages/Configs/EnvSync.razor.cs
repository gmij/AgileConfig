using AgileConfig.Server.UI.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class EnvSyncBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string CurrentEnv { get; set; } = "";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnSuccess { get; set; }

    protected bool saving = false;
    protected string[] selectedEnvs = Array.Empty<string>();
    protected string[] availableEnvs = Array.Empty<string>();

    protected override void OnParametersSet()
    {
        var allEnvs = new[] { "DEV", "TEST", "STAGING", "PROD" };
        availableEnvs = allEnvs.Where(e => e != CurrentEnv).ToArray();
    }

    protected async Task HandleSubmit()
    {
        if (selectedEnvs.Length == 0)
        {
            return;
        }

        saving = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.PostAsync($"/api/config/{AppId}/sync", new
            {
                sourceEnv = CurrentEnv,
                targetEnvs = selectedEnvs
            });

            if (response.IsSuccessStatusCode)
            {
                await OnSuccess.InvokeAsync();
                await VisibleChanged.InvokeAsync(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing environments: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }

    protected async Task HandleCancel()
    {
        selectedEnvs = Array.Empty<string>();
        await VisibleChanged.InvokeAsync(false);
    }
}
