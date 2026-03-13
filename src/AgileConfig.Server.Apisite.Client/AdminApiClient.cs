using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>登录相关接口（不需要 Bearer Token）</summary>
public class AdminApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>
    /// 调用 POST /admin/jwt/login 获取 JWT Token。
    /// </summary>
    public async Task<LoginResult?> LoginAsync(string userName, string password)
    {
        var resp = await http.PostAsJsonAsync("/admin/jwt/login",
            new { userName, password }, JsonOptions);

        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<LoginResult>(JsonOptions);
    }
}
