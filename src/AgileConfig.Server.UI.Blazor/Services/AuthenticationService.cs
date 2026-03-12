using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Services;

public class AuthenticationService
{
    private readonly ApiClient _apiClient;
    private readonly NavigationManager _navigationManager;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(
        ApiClient apiClient,
        NavigationManager navigationManager,
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _navigationManager = navigationManager;
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _apiClient.PostAsync("/admin/jwt/login", new { userName = username, password = password });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JwtResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Status == "ok" && !string.IsNullOrEmpty(result.Token))
                {
                    // Create user principal with claims
                    var userPrincipal = await _authStateProvider.CreateUserPrincipal(result.Token, username);

                    // Sign in using cookie authentication
                    var httpContext = _httpContextAccessor.HttpContext;
                    if (httpContext != null)
                    {
                        await httpContext.SignInAsync("Blazor.Cookie", userPrincipal);
                    }

                    // Set the auth token in the API client
                    _apiClient.SetAuthToken(result.Token);

                    // Notify the authentication state has changed
                    _authStateProvider.NotifyUserAuthentication();

                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync("Blazor.Cookie");
        }

        _authStateProvider.NotifyUserLoggedOut();
        _navigationManager.NavigateTo("/login", true);
    }

    public async Task<bool> CheckAuthAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    private class JwtResponse
    {
        public string Status { get; set; } = "";
        public string Token { get; set; } = "";
        public string Type { get; set; } = "";
    }
}
