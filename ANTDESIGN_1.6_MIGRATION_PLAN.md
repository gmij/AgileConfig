# AntDesign 1.6.0 Migration and Code-Behind Refactoring Plan

## Overview
This document outlines the complete migration from AntDesign 0.20.8 to 1.6.0 and refactoring all Blazor components to use proper code-behind pattern.

## Phase 1: Completed ✅
- Upgraded AntDesign to 1.6.0
- Removed obsolete AntDesign.ProLayout
- Created Login page with code-behind pattern as example

## Phase 2: Critical Fixes

### 2.1 Replace PageContainer Component
**Issue**: `PageContainer` was part of AntDesign.ProLayout which no longer exists in 1.x

**Affected Pages**: 8 pages
- Home.razor
- Apps/AppList.razor
- Configs/ConfigList.razor
- Users/UserList.razor
- Services/ServiceList.razor
- Nodes/NodeList.razor
- Logs/LogList.razor
- Clients/ClientList.razor

**Solution Pattern**:
```razor
<!-- Old (0.20.8) -->
<PageContainer Title="Page Title">
    <Extra><!-- Toolbar --></Extra>
    <ChildContent><!-- Main content --></ChildContent>
</PageContainer>

<!-- New (1.6.0) -->
<div class="page-header">
    <div class="page-header-content">
        <h1>Page Title</h1>
        <div class="page-header-extra"><!-- Toolbar --></div>
    </div>
</div>
<div class="page-content">
    <!-- Main content -->
</div>
```

### 2.2 Fix Select Component Type Parameters
**Issue**: Select component now requires explicit type parameters

**Examples**:
```razor
<!-- Old -->
<Select @bind-Value="@context.Group">
    <SelectOption Value="@group">@group</SelectOption>
</Select>

<!-- New -->
<Select TItemValue="string" TItem="string"
        @bind-Value="@context.Group">
    <SelectOption TItemValue="string" TItem="string" Value="@group" Label="@group" />
</Select>
```

### 2.3 Fix Statistic Component
**Issue**: Statistic requires TValue parameter

```razor
<!-- Old -->
<Statistic Title="Applications" Value="@statistics.AppCount" />

<!-- New -->
<Statistic TValue="int" Title="Applications" Value="@statistics.AppCount" />
```

### 2.4 Fix Context Naming Conflicts
**Issue**: Nested components with ChildContent cause context naming conflicts

```razor
<!-- Old -->
<Form Model="@model">
    <Select @bind-Value="@context.Property">
        <SelectOption Value="@value">@value</SelectOption>
    </Select>
</Form>

<!-- New -->
<Form Model="@model">
    <SelectContext="formContext">
        <Select TItemValue="string" TItem="string" @bind-Value="@formContext.Property">
            <SelectOption TItemValue="string" TItem="string" Value="@value" Label="@value" />
        </Select>
    </Select>
</Form>
```

## Phase 3: Code-Behind Refactoring

### Pattern Structure
Each component should have 3 files:
1. **ComponentName.razor** - Markup only
2. **ComponentName.razor.cs** - Code-behind class
3. **ComponentName.razor.css** - Component-scoped styles

### Example: Login Page (Already Complete)

**Login.razor**:
```razor
@page "/login"
@inherits LoginBase

<div class="login-container">
    <!-- Markup only -->
</div>
```

**Login.razor.cs**:
```csharp
public class LoginBase : ComponentBase
{
    [Inject] protected Service MyService { get; set; }

    protected bool loading;

    protected async Task OnAction() { }
}
```

**Login.razor.css**:
```css
.login-container {
    /* Styles */
}
```

### 3.1 Pages to Refactor (Priority Order)

1. **Home.razor** - Dashboard
   - Extract statistics loading logic
   - Move timer logic to OnInitializedAsync
   - Create Home.razor.css for grid layout

2. **Apps/AppList.razor** - Application Management
   - Extract CRUD operations
   - Move modal visibility state
   - Create AppList.razor.css

3. **Configs/ConfigList.razor** - Configuration Management
   - Extract publish workflow logic
   - Move environment switching logic
   - Create ConfigList.razor.css

4. **Users/UserList.razor** - User Management
   - Extract user CRUD logic
   - Move role management
   - Create UserList.razor.css

5. **Services/ServiceList.razor** - Service Registry
   - Extract service monitoring logic
   - Move timer for auto-refresh
   - Create ServiceList.razor.css

