using System.Net.Http.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>服务节点管理接口 → ServerNodeController</summary>
public class NodeApiClient(HttpClient http) : ApiClientBase
{
    /// <summary>GET /ServerNode/All — 获取所有节点</summary>
    public async Task<AgileResponse<List<ServerNodeInfo>>?> GetAllAsync() =>
        await http.GetFromJsonAsync<AgileResponse<List<ServerNodeInfo>>>("/ServerNode/All", JsonOptions);

    /// <summary>POST /ServerNode/Add — 新增节点</summary>
    public async Task<AgileResponse?> AddAsync(AddNodeRequest request)
    {
        var resp = await http.PostAsJsonAsync("/ServerNode/Add", request, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }

    /// <summary>
    /// POST /ServerNode/Delete — 删除节点。
    /// 注意：Apisite 通过 address 字段识别节点，body 为 { address }。
    /// </summary>
    public async Task<AgileResponse?> DeleteAsync(string address)
    {
        var resp = await http.PostAsJsonAsync("/ServerNode/Delete", new { address }, JsonOptions);
        if (!resp.IsSuccessStatusCode) return new AgileResponse { Success = false };
        return await resp.Content.ReadFromJsonAsync<AgileResponse>(JsonOptions);
    }
}
