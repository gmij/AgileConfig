using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>操作日志接口 → SysLogController</summary>
public class SysLogApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /SysLog/Search — 分页搜索操作日志</summary>
    public async Task<PagedResponse<SysLogInfo>?> SearchAsync(
        int current = 1, int pageSize = 20,
        string? appId = null, int? logType = null,
        DateTime? startTime = null, DateTime? endTime = null)
    {
        var url = $"/SysLog/Search?current={current}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(appId)) url += $"&appId={Uri.EscapeDataString(appId)}";
        if (logType.HasValue) url += $"&logType={logType.Value}";
        if (startTime.HasValue) url += $"&startTime={Uri.EscapeDataString(startTime.Value.ToString("yyyy-MM-dd"))}";
        if (endTime.HasValue)   url += $"&endTime={Uri.EscapeDataString(endTime.Value.ToString("yyyy-MM-dd"))}";
        return await http.GetFromJsonAsync<PagedResponse<SysLogInfo>>(url, JsonOptions);
    }
}
