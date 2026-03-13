using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Logs;

public class LogListBase : ComponentBase
{
    [Inject] protected SysLogApiClient SysLogApi { get; set; } = default!;

    protected List<SysLogInfo> logs = new();
    protected bool loading = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;

    protected override async Task OnInitializedAsync() => await LoadLogs();

    protected async Task LoadLogs()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await SysLogApi.SearchAsync(pageIndex, pageSize);
            if (response != null)
            {
                logs = response.Data;
                total = response.Total;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading logs: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }
}
