using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace AgileConfig.Server.UI.Blazor.Services;

/// <summary>
/// Extends ServerAuthenticationStateProvider which automatically reads auth state
/// from HttpContext.User on the initial HTTP request (F5 refresh), then maintains it
/// throughout the SignalR circuit lifetime.
/// </summary>
public class CustomAuthenticationStateProvider : ServerAuthenticationStateProvider
{
    private readonly ApiClient _apiClient;

    public CustomAuthenticationStateProvider(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = await base.GetAuthenticationStateAsync();

        if (state.User.Identity?.IsAuthenticated == true)
        {
            var token = state.User.FindFirst("token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _apiClient.SetAuthToken(token);
            }
        }

        return state;
    }
}
