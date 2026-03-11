using AgileConfig.Server.UI.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class LoginBase : ComponentBase
{
    [Inject] protected AuthenticationService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected LoginModel loginModel = new();
    protected string? errorMessage;
    protected bool isLoading = false;

    protected async Task OnFinish()
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
}
