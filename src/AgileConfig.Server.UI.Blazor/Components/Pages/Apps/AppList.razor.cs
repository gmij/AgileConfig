using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Apps;

public class AppListBase : ComponentBase
{
    [Inject] protected AppApiClient AppApi { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected List<AppInfo> apps = new();
    protected List<string> appGroups = new();
    protected List<InheritancedAppItem> publicApps = new();
    protected bool loading = false;
    protected bool saving = false;
    protected bool groupAggregation = false;
    protected int pageIndex = 1;
    protected int pageSize = 20;
    protected int total = 0;
    protected string sortField = "CreateTime";
    protected string sortOrder = "descend";

    protected bool modalVisible = false;
    protected bool authModalVisible = false;
    protected bool isEditMode = false;
    protected AddEditAppRequest currentApp = new();

    protected List<string> authorizedUserIds = new();
    protected List<UserAuthModel> userAuths = new();
    protected string newUserName = "";
    protected string newUserPermission = "R";
    protected string currentAppId = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadApps();
        await LoadAppGroups();
        await LoadPublicApps();
    }

    protected async Task LoadApps()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await AppApi.SearchAsync(pageIndex, pageSize,
                sortField: sortField, ascOrDesc: sortOrder, tableGrouped: groupAggregation);
            if (response != null)
            {
                apps = response.Data;
                total = response.Total;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading apps: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected async Task LoadAppGroups()
    {
        try
        {
            var response = await AppApi.GetGroupsAsync();
            if (response?.Success == true && response.Data != null)
                appGroups = response.Data;
        }
        catch (Exception ex) { Console.WriteLine($"Error loading app groups: {ex.Message}"); }
    }

    protected async Task LoadPublicApps()
    {
        try
        {
            var response = await AppApi.GetInheritancedAppsAsync();
            if (response?.Success == true && response.Data != null)
                publicApps = response.Data;
        }
        catch (Exception ex) { Console.WriteLine($"Error loading public apps: {ex.Message}"); }
    }

    protected async Task OnGroupAggregationChange() => await LoadApps();

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentApp = new AddEditAppRequest { Enabled = true };
        modalVisible = true;
    }

    protected void ShowEditModal(AppInfo app)
    {
        isEditMode = true;
        currentApp = new AddEditAppRequest
        {
            Id = app.Id,
            Name = app.Name,
            Secret = app.Secret,
            Group = app.Group,
            Inheritanced = app.Inheritanced,
            InheritancedApps = new List<string>(app.InheritancedApps),
            Enabled = app.Enabled
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
                ? await AppApi.EditAsync(currentApp)
                : await AppApi.AddAsync(currentApp);

            if (response?.Success == true)
            {
                modalVisible = false;
                await LoadApps();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error saving app: {ex.Message}"); }
        finally { saving = false; StateHasChanged(); }
    }

    protected void HandleCancel()
    {
        modalVisible = false;
        currentApp = new();
    }

    protected async Task ToggleEnabled(AppInfo app)
    {
        try
        {
            var response = await AppApi.ToggleEnabledAsync(app.Id);
            if (response?.Success == true)
            {
                app.Enabled = !app.Enabled;
                StateHasChanged();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error toggling app: {ex.Message}"); }
    }

    protected async Task DeleteApp(AppInfo app)
    {
        try
        {
            var response = await AppApi.DeleteAsync(app.Id);
            if (response?.Success == true)
                await LoadApps();
        }
        catch (Exception ex) { Console.WriteLine($"Error deleting app: {ex.Message}"); }
    }

    protected void NavigateToConfigs(AppInfo app) =>
        Navigation.NavigateTo($"/configs/{app.Id}/{app.Name}");

    protected async Task ShowAuthModal(AppInfo app)
    {
        currentAppId = app.Id;
        authModalVisible = true;
        try
        {
            var response = await AppApi.GetUserAuthAsync(app.Id);
            if (response?.Success == true && response.Data != null)
            {
                authorizedUserIds = new List<string>(response.Data.AuthorizedUsers);
                userAuths = authorizedUserIds.Select(uid => new UserAuthModel { UserId = uid, UserName = uid, Permission = "R" }).ToList();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading user auth: {ex.Message}"); }
    }

    protected void RemoveUserAuth(UserAuthModel model)
    {
        userAuths.Remove(model);
        authorizedUserIds = userAuths.Select(u => u.UserId).ToList();
    }

    protected void AddUserAuth()
    {
        if (!string.IsNullOrWhiteSpace(newUserName))
        {
            userAuths.Add(new UserAuthModel { UserId = newUserName, UserName = newUserName, Permission = newUserPermission });
            authorizedUserIds = userAuths.Select(u => u.UserId).ToList();
            newUserName = "";
            newUserPermission = "R";
        }
    }

    protected async Task HandleAuthSubmit()
    {
        saving = true;
        StateHasChanged();
        try
        {
            var response = await AppApi.SaveUserAuthAsync(currentAppId, authorizedUserIds);
            if (response?.Success == true)
                authModalVisible = false;
        }
        catch (Exception ex) { Console.WriteLine($"Error saving user auth: {ex.Message}"); }
        finally { saving = false; StateHasChanged(); }
    }
}
