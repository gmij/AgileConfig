using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Users;

public class UserSearchModel
{
    public string? UserName { get; set; }
    public string? Team { get; set; }
    public string? Role { get; set; }
    public int? StatusFilter { get; set; }
}

public class UserListBase : ComponentBase
{
    [Inject] protected UserApiClient UserApi { get; set; } = default!;

    protected List<UserInfo> users = new();
    protected List<UserInfo> filteredUsers = new();
    protected bool loading = false;
    protected bool saving = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;

    protected bool modalVisible = false;
    protected bool isEditMode = false;
    protected AddUserRequest addRequest = new();
    protected EditUserRequest editRequest = new();
    protected UserModel currentUser = new();

    protected UserSearchModel searchModel = new();

    protected override async Task OnInitializedAsync() => await LoadUsers();

    protected async Task LoadUsers()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await UserApi.SearchAsync(pageIndex, pageSize);
            if (response != null)
            {
                users = response.Data;
                total = response.Total;
                ApplyFilters();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading users: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected void HandleSearch() => ApplyFilters();

    protected void HandleReset()
    {
        searchModel = new UserSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredUsers = users.Where(u =>
        {
            if (!string.IsNullOrWhiteSpace(searchModel.UserName) &&
                !u.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrWhiteSpace(searchModel.Team) &&
                (u.Team == null || !u.Team.Contains(searchModel.Team, StringComparison.OrdinalIgnoreCase)))
                return false;
            return true;
        }).ToList();
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        addRequest = new AddUserRequest();
        currentUser = new UserModel();
        modalVisible = true;
    }

    protected void ShowEditModal(UserInfo user)
    {
        isEditMode = true;
        editRequest = new EditUserRequest
        {
            Id = user.Id,
            UserName = user.UserName,
            Team = user.Team,
            UserRoleIds = new List<string>(user.UserRoleIds)
        };
        currentUser = new UserModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = new List<string>(user.UserRoleNames)
        };
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();
        try
        {
            AgileResponse? response;
            if (isEditMode)
            {
                editRequest.UserName = currentUser.UserName;
                editRequest.UserRoleIds = currentUser.Roles ?? new List<string>();
                response = await UserApi.EditAsync(editRequest);
            }
            else
            {
                addRequest.UserName = currentUser.UserName;
                addRequest.Password = currentUser.Password ?? "";
                addRequest.UserRoleIds = currentUser.Roles ?? new List<string>();
                response = await UserApi.AddAsync(addRequest);
            }

            if (response?.Success == true)
            {
                modalVisible = false;
                await LoadUsers();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error saving user: {ex.Message}"); }
        finally { saving = false; StateHasChanged(); }
    }

    protected async Task DeleteUser(UserInfo user)
    {
        try
        {
            var response = await UserApi.DeleteAsync(user.Id);
            if (response?.Success == true)
                await LoadUsers();
        }
        catch (Exception ex) { Console.WriteLine($"Error deleting user: {ex.Message}"); }
    }

    protected async Task ResetPassword(UserInfo user)
    {
        try { await UserApi.ResetPasswordAsync(user.Id); }
        catch (Exception ex) { Console.WriteLine($"Error resetting password: {ex.Message}"); }
    }
}
