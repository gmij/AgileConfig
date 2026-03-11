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
            _apiClient.SetBasicAuth(username, password);
            var response = await _apiClient.PostAsync("/api/admin/jwt", new { });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JwtResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true && !string.IsNullOrEmpty(result.Data?.Token))
                {
                    // Store token using the authentication state provider
                    await _authStateProvider.MarkUserAsAuthenticated(result.Data.Token, username);
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
        public bool Success { get; set; }
        public string? Message { get; set; }
        public JwtData? Data { get; set; }
    }

    private class JwtData
    {
        public string Token { get; set; } = "";
    }
}
