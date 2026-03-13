using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace AgileConfig.Server.UI.Blazor.Services;

/// <summary>
/// 继承 ServerAuthenticationStateProvider，在初始 HTTP 请求时从 HttpContext.User
/// 读取认证状态，后续 SignalR 电路复用该状态。
/// JWT Token 的附加由 SDK 的 BearerTokenHandler 在每次请求时自动完成，
/// 无需在此处再操作 HttpClient。
/// </summary>
public class CustomAuthenticationStateProvider : ServerAuthenticationStateProvider
{
}
