using System.Net.Http.Json;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class InitPasswordBase : ComponentBase
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected MessageService MessageService { get; set; } = default!;

    protected InitPasswordModel initPasswordModel = new();
    protected string? errorMessage;
    protected bool isLoading = false;

    protected FormValidationRule[] passwordRules = new[]
    {
        new FormValidationRule { Required = true, Message = "Password is required" },
        new FormValidationRule { Min = 6, Message = "Password must be at least 6 characters" }
    };

    protected FormValidationRule[] confirmPasswordRules = new[]
    {
        new FormValidationRule { Required = true, Message = "Please confirm your password" }
    };

    protected async Task OnFinish(EditContext editContext)
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            // Validate passwords match
            if (initPasswordModel.Password != initPasswordModel.ConfirmPassword)
            {
                errorMessage = "Passwords do not match";
                return;
            }

            // Call the API to initialize password
            var response = await HttpClient.PostAsJsonAsync("/admin/InitPassword", new
            {
                password = initPasswordModel.Password,
                confirmPassword = initPasswordModel.ConfirmPassword
            });

            if (response.IsSuccessStatusCode)
            {
                MessageService.Success("Password initialized successfully. Redirecting to login...");
                await Task.Delay(1500); // Give user time to see the success message
                Navigation.NavigateTo("/login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                errorMessage = $"Failed to initialize password: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    protected class InitPasswordModel
    {
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}