6. **Nodes/NodeList.razor** - Node Management
   - Extract node CRUD logic
   - Move heartbeat monitoring
   - Create NodeList.razor.css

7. **Logs/LogList.razor** - System Logs
   - Extract log query logic
   - Move pagination
   - Create LogList.razor.css

8. **Clients/ClientList.razor** - Client Connections
   - Extract client monitoring logic
   - Move real-time updates
   - Create ClientList.razor.css

## Phase 4: Missing Config Management Features

### 4.1 Config UpdateForm Component
Create `Configs/Components/UpdateForm.razor` + `.cs` + `.css`

**Purpose**: Edit existing configuration item
**Features**:
- Group (readonly)
- Key (readonly)
- Value (editable textarea, auto-size 3-12 rows)
- Description (editable)

### 4.2 Config EnvSync Component
Create `Configs/Components/EnvSync.razor` + `.cs` + `.css`

**Purpose**: Sync configurations from current environment to other environments
**Features**:
- Show current environment
- Checkbox group for target environments
- Sync operation with confirmation

### 4.3 Config VersionHistory Component
Create `Configs/Components/VersionHistory.razor` + `.cs` + `.css`

**Purpose**: View configuration publish history and rollback
**Features**:
- Timeline of publish events
- Display publish time, user, log message
- Show config changes per version (add/edit/delete status)
- Rollback button for each version (except current)
- Color-coded status tags:
  - Blue: New
  - Gold: Modified
  - Red: Deleted
  - Default: Published

**API Endpoints**:
- GET `/api/config/publishHistory?appId={appId}&env={env}` - Get history
- POST `/api/config/rollback?timelineId={id}&env={env}` - Rollback

### 4.4 Config JsonImport Component
Create `Configs/Components/JsonImport.razor` + `.cs` + `.css`

**Purpose**: Import configurations from JSON file
**Features**:
- File upload
- JSON validation
- Preview before import
- Import confirmation

### 4.5 Config JsonEditor Component
Create `Configs/Components/JsonEditor.razor` + `.cs` + `.css`

**Purpose**: Edit all configurations as JSON
**Features**:
- Monaco editor or textarea with JSON formatting
- Syntax validation
- Save all changes at once

### 4.6 Config TextEditor Component
Create `Configs/Components/TextEditor.razor` + `.cs` + `.css`

**Purpose**: Edit all configurations as text (properties format)
**Features**:
- Text editor with key=value format
- Parse and save

## Phase 5: Missing App Management Features

### 5.1 App UpdateForm Component
Create `Apps/Components/UpdateForm.razor` + `.cs` + `.css`

**Purpose**: Edit existing application
**Features**:
- Name (editable)
- ID (readonly)
- Secret (optional)
- Group (dropdown with create-new option)
- Public (toggle)
- Linked apps (multi-select, hidden if public)
- Enabled (toggle)

### 5.2 App UserAuth Component
Create `Apps/Components/UserAuth.razor` + `.cs` + `.css`

**Purpose**: Manage per-app user permissions
**Features**:
- Table of authorized users
- Permission dropdown (Read/Read & Write)
- Add new user with permission
- Remove user

## Phase 6: MainLayout Refactoring

**Current Issues**:
- Uses inline @code block
- Uses inline styles
- Needs refactoring to code-behind

**Solution**:
- Create MainLayout.razor.cs
- Create MainLayout.razor.css
- Move all logic to code-behind
- Extract menu items to configuration

## Implementation Priority

### Week 1
1. ✅ Phase 1: Upgrade and Login example
2. Fix all Page Container issues (8 pages)
3. Fix all Select/Statistic type parameter issues
4. Refactor Home.razor to code-behind

### Week 2
5. Refactor Apps/AppList.razor to code-behind
6. Create App UpdateForm component
7. Create App UserAuth component
8. Refactor Configs/ConfigList.razor to code-behind

### Week 3
9. Create Config UpdateForm component
10. Create Config EnvSync component
11. Create Config VersionHistory component
12. Create Config JsonImport/JsonEditor/TextEditor components

### Week 4
13. Refactor remaining pages to code-behind (Users, Services, Nodes, Logs, Clients)
14. Refactor MainLayout to code-behind
15. Final testing and bug fixes

## Testing Checklist

