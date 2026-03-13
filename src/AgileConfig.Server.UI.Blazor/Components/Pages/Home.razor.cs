using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class HomeBase : ComponentBase, IDisposable
{
    [Inject] protected AppApiClient AppApi { get; set; } = default!;
    [Inject] protected NodeApiClient NodeApi { get; set; } = default!;
    [Inject] protected SysLogApiClient SysLogApi { get; set; } = default!;
    [Inject] protected ReportApiClient ReportApi { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected DashboardStats statistics = new();
    protected List<AppInfo> recentApps = new();
    protected List<ServerNodeInfo> serverNodes = new();
    protected List<SysLogInfo> recentLogs = new();
    protected bool loading = false;

    private Timer? _refreshTimer;

    protected string GetNodeValueText() => $"{statistics.ServiceOnlineCount} / {statistics.NodeCount}";

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardData();
        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () => { await LoadDashboardData(); StateHasChanged(); });
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    protected async Task LoadDashboardData()
    {
        loading = true;
        try
        {
            // 并行加载所有数据
            var statsTask   = ReportApi.GetDashboardStatsAsync();
            var appsTask    = AppApi.SearchAsync(current: 1, pageSize: 5, sortField: "CreateTime", ascOrDesc: "descend");
            var nodesTask   = NodeApi.GetAllAsync();
            var logsTask    = SysLogApi.SearchAsync(current: 1, pageSize: 5);

            await Task.WhenAll(statsTask, appsTask, nodesTask, logsTask);

            statistics  = statsTask.Result;
            recentApps  = appsTask.Result?.Data.Take(5).ToList() ?? new();
            serverNodes = nodesTask.Result?.Data?.Take(5).ToList() ?? new();
            recentLogs  = logsTask.Result?.Data.Take(5).ToList() ?? new();
        }
        catch (Exception ex) { Console.WriteLine($"Error loading dashboard: {ex.Message}"); }
        finally { loading = false; }
    }

    protected void NavigateToConfigs(AppInfo app) =>
        Navigation.NavigateTo($"/configs/{app.Id}/{app.Name}");

    public void Dispose() => _refreshTimer?.Dispose();
}
