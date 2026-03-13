using AgileConfig.Server.Apisite.Client;
using AgileConfig.Server.Apisite.Client.Models;
using Microsoft.AspNetCore.Components;

namespace AgileConfig.Server.UI.Blazor.Components.Pages.Nodes;

public class NodeSearchModel
{
    public string? Address { get; set; }
    public int? Status { get; set; }
}

public class NodeListBase : ComponentBase, IDisposable
{
    [Inject] protected NodeApiClient NodeApi { get; set; } = default!;

    protected List<ServerNodeInfo> nodes = new();
    protected List<ServerNodeInfo> filteredNodes = new();
    protected bool loading = false;
    protected bool saving = false;

    protected bool modalVisible = false;
    protected bool isEditMode = false;
    protected AddNodeRequest currentNode = new();
    protected bool statusChecked = true;

    protected NodeSearchModel searchModel = new();

    private Timer? _refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadNodes();
        _refreshTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () => { await LoadNodes(); StateHasChanged(); });
        }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    protected async Task LoadNodes()
    {
        loading = true;
        StateHasChanged();
        try
        {
            var response = await NodeApi.GetAllAsync();
            if (response?.Success == true && response.Data != null)
            {
                nodes = response.Data;
                ApplyFilters();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error loading nodes: {ex.Message}"); }
        finally { loading = false; StateHasChanged(); }
    }

    protected void HandleSearch() => ApplyFilters();

    protected void HandleReset()
    {
        searchModel = new NodeSearchModel();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredNodes = nodes.Where(n =>
        {
            if (!string.IsNullOrWhiteSpace(searchModel.Address) &&
                !n.Address.Contains(searchModel.Address, StringComparison.OrdinalIgnoreCase))
                return false;
            if (searchModel.Status.HasValue && n.Status != searchModel.Status.Value)
                return false;
            return true;
        }).ToList();
    }

    protected void ShowAddModal()
    {
        isEditMode = false;
        currentNode = new AddNodeRequest();
        statusChecked = true;
        modalVisible = true;
    }

    protected async Task HandleSubmit()
    {
        saving = true;
        StateHasChanged();
        try
        {
            var response = await NodeApi.AddAsync(currentNode);
            if (response?.Success == true)
            {
                modalVisible = false;
                await LoadNodes();
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error saving node: {ex.Message}"); }
        finally { saving = false; StateHasChanged(); }
    }

    protected async Task DeleteNode(ServerNodeInfo node)
    {
        try
        {
            var response = await NodeApi.DeleteAsync(node.Address);
            if (response?.Success == true)
                await LoadNodes();
        }
        catch (Exception ex) { Console.WriteLine($"Error deleting node: {ex.Message}"); }
    }

    public void Dispose() => _refreshTimer?.Dispose();
}
