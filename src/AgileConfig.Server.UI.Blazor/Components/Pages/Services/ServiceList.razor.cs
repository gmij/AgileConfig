using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
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
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<ServiceInfoModel> services = new();
    protected List<ServiceInfoModel> filteredServices = new();
    protected bool loading = false;
    protected ServiceSearchModel searchModel = new();

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadServices();

        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadServices();
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    protected async Task LoadServices()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<ServiceInfoModel>>>("/api/service");
            if (response?.Success == true && response.Data != null)
            {
                services = response.Data;
                ApplyFilters();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading services: {ex.Message}");
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
        searchModel = new ServiceSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredServices = services.Where(service =>
        {
            // Filter by service ID
            if (!string.IsNullOrWhiteSpace(searchModel.ServiceId) &&
                !service.ServiceId.Contains(searchModel.ServiceId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by service name
            if (!string.IsNullOrWhiteSpace(searchModel.ServiceName) &&
                !service.ServiceName.Contains(searchModel.ServiceName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(searchModel.Status) &&
                !service.Status.Equals(searchModel.Status, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }).ToList();
    }

    protected async Task UnregisterService(ServiceInfoModel service)
    {
        try
        {
            var response = await ApiClient.DeleteAsync($"/api/service/{service.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadServices();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unregistering service: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
