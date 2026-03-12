using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Services;

public class AuthenticationService
{
    private readonly ApiClient _apiClient;
    private readonly NavigationManager _navigationManager;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthenticationService(
        ApiClient apiClient,
        NavigationManager navigationManager,
        AuthenticationStateProvider authStateProvider)
    {
        _apiClient = apiClient;
        _navigationManager = navigationManager;
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
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
                    // Store token using the authentication state provider
                    await _authStateProvider.MarkUserAsAuthenticated(result.Token, username);
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
        await _authStateProvider.MarkUserAsLoggedOut();
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
