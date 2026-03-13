using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Services;

public class ServiceSearchModel
{
    public string? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? Status { get; set; }
}

public class ServiceListBase : ComponentBase, IDisposable
{
    [Inject] protected ServiceApiClient ServiceApi { get; set; } = default!;

    protected List<ServiceInfo> services = new();
    protected List<ServiceInfo> filteredServices = new();
    protected bool loading = false;
    protected ServiceSearchModel searchModel = new();

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadServices();
        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () => { await LoadServices(); StateHasChanged(); });
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    protected async Task LoadServices()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await ServiceApi.SearchAsync();
            if (response != null)
            {
                services = response.Data;
                ApplyFilters();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading services: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected void HandleSearch() => ApplyFilters();

    protected void HandleReset()
    {
        searchModel = new ServiceSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredServices = services.Where(s =>
        {
            if (!string.IsNullOrWhiteSpace(searchModel.ServiceId) &&
                !s.ServiceId.Contains(searchModel.ServiceId, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrWhiteSpace(searchModel.ServiceName) &&
                !s.ServiceName.Contains(searchModel.ServiceName, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrWhiteSpace(searchModel.Status))
            {
                var expectedStatus = searchModel.Status == "Healthy" ? 0 : 1;
                if (s.Status != expectedStatus) return false;
            }
            return true;
        }).ToList();
    }

    protected async Task UnregisterService(ServiceInfo service)
    {
        try
        {
            // 注意：RemoveAsync 使用 service.Id（uniqueId），不是 service.ServiceId
            var response = await ServiceApi.RemoveAsync(service.Id);
            if (response?.Success == true)
                await LoadServices();
        }
        catch (Exception ex) { Console.WriteLine($"Error unregistering service: {ex.Message}"); }
    }

    public void Dispose() => _refreshTimer?.Dispose();
}
