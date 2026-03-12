using Microsoft.AspNetCore.Components;
using System.Net;

namespace AgileConfig.Server.UI.Blazor.Services;

/// <summary>
/// HTTP message handler that intercepts 401/403 responses and redirects to login page
/// This ensures that when login expires, the entire browser navigates to login, not just the component
/// </summary>
public class AuthHttpHandler : DelegatingHandler
{
    private readonly NavigationManager _navigationManager;

    public AuthHttpHandler(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // If we get 401 (Unauthorized) or 403 (Forbidden), redirect to login page
        // using forceLoad=true to force full browser navigation
        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            _navigationManager.NavigateTo("/login", forceLoad: true);
        }

        return response;
    }
}
