using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Users;

public class UserSearchModel
{
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public bool? Enabled { get; set; }
}

public class UserListBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<UserModel> users = new();
    protected List<UserModel> filteredUsers = new();
    protected bool loading = false;
    protected bool saving = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;

    protected bool modalVisible = false;
    protected bool isEditMode = false;
    protected UserModel currentUser = new();

    protected UserSearchModel searchModel = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
    }

    protected async Task LoadUsers()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<PagedResult<UserModel>>>($"/api/user?current={pageIndex}&pageSize={pageSize}");
            if (response?.Success == true && response.Data != null)
            {
                users = response.Data.Data ?? new();
                total = response.Data.Total;
                ApplyFilters();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading users: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    protected void HandleSearch()
    {
        ApplyFilters();
    }

    protected void HandleReset()
    {
        searchModel = new UserSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredUsers = users.Where(user =>
        {
            // Filter by username
            if (!string.IsNullOrWhiteSpace(searchModel.UserName) &&
                !user.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(searchModel.Role) &&
                (user.Roles == null || !user.Roles.Contains(searchModel.Role)))
            {
                return false;
            }

            // Filter by enabled status
            if (searchModel.Enabled.HasValue && user.Enabled != searchModel.Enabled.Value)
            {
                return false;
            }

            return true;
        }).ToList();
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentUser = new UserModel { Enabled = true };
        modalVisible = true;
    }

    protected void ShowEditModal(UserModel user)
    {
        isEditMode = true;
        currentUser = new UserModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = user.Roles,
            Enabled = user.Enabled
        };
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();

        try
        {
            var response = isEditMode
                ? await ApiClient.PutAsync("/api/user", currentUser)
                : await ApiClient.PostAsync("/api/user", currentUser);

            if (response.IsSuccessStatusCode)
            {
                modalVisible = false;
                await LoadUsers();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }

    protected async Task ToggleUserStatus(UserModel user)
    {
        try
        {
            var response = await ApiClient.PostAsync($"/api/user/{user.Id}/toggle", new { });
            if (response.IsSuccessStatusCode)
            {
                user.Enabled = !user.Enabled;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling user status: {ex.Message}");
        }
    }

    protected async Task DeleteUser(UserModel user)
    {
        try
        {
            var response = await ApiClient.DeleteAsync($"/api/user/{user.Id}");
            if (response.IsSuccessStatusCode)
            {
                await LoadUsers();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user: {ex.Message}");
        }
    }
}
