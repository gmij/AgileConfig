using AgileConfig.Server.Apisite.Client;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Configs;

public class JsonImportBase : ComponentBase
{
    [Inject] protected ConfigApiClient ConfigApi { get; set; } = default!;

    [Parameter] public string AppId { get; set; } = "";
    [Parameter] public string Env { get; set; } = "";
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback OnSuccess { get; set; }

    protected bool saving = false;
    protected string jsonContent = "";
    protected string? errorMessage;

    protected async Task HandleSubmit()
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            errorMessage = "Please enter JSON content";
            return;
        }

        try { JsonDocument.Parse(jsonContent); }
        catch (JsonException ex)
        {
            errorMessage = $"Invalid JSON format: {ex.Message}";
            return;
        }

        saving = true;
        StateHasChanged();
        try
        {
            var response = await ConfigApi.SaveJsonAsync(AppId, Env, jsonContent);
            if (response?.Success == true)
            {
                jsonContent = "";
                await OnSuccess.InvokeAsync();
                await VisibleChanged.InvokeAsync(false);
            }
            else
            {
                errorMessage = "Failed to import configurations";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
            Console.WriteLine($"Error importing JSON: {ex.Message}");
        }
        finally { saving = false; StateHasChanged(); }
    }

    protected async Task HandleCancel()
    {
        jsonContent = "";
        errorMessage = null;
        await VisibleChanged.InvokeAsync(false);
    }
}
