using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace AgileConfig.Server.Apisite.Client;

/// <summary>
/// DelegatingHandler，从当前 HTTP 上下文的用户 Claims 中提取 JWT Token
/// 并将其作为 Bearer Token 附加到每个向 Apisite 发出的 HTTP 请求。
///
/// 注意：不要在 HttpClientFactory 的消息处理管线中依赖 Razor 组件作用域服务
/// （如 AuthenticationStateProvider、NavigationManager）。
/// </summary>
public class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = httpContextAccessor.HttpContext?.User.FindFirst("token")?.Value;

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
