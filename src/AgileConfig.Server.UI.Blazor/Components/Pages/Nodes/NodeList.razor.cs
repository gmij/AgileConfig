using AgileConfig.Server.UI.Blazor.Services;
using AgileConfig.Server.UI.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Nodes;

public class NodeSearchModel
{
    public string? Address { get; set; }
    public int? Status { get; set; }
}

public class NodeListBase : ComponentBase, IDisposable
{
    [Inject] protected ApiClient ApiClient { get; set; } = default!;

    protected List<ServerNodeModel> nodes = new();
    protected List<ServerNodeModel> filteredNodes = new();
    protected bool loading = false;
    protected bool saving = false;

    protected bool modalVisible = false;
    protected bool isEditMode = false;
    protected ServerNodeModel currentNode = new();
    protected bool statusChecked = true;

    protected NodeSearchModel searchModel = new();

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadNodes();

        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadNodes();
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    protected async Task LoadNodes()
    {
        loading = true;
        StateHasChanged();

        try
        {
            var response = await ApiClient.GetAsync<ApiResponse<List<ServerNodeModel>>>("/api/serverNode");
            if (response?.Success == true && response.Data != null)
            {
                nodes = response.Data;
                ApplyFilters();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading nodes: {ex.Message}");
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
        searchModel = new NodeSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredNodes = nodes.Where(node =>
        {
            // Filter by address
            if (!string.IsNullOrWhiteSpace(searchModel.Address) &&
                !node.Address.Contains(searchModel.Address, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Filter by status
            if (searchModel.Status.HasValue && node.Status != searchModel.Status.Value)
            {
                return false;
            }

            return true;
        }).ToList();
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentNode = new ServerNodeModel { Status = 1 };
        statusChecked = true;
        modalVisible = true;
    }

    protected void ShowEditModal(ServerNodeModel node)
    {
        isEditMode = true;
        currentNode = new ServerNodeModel
        {
            Id = node.Id,
            Address = node.Address,
            Remark = node.Remark,
            Status = node.Status
        };
        statusChecked = node.Status == 1;
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        currentNode.Status = statusChecked ? 1 : 0;
        StateHasChanged();

        try
        {
            var response = isEditMode
                ? await ApiClient.PutAsync("/api/serverNode", currentNode)
                : await ApiClient.PostAsync("/api/serverNode", currentNode);

            if (response.IsSuccessStatusCode)
            {
                modalVisible = false;
                await LoadNodes();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving node: {ex.Message}");
        }
        finally
        {
            saving = false;
            StateHasChanged();
        }
    }

    protected async Task DeleteNode(ServerNodeModel node)
    {
        try
        {
            var response = await ApiClient.DeleteAsync($"/api/serverNode/{node.Id}");
            if (response.IsSuccessStatusCode)
            {
                await LoadNodes();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting node: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
