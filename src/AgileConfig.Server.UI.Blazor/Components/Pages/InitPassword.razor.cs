using System.Net.Http.Json;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class InitPasswordBase : ComponentBase
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected MessageService MessageService { get; set; } = default!;
    [Inject] protected ILogger<InitPasswordBase> Logger { get; set; } = default!;

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
        Logger.LogInformation("OnFinish called - starting password initialization");

        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            Logger.LogInformation("Password: {PasswordLength} chars, ConfirmPassword: {ConfirmLength} chars",
                initPasswordModel.Password?.Length ?? 0,
                initPasswordModel.ConfirmPassword?.Length ?? 0);

            // Validate passwords match
            if (initPasswordModel.Password != initPasswordModel.ConfirmPassword)
            {
                errorMessage = "Passwords do not match";
                Logger.LogWarning("Password validation failed - passwords do not match");
                return;
            }

            // Call the API to initialize password
            Logger.LogInformation("Calling API: {BaseAddress}/admin/InitPassword", HttpClient.BaseAddress);

            var response = await HttpClient.PostAsJsonAsync("/admin/InitPassword", new
            {
                password = initPasswordModel.Password,
                confirmPassword = initPasswordModel.ConfirmPassword
            });

            Logger.LogInformation("API response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Logger.LogInformation("API response content: {Content}", content);

                MessageService.Success("Password initialized successfully. Redirecting to login...");
                await Task.Delay(1500); // Give user time to see the success message
                Navigation.NavigateTo("/login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Logger.LogError("API call failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                errorMessage = $"Failed to initialize password: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during password initialization");
            errorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
            Logger.LogInformation("OnFinish completed, isLoading set to false");
        }
    }

    protected class InitPasswordModel
    {
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}
