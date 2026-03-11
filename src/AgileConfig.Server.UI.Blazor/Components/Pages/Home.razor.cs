using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class HomeBase : ComponentBase, IDisposable
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected DashboardStatistics statistics = new();
    protected List<AppModel> recentApps = new();
    protected List<ServerNodeModel> serverNodes = new();
    protected List<SysLogModel> recentLogs = new();
    protected bool loading = false;

    private Timer? _refreshTimer;

    protected RenderFragment NodeValueTemplate => __builder =>
    {
        <text>
            <span>@statistics.NodeOnlineCount / @statistics.NodeCount</span>
        </text>
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardData();

        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadDashboardData();
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    protected async Task LoadDashboardData()
    {
        loading = true;

        try
        {
            var statsResponse = await ApiClient.GetAsync<ApiResponse<DashboardStatistics>>("/api/report/statistics");
            if (statsResponse?.Success == true && statsResponse.Data != null)
            {
                statistics = statsResponse.Data;
            }

            var recentAppsResponse = await ApiClient.GetAsync<ApiResponse<List<AppModel>>>("/api/app/recent");
            if (recentAppsResponse?.Success == true && recentAppsResponse.Data != null)
            {
                recentApps = recentAppsResponse.Data.Take(5).ToList();
            }

            var nodesResponse = await ApiClient.GetAsync<ApiResponse<List<ServerNodeModel>>>("/api/serverNode");
            if (nodesResponse?.Success == true && nodesResponse.Data != null)
            {
                serverNodes = nodesResponse.Data.Take(5).ToList();
            }

            var logsResponse = await ApiClient.GetAsync<ApiResponse<List<SysLogModel>>>("/api/log?pageSize=10");
            if (logsResponse?.Success == true && logsResponse.Data != null)
            {
                recentLogs = logsResponse.Data.Take(5).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard: {ex.Message}");
        }
        finally
        {
            loading = false;
        }
    }

    protected RenderFragment IconRender(string iconType)
    {
        return __builder =>
        {
            <Icon Type="@iconType" Style="color: #1890ff;" />
        };
    }

    protected void NavigateToConfigs(AppModel app)
    {
        Navigation.NavigateTo($"/configs/{app.Id}/{app.Name}");
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
