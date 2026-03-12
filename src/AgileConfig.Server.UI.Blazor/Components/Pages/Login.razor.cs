using AgileConfig.Server.UI.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class LoginBase : ComponentBase
{
    [Inject] protected AuthenticationService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;

    protected LoginModel loginModel = new();
    protected string? errorMessage;
    protected bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        // Check if SA password has been initialized
        try
        {
            var response = await HttpClient.GetAsync("/home/sys");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var sysInfo = System.Text.Json.JsonSerializer.Deserialize<SystemInfo>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (sysInfo != null && !sysInfo.PasswordInited)
                {
                    // Redirect to initpassword page in the same window
                    Navigation.NavigateTo("/initpassword", forceLoad: false);
                }
            }
        }
        catch
        {
            // If the API call fails, continue to show login page
        }
    }

    protected async Task OnFinish(EditContext editContext)
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var success = await AuthService.LoginAsync(loginModel.Username, loginModel.Password);

            if (success)
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                errorMessage = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    protected class LoginModel
    {
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "";
    }

    protected class SystemInfo
    {
        public string? AppVer { get; set; }
        public bool PasswordInited { get; set; }
        public bool SsoEnabled { get; set; }
        public string? SsoButtonText { get; set; }
    }
}
