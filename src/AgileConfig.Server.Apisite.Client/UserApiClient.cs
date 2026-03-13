using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>用户管理接口 → UserController</summary>
public class UserApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /User/Search — 分页搜索用户</summary>
    public async Task<PagedResponse<UserInfo>?> SearchAsync(
        int current = 1, int pageSize = 20,
        string? userName = null, string? team = null) =>
        await http.GetFromJsonAsync<PagedResponse<UserInfo>>(
            $"/User/Search?current={current}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(userName) ? "" : $"&userName={Uri.EscapeDataString(userName)}") +
            (string.IsNullOrWhiteSpace(team) ? "" : $"&team={Uri.EscapeDataString(team)}"),
            JsonOptions);

    /// <summary>POST /User/Add — 新增用户</summary>
    public async Task<AgileResponse?> AddAsync(AddUserRequest request)
    {
        var resp = await http.PostAsJsonAsync("/User/Add", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /User/Edit — 编辑用户</summary>
    public async Task<AgileResponse?> EditAsync(EditUserRequest request)
    {
        var resp = await http.PostAsJsonAsync("/User/Edit", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /User/Delete?userId= — 删除用户（软删除，Status = Deleted）</summary>
    public async Task<AgileResponse?> DeleteAsync(string userId)
    {
        var resp = await http.PostAsync($"/User/Delete?userId={Uri.EscapeDataString(userId)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>POST /User/ResetPassword?userId= — 重置用户密码为默认值</summary>
    public async Task<AgileResponse?> ResetPasswordAsync(string userId)
    {
        var resp = await http.PostAsync($"/User/ResetPassword?userId={Uri.EscapeDataString(userId)}", null);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }
}
