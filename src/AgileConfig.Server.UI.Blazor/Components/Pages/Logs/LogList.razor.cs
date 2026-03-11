using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Logs;

public class LogListBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<SysLogModel> logs = new();
    protected bool loading = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadLogs();
    }

    protected async Task LoadLogs()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<PagedResult<SysLogModel>>>($"/api/log?current={pageIndex}&pageSize={pageSize}");
            if (response?.Success == true && response.Data != null)
            {
                logs = response.Data.Data ?? new();
                total = response.Data.Total;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading logs: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }
}
