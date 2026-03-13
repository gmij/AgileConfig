using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>配置项管理接口 → ConfigController</summary>
public class ConfigApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /Config/Search?appId=&env= — 搜索配置项</summary>
    public async Task<PagedResponse<ConfigInfo>?> SearchAsync(
        string appId, string env,
        string? group = null, string? key = null,
        int current = 1, int pageSize = 200)
    {
        var url = $"/Config/Search?appId={Uri.EscapeDataString(appId)}&env={Uri.EscapeDataString(env)}" +
                  $"&current={current}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(group)) url += $"&group={Uri.EscapeDataString(group)}";
        if (!string.IsNullOrWhiteSpace(key))   url += $"&key={Uri.EscapeDataString(key)}";
        return await http.GetFromJsonAsync<PagedResponse<ConfigInfo>>(url, JsonOptions);
    }

    /// <summary>GET /Config/WaitPublishStatus?appId=&env= — 获取待发布数量统计</summary>
    public async Task<AgileResponse<WaitPublishStatus>?> GetWaitPublishStatusAsync(string appId, string env) =>
        await http.GetFromJsonAsync<AgileResponse<WaitPublishStatus>>(
            $"/Config/WaitPublishStatus?appId={Uri.EscapeDataString(appId)}&env={Uri.EscapeDataString(env)}",
            JsonOptions);

    /// <summary>POST /Config/Add?env= — 新增配置项</summary>
    public async Task<AgileResponse?> AddAsync(AddEditConfigRequest request, string env)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/Add?env={Uri.EscapeDataString(env)}", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/Edit?env= — 编辑配置项</summary>
    public async Task<AgileResponse?> EditAsync(AddEditConfigRequest request, string env)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/Edit?env={Uri.EscapeDataString(env)}", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/Delete?id=&env= — 删除单个配置项</summary>
    public async Task<AgileResponse?> DeleteAsync(string id, string env)
    {
        var resp = await http.PostAsync(
            $"/Config/Delete?id={Uri.EscapeDataString(id)}&env={Uri.EscapeDataString(env)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/DeleteSome?env= — 批量删除配置项</summary>
    public async Task<AgileResponse?> DeleteSomeAsync(List<string> ids, string env)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/DeleteSome?env={Uri.EscapeDataString(env)}", ids, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/Publish?env= — 发布配置</summary>
    public async Task<AgileResponse?> PublishAsync(PublishRequest request, string env)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/Publish?env={Uri.EscapeDataString(env)}", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/CancelEdit?configId=&env= — 撤销单条编辑</summary>
    public async Task<AgileResponse?> CancelEditAsync(string configId, string env)
    {
        var resp = await http.PostAsync(
            $"/Config/CancelEdit?configId={Uri.EscapeDataString(configId)}&env={Uri.EscapeDataString(env)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/CancelSomeEdit?env= — 批量撤销编辑</summary>
    public async Task<AgileResponse?> CancelSomeEditAsync(List<string> ids, string env)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/CancelSomeEdit?env={Uri.EscapeDataString(env)}", ids, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>返回导出 JSON 文件所需的完整 URL（供浏览器直接下载）</summary>
    public string GetExportUrl(string appId, string env) =>
        $"/Config/ExportJson?appId={Uri.EscapeDataString(appId)}&env={Uri.EscapeDataString(env)}";

    /// <summary>POST /Config/SyncEnv?appId=&currentEnv= — 环境间同步配置</summary>
    public async Task<AgileResponse?> SyncEnvAsync(string appId, string currentEnv, List<string> targetEnvs)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/SyncEnv?appId={Uri.EscapeDataString(appId)}&currentEnv={Uri.EscapeDataString(currentEnv)}",
            targetEnvs, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /Config/SaveJson?appId=&env= — 导入 JSON 配置</summary>
    public async Task<AgileResponse?> SaveJsonAsync(string appId, string env, string json, bool isPatch = false)
    {
        var resp = await http.PostAsJsonAsync(
            $"/Config/SaveJson?appId={Uri.EscapeDataString(appId)}&env={Uri.EscapeDataString(env)}",
            new { json, isPatch }, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>GET /Config/PublishHistory?appId=&env= — 获取发布历史</summary>
    public async Task<AgileResponse<List<PublishHistoryEntry>>?> GetPublishHistoryAsync(string appId, string env) =>
        await http.GetFromJsonAsync<AgileResponse<List<PublishHistoryEntry>>>(
            $"/Config/PublishHistory?appId={Uri.EscapeDataString(appId)}&env={Uri.EscapeDataString(env)}",
            JsonOptions);

    /// <summary>POST /Config/Rollback?publishTimelineId=&env= — 回滚到指定版本</summary>
    public async Task<AgileResponse?> RollbackAsync(string publishTimelineId, string env)
    {
        var resp = await http.PostAsync(
            $"/Config/Rollback?publishTimelineId={Uri.EscapeDataString(publishTimelineId)}&env={Uri.EscapeDataString(env)}",
            null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }
}