### Per Page
- [ ] Page loads without errors
- [ ] Table displays data correctly
- [ ] Pagination works
- [ ] Sorting works (if applicable)
- [ ] Search/Filter works (if applicable)
- [ ] Create/Add modal opens and works
- [ ] Edit modal opens and works
- [ ] Delete confirmation works
- [ ] Status toggles work (if applicable)
- [ ] Real-time updates work (if applicable)
- [ ] Styles render correctly
- [ ] Code-behind logic executes properly

### Config Management Specific
- [ ] Environment switching works
- [ ] Publish workflow works
- [ ] Rollback works
- [ ] Environment sync works
- [ ] JSON import/export works
- [ ] JSON editor works
- [ ] Text editor works
- [ ] Version history displays correctly

### App Management Specific
- [ ] App CRUD works
- [ ] Group management works
- [ ] App inheritance works
- [ ] User authorization works
- [ ] Enable/disable toggle works

## Code Standards

### Naming Conventions
- **Components**: PascalCase (e.g., `UpdateForm.razor`)
- **Code-behind**: ComponentNameBase (e.g., `public class UpdateFormBase`)
- **CSS classes**: kebab-case (e.g., `.update-form-modal`)
- **Methods**: PascalCase for public, camelCase for private (C# standard)
- **Variables**: camelCase (C# standard)

### File Organization
```
Components/
├── Pages/
│   ├── Apps/
│   │   ├── Components/
│   │   │   ├── UpdateForm.razor
│   │   │   ├── UpdateForm.razor.cs
│   │   │   ├── UpdateForm.razor.css
│   │   │   ├── UserAuth.razor
│   │   │   ├── UserAuth.razor.cs
│   │   │   └── UserAuth.razor.css
│   │   ├── AppList.razor
│   │   ├── AppList.razor.cs
│   │   └── AppList.razor.css
│   ├── Configs/
│   │   ├── Components/
│   │   │   ├── UpdateForm.razor
│   │   │   ├── EnvSync.razor
│   │   │   ├── VersionHistory.razor
│   │   │   ├── JsonImport.razor
│   │   │   ├── JsonEditor.razor
│   │   │   └── TextEditor.razor
│   │   ├── ConfigList.razor
│   │   ├── ConfigList.razor.cs
│   │   └── ConfigList.razor.css
│   └── ...
```

### CSS Scoping
- Use component-scoped CSS files (.razor.css)
- Blazor automatically scopes styles to component
- Global styles go in wwwroot/css/
- Use BEM naming for consistency

### Code-Behind Pattern
```csharp
namespace AgileConfig.Server.UI.Blazor.Components.Pages.FeatureName;

public class ComponentNameBase : ComponentBase
{
    // 1. Injected services
    [Inject] protected ServiceName Service { get; set; } = default!;

    // 2. Parameters
    [Parameter] public string SomeParam { get; set; } = "";

    // 3. Protected fields (accessible from razor)
    protected List<Model> items = new();
    protected bool loading;

    // 4. Private fields
    private Timer? _timer;

    // 5. Lifecycle methods
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    // 6. Event handlers
    protected async Task OnAction()
    {
        // Implementation
    }

    // 7. Private methods
    private async Task LoadData()
    {
        // Implementation
    }

    // 8. Dispose if needed
    public void Dispose()
    {
        _timer?.Dispose();
    }
}
```

## Migration from Old Pattern

### Before (Inline)
```razor
@page "/feature"
@inject Service MyService

<div>Content</div>

@code {
    private bool loading;

    protected override async Task OnInitializedAsync()
    {
        // Logic
    }
}

<style>
    .my-class { }
</style>
```

### After (Code-Behind)
**Feature.razor**:
```razor
@page "/feature"
@inherits FeatureBase

<div>Content</div>
```

**Feature.razor.cs**:
```csharp
public class FeatureBase : ComponentBase
{
    [Inject] protected Service MyService { get; set; } = default!;

    protected bool loading;

    protected override async Task OnInitializedAsync()
    {
        // Logic
    }
}
```

**Feature.razor.css**:
```css
.my-class { }
```

## Notes

- All new AntDesign 1.6.0 components require explicit type parameters
- Use `Block` property instead of inline `width: 100%` on buttons
- Replace `LabelCol`/`WrapperCol` with `LabelColSpan`/`WrapperColSpan`
- Context naming must be unique in nested components
- Component-scoped CSS is automatically prefixed by Blazor
