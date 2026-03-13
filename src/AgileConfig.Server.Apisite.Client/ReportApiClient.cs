using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>
/// 统计报表 + 已连接客户端接口 → ReportController。
/// Dashboard 统计数据通过多个接口并行获取后组合。
/// </summary>
public class ReportApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>
    /// 并行调用 AppCount / ConfigCount / NodeCount / ServiceCount，
    /// 组合为一次 DashboardStats 返回。
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var appTask     = http.GetFromJsonAsync<int>("/Report/AppCount", JsonOptions);
        var configTask  = http.GetFromJsonAsync<int>("/Report/ConfigCount", JsonOptions);
        var nodeTask    = http.GetFromJsonAsync<int>("/Report/NodeCount", JsonOptions);
        var serviceTask = http.GetFromJsonAsync<ServiceCountResult>("/Report/ServiceCount", JsonOptions);

        await Task.WhenAll(appTask, configTask, nodeTask, serviceTask);

        var svc = serviceTask.Result;
        return new DashboardStats
        {
            AppCount          = appTask.Result,
            ConfigCount       = configTask.Result,
            NodeCount         = nodeTask.Result,
            ServiceCount      = svc?.ServiceCount ?? 0,
            ServiceOnlineCount = svc?.ServiceOnCount ?? 0
        };
    }

    /// <summary>
    /// GET /Report/SearchServerNodeClients — 搜索已连接的客户端实例。
    /// </summary>
    public async Task<PagedResponse<ClientInfo>?> SearchClientsAsync(
        int current = 1, int pageSize = 20,
        string? address = null, string? appId = null, string? env = null)
    {
        var url = $"/Report/SearchServerNodeClients?current={current}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(address)) url += $"&address={Uri.EscapeDataString(address)}";
        if (!string.IsNullOrWhiteSpace(appId))   url += $"&appId={Uri.EscapeDataString(appId)}";
        if (!string.IsNullOrWhiteSpace(env))      url += $"&env={Uri.EscapeDataString(env)}";
        return await http.GetFromJsonAsync<PagedResponse<ClientInfo>>(url, JsonOptions);
    }

    private record ServiceCountResult(int ServiceCount, int ServiceOnCount);
}
