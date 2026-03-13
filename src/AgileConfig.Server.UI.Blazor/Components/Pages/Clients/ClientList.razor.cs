using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Clients;

public class ClientSearchModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? AppId { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
}

public class ClientListBase : ComponentBase, IDisposable
{
    [Inject] protected ReportApiClient ReportApi { get; set; } = default!;

    protected List<ClientInfo> clients = new();
    protected List<ClientInfo> filteredClients = new();
    protected bool loading = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;
    protected ClientSearchModel searchModel = new();

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadClients();
        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () => { await LoadClients(); StateHasChanged(); });
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    protected async Task LoadClients()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await ReportApi.SearchClientsAsync(pageIndex, pageSize,
                appId: searchModel.AppId, address: searchModel.Address);
            if (response != null)
            {
                clients = response.Data;
                total = response.Total;
                filteredClients = clients;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading clients: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected async Task HandleSearch() => await LoadClients();

    protected async Task HandleReset()
    {
        searchModel = new ClientSearchModel();
        await LoadClients();
    }

    public void Dispose() => _refreshTimer?.Dispose();
}
