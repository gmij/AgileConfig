using System.Net.Http.Json;
using System.Text.Json;
using AgileConfig.Server.Apisite.Client.Models;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>
/// 所有类型化客户端的基类，提供统一的 JSON 序列化选项。
/// </summary>
public abstract class ApiClientBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected static HttpContent ToJson<T>(T data) =>
        JsonContent.Create(data, options: JsonOptions);
}
