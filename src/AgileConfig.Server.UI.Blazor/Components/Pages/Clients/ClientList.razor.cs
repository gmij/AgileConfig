using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Clients;

public class ClientListBase : ComponentBase, IDisposable
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<ClientInfoModel> clients = new();
    protected bool loading = false;

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadClients();

        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadClients();
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    protected async Task LoadClients()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<ClientInfoModel>>>("/api/client");
            if (response?.Success == true && response.Data != null)
            {
                clients = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading clients: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
