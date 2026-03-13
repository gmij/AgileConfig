using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>应用管理接口 → AppController</summary>
public class AppApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /App/Search — 分页搜索应用</summary>
    public async Task<PagedResponse<AppInfo>?> SearchAsync(
        int current = 1, int pageSize = 20,
        string? name = null, string? id = null, string? group = null,
        string sortField = "CreateTime", string ascOrDesc = "descend",
        bool tableGrouped = false)
    {
        var url = $"/App/Search?current={current}&pageSize={pageSize}" +
                  $"&sortField={Uri.EscapeDataString(sortField)}&ascOrDesc={Uri.EscapeDataString(ascOrDesc)}" +
                  $"&tableGrouped={tableGrouped}";
        if (!string.IsNullOrWhiteSpace(name))   url += $"&name={Uri.EscapeDataString(name)}";
        if (!string.IsNullOrWhiteSpace(id))     url += $"&id={Uri.EscapeDataString(id)}";
        if (!string.IsNullOrWhiteSpace(group))  url += $"&group={Uri.EscapeDataString(group)}";

        return await http.GetFromJsonAsync<PagedResponse<AppInfo>>(url, JsonOptions);
    }

    /// <summary>GET /App/GetAppGroups — 获取所有应用分组</summary>
    public async Task<AgileResponse<List<string>>?> GetGroupsAsync() =>
        await http.GetFromJsonAsync<AgileResponse<List<string>>>("/App/GetAppGroups", JsonOptions);

    /// <summary>GET /App/InheritancedApps — 获取可被继承的应用列表</summary>
    public async Task<AgileResponse<List<InheritancedAppItem>>?> GetInheritancedAppsAsync(string? currentAppId = null)
    {
        var url = "/App/InheritancedApps";
        if (!string.IsNullOrWhiteSpace(currentAppId))
            url += $"?currentAppId={Uri.EscapeDataString(currentAppId)}";
        return await http.GetFromJsonAsync<AgileResponse<List<InheritancedAppItem>>>(url, JsonOptions);
    }

    /// <summary>POST /App/Add — 新增应用</summary>
    public async Task<AgileResponse?> AddAsync(AddEditAppRequest request)
    {
        var resp = await http.PostAsJsonAsync("/App/Add", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /App/Edit — 编辑应用</summary>
    public async Task<AgileResponse?> EditAsync(AddEditAppRequest request)
    {
        var resp = await http.PostAsJsonAsync("/App/Edit", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /App/DisableOrEnable?id= — 切换应用启用状态</summary>
    public async Task<AgileResponse?> ToggleEnabledAsync(string appId)
    {
        var resp = await http.PostAsync($"/App/DisableOrEnable?id={Uri.EscapeDataString(appId)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /App/Delete?id= — 删除应用</summary>
    public async Task<AgileResponse?> DeleteAsync(string appId)
    {
        var resp = await http.PostAsync($"/App/Delete?id={Uri.EscapeDataString(appId)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>GET /App/GetUserAppAuth?appId= — 获取应用授权用户列表</summary>
    public async Task<AgileResponse<AppAuthInfo>?> GetUserAuthAsync(string appId) =>
        await http.GetFromJsonAsync<AgileResponse<AppAuthInfo>>(
            $"/App/GetUserAppAuth?appId={Uri.EscapeDataString(appId)}", JsonOptions);

    /// <summary>POST /App/SaveAppAuth — 保存应用授权用户列表</summary>
    public async Task<AgileResponse?> SaveUserAuthAsync(string appId, List<string> userIds)
    {
        var resp = await http.PostAsJsonAsync("/App/SaveAppAuth",
            new { appId, authorizedUsers = userIds }, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }
}
