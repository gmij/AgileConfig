using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>注册中心服务管理接口 → ServiceController</summary>
public class ServiceApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /Service/Search — 搜索服务实例</summary>
    public async Task<PagedResponse<ServiceInfo>?> SearchAsync(
        int current = 1, int pageSize = 200,
        string? serviceName = null, string? serviceId = null)
    {
        var url = $"/Service/Search?current={current}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(serviceName))
            url += $"&serviceName={Uri.EscapeDataString(serviceName)}";
        if (!string.IsNullOrWhiteSpace(serviceId))
            url += $"&serviceId={Uri.EscapeDataString(serviceId)}";
        return await http.GetFromJsonAsync<PagedResponse<ServiceInfo>>(url, JsonOptions);
    }

    /// <summary>
    /// POST /Service/Remove?id= — 注销服务。
    /// 注意：id 是 ServiceInfo.Id（uniqueId），不是 ServiceId。
    /// </summary>
    public async Task<AgileResponse?> RemoveAsync(string uniqueId)
    {
        var resp = await http.PostAsync($"/Service/Remove?id={Uri.EscapeDataString(uniqueId)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }
}
