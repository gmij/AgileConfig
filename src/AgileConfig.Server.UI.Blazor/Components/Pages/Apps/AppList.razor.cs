using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Apps;

public class AppListBase : ComponentBase
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected List<AppModel> apps = new();
    protected List<string> appGroups = new();
    protected List<AppModel> publicApps = new();
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
    protected AppModel currentApp = new();

    protected List<UserAuthModel> userAuths = new();
    protected string newUserName = "";
    protected string newUserPermission = "R";

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
            var queryParams = new
            {
                current = pageIndex,
                pageSize = pageSize,
                sortField = sortField,
                ascOrDesc = sortOrder,
                tableGrouped = groupAggregation
            };

            var response = await ApiClient.GetAsync<ApiResponse<PagedResult<AppModel>>>("/api/app/search");
            if (response?.Success == true && response.Data != null)
            {
                apps = response.Data.Data ?? new();
                total = response.Data.Total;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading apps: {ex.Message}");
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    protected async Task LoadAppGroups()
    {
        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<string>>>("/api/app/groups");
            if (response?.Success == true && response.Data != null)
            {
                appGroups = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading app groups: {ex.Message}");
        }
    }

    protected async Task LoadPublicApps()
    {
        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<AppModel>>>("/api/app/inheritancedApps");
            if (response?.Success == true && response.Data != null)
            {
                publicApps = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading public apps: {ex.Message}");
        }
    }

    protected async Task OnGroupAggregationChange()
    {
        await LoadApps();
    }

    protected async Task OnTableChange(QueryModel<AppModel> queryModel)
    {
        if (queryModel.SortModel != null && queryModel.SortModel.Length > 0)
        {
            var sort = queryModel.SortModel[0];
            sortField = sort.FieldName;
            sortOrder = sort.Sort;
        }
        await LoadApps();
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentApp = new AppModel { Enabled = true };
        modalVisible = true;
    }

    protected void ShowEditModal(AppModel app)
    {
        isEditMode = true;
        currentApp = new AppModel
        {
            Id = app.Id,
            Name = app.Name,
            Secret = app.Secret,
            Group = app.Group,
            Inheritanced = app.Inheritanced,
            InheritancedApps = app.InheritancedApps,
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
            var endpoint = "/api/app";
            var response = isEditMode
                ? await ApiClient.PutAsync(endpoint, currentApp)
                : await ApiClient.PostAsync(endpoint, currentApp);

            if (response.IsSuccessStatusCode)
            {
                modalVisible = false;
                await LoadApps();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving app: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }

    protected void HandleCancel()
    {
        modalVisible = false;
        currentApp = new();
    }

    protected async Task ToggleEnabled(AppModel app)
    {
        try
        {
            var response = await ApiClient.PostAsync($"/api/app/{app.Id}/enable", new { });
            if (response.IsSuccessStatusCode)
            {
                app.Enabled = !app.Enabled;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling app enabled state: {ex.Message}");
        }
    }

    protected async Task DeleteApp(AppModel app)
    {
        try
        {
            var response = await ApiClient.DeleteAsync($"/api/app/{app.Id}");
            if (response.IsSuccessStatusCode)
            {
                await LoadApps();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting app: {ex.Message}");
        }
    }

    protected void NavigateToConfigs(AppModel app)
    {
        Navigation.NavigateTo($"/configs/{app.Id}/{app.Name}");
    }

    protected async Task ShowAuthModal(AppModel app)
    {
        currentApp = app;
        authModalVisible = true;

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<UserAuthModel>>>($"/api/app/{app.Id}/auth");
            if (response?.Success == true && response.Data != null)
            {
                userAuths = response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user auth: {ex.Message}");
        }
    }

    protected void AddUserAuth()
    {
        if (!string.IsNullOrWhiteSpace(newUserName))
        {
            userAuths.Add(new UserAuthModel
            {
                UserId = newUserName,
                UserName = newUserName,
                Permission = newUserPermission
            });
            newUserName = "";
            newUserPermission = "R";
            StateHasChanged();
        }
    }

    protected void RemoveUserAuth(UserAuthModel userAuth)
    {
        userAuths.Remove(userAuth);
        StateHasChanged();
    }

    protected async Task HandleAuthSubmit()
    {
        saving = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.PostAsync($"/api/app/{currentApp.Id}/auth", userAuths);
            if (response.IsSuccessStatusCode)
            {
                authModalVisible = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving user auth: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }
}
