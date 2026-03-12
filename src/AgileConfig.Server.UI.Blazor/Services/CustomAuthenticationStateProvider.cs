using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AgileConfig.Server.UI.Blazor.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiClient _apiClient;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ApiClient apiClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _apiClient = apiClient;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Extract token from claims
                var tokenClaim = httpContext.User.FindFirst("token");
                if (tokenClaim != null)
                {
                    _apiClient.SetAuthToken(tokenClaim.Value);
                }

                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            return Task.FromResult(new AuthenticationState(_anonymous));
        }
        catch
        {
            return Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public Task<ClaimsPrincipal> CreateUserPrincipal(string token, string username)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("token", token)
        };

        var identity = new ClaimsIdentity(claims, "Blazor.Cookie");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(user);
    }

    public void NotifyUserAuthentication()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(httpContext.User)));
        }
    }

    public void NotifyUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
