using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ApiClient _apiClient;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        ApiClient apiClient)
    {
        _sessionStorage = sessionStorage;
        _apiClient = apiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
            var usernameResult = await _sessionStorage.GetAsync<string>("username");

            if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
            {
                return new AuthenticationState(_anonymous);
            }

            var token = tokenResult.Value;
            var username = usernameResult.Success ? usernameResult.Value : "Unknown";

            // Set the auth token in the API client
            _apiClient.SetAuthToken(token);

            // Create claims from token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username ?? "Unknown"),
                new Claim("token", token)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(string token, string username)
    {
        await _sessionStorage.SetAsync("authToken", token);
        await _sessionStorage.SetAsync("username", username);

        // Set the auth token in the API client
        _apiClient.SetAuthToken(token);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("token", token)
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.DeleteAsync("authToken");
        await _sessionStorage.DeleteAsync("username");

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
