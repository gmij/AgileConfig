using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Clients;

public class ClientSearchModel
{
    public string? Id { get; set; }
    public string? AppId { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
}

public class ClientListBase : ComponentBase, IDisposable
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<ClientInfoModel> clients = new();
    protected List<ClientInfoModel> filteredClients = new();
    protected bool loading = false;
    protected ClientSearchModel searchModel = new();

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
                ApplyFilters();
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

    protected void HandleSearch()
    {
        ApplyFilters();
    }

    protected void HandleReset()
    {
        searchModel = new ClientSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredClients = clients.Where(client =>
        {
            // Filter by client ID
            if (!string.IsNullOrWhiteSpace(searchModel.Id) &&
                !client.Id.Contains(searchModel.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by app ID
            if (!string.IsNullOrWhiteSpace(searchModel.AppId) &&
                !client.AppId.Contains(searchModel.AppId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by name
            if (!string.IsNullOrWhiteSpace(searchModel.Name) &&
                !client.Name.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(searchModel.Status) &&
                !client.Status.Equals(searchModel.Status, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }).ToList();
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
