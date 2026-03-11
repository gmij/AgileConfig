using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace AgileConfig.Server.UI.Blazor.Services;

public class AuthenticationService
{
    private readonly ApiClient _apiClient;
    private readonly NavigationManager _navigationManager;
    private string? _jwtToken;
    private string? _currentUser;

    public event Action? OnAuthStateChanged;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);
    public string? CurrentUser => _currentUser;

    public AuthenticationService(ApiClient apiClient, NavigationManager navigationManager)
    {
        _apiClient = apiClient;
        _navigationManager = navigationManager;
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
                    _jwtToken = result.Data.Token;
                    _currentUser = username;
                    _apiClient.SetAuthToken(_jwtToken);

                    // Store in session storage (using JS interop in real implementation)
                    // For now, keep in memory

                    OnAuthStateChanged?.Invoke();
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

    public void Logout()
    {
        _jwtToken = null;
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
        _navigationManager.NavigateTo("/login");
    }

    public async Task<bool> CheckAuthAsync()
    {
        // In a real implementation, validate token with backend
        // For now, just check if token exists
        return await Task.FromResult(IsAuthenticated);
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
