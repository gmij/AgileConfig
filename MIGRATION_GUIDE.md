# AgileConfig Full-Stack .NET Migration - Implementation Guide

## Overview

This document provides a comprehensive guide for migrating AgileConfig from FreeSql to EF Core Code First and from React to Blazor, maintaining all existing REST API and WebSocket protocols.

## Architecture Overview

### Data Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    AgileConfig.Server.Apisite                │
│                   (ASP.NET Core 10 WebAPI)                   │
└──────────────────────┬──────────────────────────────────────┘
                       │
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
┌────────────────────┐     ┌────────────────────┐
│  IService Layer    │     │  WebSocket Layer   │
│  (Business Logic)  │     │  (Real-time Push)  │
└─────────┬──────────┘     └────────────────────┘
          │
          ▼
┌────────────────────────────┐
│  Repository.Selector       │
│  (Provider Selection)      │
└──────┬──────────┬──────────┘
       │          │
       ▼          ▼
┌──────────┐  ┌──────────────────┐
│ FreeSql  │  │  EF Core         │
│ (Legacy) │  │  (New)           │
└──────────┘  └──┬───────────────┘
                 │
                 ▼
         ┌───────────────────┐
         │ AgileConfigDbContext│
         │ (17 DbSets)        │
         └───────────────────┘
```

### Frontend Architecture

```
┌─────────────────────────────────────────┐
│         React UI (Existing)              │
│         Ant Design + REST/WS             │
└─────────────────────────────────────────┘
                   │
                   │ Gradual Migration
                   ▼
┌─────────────────────────────────────────┐
│      Blazor UI (New)                     │
│      AntDesign Blazor + REST/WS          │
├─────────────────────────────────────────┤
│  Components:                             │
│  • LoginPage                             │
│  • AppManagement (List/Create/Edit)      │
│  • ConfigManagement (List/Create/Edit)   │
│  • PublishManagement (Timeline/Rollback) │
│  • ServiceRegistry (List/Monitor)        │
│  • UserManagement (List/Roles)           │
│  • SystemSettings                        │
└─────────────────────────────────────────┘
                   │
                   │ Consumes
                   ▼
┌─────────────────────────────────────────┐
│    Existing REST API + WebSocket         │
│    /api/config/*, /ws                    │
└─────────────────────────────────────────┘
```

## Completed Work

### 1. EF Core Data Layer ✅

#### AgileConfig.Server.Data.EFCore
- **AgileConfigDbContext.cs**: Complete DbContext with all 17 entity configurations
  - Apps, Configs, Users, Roles, Functions
  - ServiceInfo, PublishTimeline, ServerNodes
  - All relationships, indexes, and column mappings

- **AgileConfigDbSeedData.cs**: Seed data for initialization
  - Admin user (username: admin, password: 123456)
  - System roles (SuperAdmin, Administrator, Operator)
  - 14 default functions (permissions)
  - Role-function mappings

- **Migrations/InitialCreate.cs**: Database schema migration
  - Creates all tables with correct schema
  - Applies seed data
  - Supports SQL Server, MySQL, PostgreSQL, SQLite

#### AgileConfig.Server.Data.Repository.EFCore
Implemented 18 repositories:

1. **Base Classes**:
   - `EFCoreRepository<T, TId>`: Generic repository with CRUD operations
   - `EFCoreUow`: Unit of Work implementation

2. **Entity Repositories**:
   - AppRepository, AppInheritancedRepository
   - ConfigRepository, ConfigPublishedRepository
   - PublishTimelineRepository, PublishDetailRepository
   - UserRepository, UserRoleRepository, UserAppAuthRepository
   - RoleDefinitionRepository, RoleFunctionRepository, FunctionRepository
   - ServiceInfoRepository, ServerNodeRepository
   - SysLogRepository, SettingRepository
   - SysInitRepository (system initialization)

## Next Steps Implementation Guide

### Step 1: Fix Interface Compatibility Issues

The following issues need to be resolved:

```csharp
// File: PublishTimelineRepository.cs
public class PublishTimelineRepository : EFCoreRepository<PublishTimeline, string>, IPublishTimelineRepository
{
    public PublishTimelineRepository(AgileConfigDbContext context) : base(context) { }

    // Add missing method
    public async Task<string?> GetLastPublishTimelineNodeIdAsync(string appId, string env)
    {
        var timeline = await _context.PublishTimelines
            .Where(x => x.AppId == appId && x.Env == env)
            .OrderByDescending(x => x.PublishTime)
            .FirstOrDefaultAsync();
        return timeline?.Id;
    }
}

// File: EFCoreRepositoryServiceRegister.cs
public class EFCoreRepositoryServiceRegister : IRepositoryServiceRegister
{
    public void Register(IServiceCollection services)
    {
        // existing code...
    }

    // Add missing methods
    public bool IsSuit4Provider(string provider)
    {
        return provider.Equals("efcore", StringComparison.OrdinalIgnoreCase);
    }

    public void AddFixedRepositories(IServiceCollection services)
    {
        // Add repositories that don't depend on provider selection
        services.AddScoped<ISysInitRepository, SysInitRepository>();
    }

    public T GetServiceByEnv<T>(IServiceProvider serviceProvider, string env)
    {
        // Simple implementation - return the registered service
        return serviceProvider.GetRequiredService<T>();
    }
}
```

### Step 2: Update Repository Selector

```csharp
// File: AgileConfig.Server.Data.Repository.Selector/RepositorySelector.cs

public static class RepositorySelector
{
    public static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["db__provider"] ?? "freesql";

        if (provider.Equals("efcore", StringComparison.OrdinalIgnoreCase))
        {
            // Register EF Core DbContext
            var connectionString = configuration["db__conn"];
            services.AddDbContext<AgileConfigDbContext>(options =>
            {
                switch (provider.ToLower())
                {
                    case "sqlserver":
                        options.UseSqlServer(connectionString);
                        break;
                    case "mysql":
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                        break;
                    case "npgsql":
                        options.UseNpgsql(connectionString);
                        break;
                    case "sqlite":
                    default:
                        options.UseSqlite(connectionString);
                        break;
                }
            });

            var register = new EFCoreRepositoryServiceRegister();
            register.Register(services);
        }
        else
        {
            // Use FreeSql (existing)
            var register = new FreesqlRepositoryServiceRegister(configuration);
            register.Register(services);
        }
    }
}
```

### Step 3: Update Apisite Startup

```csharp
// File: AgileConfig.Server.Apisite/Program.cs

var builder = WebApplication.CreateBuilder(args);

// Add EF Core or FreeSql based on configuration
RepositorySelector.RegisterRepositories(builder.Services, builder.Configuration);

// Rest of the existing configuration...

var app = builder.Build();

// Run migrations on startup (EF Core only)
if (builder.Configuration["db__provider"]?.Equals("efcore", StringComparison.OrdinalIgnoreCase) == true)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AgileConfigDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
```

### Step 4: Create Blazor Frontend

#### Create Project
```bash
cd src
dotnet new blazorserver -n AgileConfig.Server.UI.Blazor -f net10.0
cd AgileConfig.Server.UI.Blazor
dotnet add package AntDesign --version 0.20.8
```

#### Project Structure
```
AgileConfig.Server.UI.Blazor/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Login.razor
│   │   ├── Apps/
│   │   │   ├── AppList.razor
│   │   │   ├── AppEdit.razor
│   │   │   └── AppCreate.razor
│   │   ├── Configs/
│   │   │   ├── ConfigList.razor
│   │   │   ├── ConfigEdit.razor
│   │   │   └── ConfigPublish.razor
│   │   ├── Services/
│   │   │   ├── ServiceList.razor
│   │   │   └── ServiceMonitor.razor
│   │   └── Users/
│   │       ├── UserList.razor
│   │       └── RoleManagement.razor
│   └── Shared/
│       ├── ConfigEditor.razor
│       └── WebSocketClient.razor
├── Services/
│   ├── ApiClient.cs
│   ├── WebSocketService.cs
│   └── AuthenticationService.cs
└── wwwroot/
```

#### Key Component Examples

**Login.razor**
```razor
@page "/login"
@using AntDesign
@inject NavigationManager NavigationManager
@inject AuthenticationService AuthService

<div class="login-container">
    <Card Title="AgileConfig Login">
        <Form Model="@loginModel" OnFinish="OnFinish">
            <FormItem Label="Username">
                <Input @bind-Value="@loginModel.Username" />
            </FormItem>
            <FormItem Label="Password">
                <InputPassword @bind-Value="@loginModel.Password" />
            </FormItem>
            <FormItem>
                <Button Type="@ButtonType.Primary" HtmlType="submit">
                    Login
                </Button>
            </FormItem>
        </Form>
    </Card>
</div>

@code {
    private LoginModel loginModel = new();

    private async Task OnFinish()
    {
        var result = await AuthService.LoginAsync(loginModel.Username, loginModel.Password);
        if (result.Success)
        {
            NavigationManager.NavigateTo("/");
        }
    }

    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
```

**WebSocketService.cs**
```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Agile.Config.Protocol;

public class WebSocketService : IDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;

    public event Action<WebsocketAction>? OnMessage;

    public async Task ConnectAsync(string appId, string secret, string env)
    {
        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        var uri = new Uri($"ws://localhost:5000/ws?appid={appId}&env={env}");

        // Add Basic Auth header
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appId}:{secret}"));
        _ws.Options.SetRequestHeader("Authorization", $"Basic {credentials}");

        await _ws.ConnectAsync(uri, _cts.Token);
        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        while (_ws?.State == WebSocketState.Open && !_cts!.Token.IsCancellationRequested)
        {
            var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var action = JsonSerializer.Deserialize<WebsocketAction>(message);
                OnMessage?.Invoke(action!);
            }
        }
    }

    public async Task SendPingAsync()
    {
        var ping = new WebsocketAction
        {
            Module = ActionModule.ConfigCenter,
            Action = ActionConst.Ping,
            Data = ""
        };
        var json = JsonSerializer.Serialize(ping);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _ws!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts!.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _ws?.Dispose();
    }
}
```

### Step 5: Update Docker Configuration

**Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy projects
COPY ["src/AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj", "src/AgileConfig.Server.Apisite/"]
COPY ["src/AgileConfig.Server.Data.EFCore/AgileConfig.Server.Data.EFCore.csproj", "src/AgileConfig.Server.Data.EFCore/"]
# ... other projects

RUN dotnet restore "src/AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj"

COPY . .
WORKDIR "/src/src/AgileConfig.Server.Apisite"
RUN dotnet build "AgileConfig.Server.Apisite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgileConfig.Server.Apisite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef --version 10.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "AgileConfig.Server.Apisite.dll"]
```

**docker-compose.yml**
```yaml
version: '3.8'

services:
  agileconfig:
    image: agileconfig:latest
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    environment:
      - TZ=Asia/Shanghai
      - adminConsole=true
      - db__provider=efcore
      - db__conn=Data Source=agile_config.db
      - ASPNETCORE_URLS=http://+:5000
    volumes:
      - ./data:/app/db
    restart: unless-stopped

  # Example with PostgreSQL
  agileconfig-postgres:
    image: agileconfig:latest
    ports:
      - "5001:5000"
    environment:
      - db__provider=efcore
      - db__conn=Host=postgres;Database=agileconfig;Username=agile;Password=config123
    depends_on:
      - postgres

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=agileconfig
      - POSTGRES_USER=agile
      - POSTGRES_PASSWORD=config123
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:
```

## Protocol Compatibility

### REST API (Preserved)
All existing REST endpoints remain unchanged:
- `GET /api/config/app/{appId}?env=xxx` - Pull configurations
- `POST /api/config` - Add/Update configuration
- `POST /api/config/publish` - Publish configurations
- `GET /api/app` - List applications
- `POST /api/user/login` - User authentication
- All other existing endpoints...

### WebSocket Protocol (Preserved)
WebSocket endpoint `/ws` with:
- **Connection**: Basic Auth with appid:secret
- **Message Format**: `{"Module":"c|r","Action":"ping|reload|offline","Data":""}`
- **Client Ping**: `c:ping` or `ping`
- **Server Response**: Ping with config MD5 or timeline ID
- **Server Push**: `reload` when config published, `offline` when node removed

### Response Headers (Preserved)
- `publish-time-line-id`: Included in config pull responses
- Standard HTTP status codes and error formats

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class EFCoreRepositoryTests
{
    private AgileConfigDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AgileConfigDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AgileConfigDbContext(options);
    }

    [TestMethod]
    public async Task AppRepository_CRUD_Operations()
    {
        using var context = GetInMemoryContext();
        var repo = new AppRepository(context);

        var app = new App
        {
            Id = "test_app",
            Name = "Test App",
            Secret = "secret123",
            CreateTime = DateTime.Now,
            Enabled = true
        };

        // Insert
        await repo.InsertAsync(app);

        // Get
        var retrieved = await repo.GetAsync("test_app");
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("Test App", retrieved.Name);

        // Update
        retrieved.Name = "Updated App";
        await repo.UpdateAsync(retrieved);

        // Delete
        await repo.DeleteAsync("test_app");
        var deleted = await repo.GetAsync("test_app");
        Assert.IsNull(deleted);
    }
}
```

### Integration Tests
```csharp
[TestClass]
public class ConfigControllerTests
{
    [TestMethod]
    public async Task GetConfig_ReturnsConfigWithPublishTimelineId()
    {
        // Arrange
        var client = new TestWebApplicationFactory().CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("test_app:secret"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        // Act
        var response = await client.GetAsync("/api/config/app/test_app?env=DEV");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.IsTrue(response.Headers.Contains("publish-time-line-id"));

        var content = await response.Content.ReadAsStringAsync();
        var configs = JsonSerializer.Deserialize<List<Config>>(content);
        Assert.IsNotNull(configs);
    }
}
```

### WebSocket Tests
```csharp
[TestClass]
public class WebSocketTests
{
    [TestMethod]
    public async Task WebSocket_PingPong_Works()
    {
        using var ws = new ClientWebSocket();
        var uri = new Uri("ws://localhost:5000/ws?appid=test_app&env=DEV");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("test_app:secret"));
        ws.Options.SetRequestHeader("Authorization", $"Basic {credentials}");

        await ws.ConnectAsync(uri, CancellationToken.None);

        // Send ping
        var ping = new WebsocketAction { Module = "c", Action = "ping", Data = "" };
        var json = JsonSerializer.Serialize(ping);
        await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);

        // Receive pong
        var buffer = new byte[4096];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var action = JsonSerializer.Deserialize<WebsocketAction>(response);

        Assert.AreEqual("ping", action.Action);
        Assert.IsNotNull(action.Data); // Should contain MD5 or timeline ID
    }
}
```

## Migration Path

### Phase 1: Parallel Run (Week 1-2)
1. Deploy both FreeSql and EF Core implementations
2. Use configuration flag to switch between providers
3. Run both in production with monitoring
4. Verify data consistency

### Phase 2: EF Core Primary (Week 3-4)
1. Switch default to EF Core
2. Keep FreeSql as fallback
3. Monitor performance and errors
4. Fix any compatibility issues

### Phase 3: FreeSql Removal (Week 5-6)
1. Remove FreeSql dependencies
2. Clean up code
3. Update documentation
4. Final testing

### Phase 4: Blazor Rollout (Week 7-10)
1. Deploy Blazor UI alongside React
2. Beta testing with selected users
3. Gradual migration of users
4. Deprecate React UI

## Performance Considerations

### EF Core Optimizations
```csharp
// Use AsNoTracking for read-only queries
public async Task<List<Config>> GetPublishedConfigsAsync(string appId, string env)
{
    return await _context.Configs
        .AsNoTracking()
        .Where(x => x.AppId == appId && x.Env == env && x.Status == ConfigStatus.Enabled)
        .ToListAsync();
}

// Use compiled queries for hot paths
private static readonly Func<AgileConfigDbContext, string, string, Task<List<Config>>>
    GetConfigsCompiled = EF.CompileAsyncQuery(
        (AgileConfigDbContext context, string appId, string env) =>
            context.Configs
                .Where(x => x.AppId == appId && x.Env == env)
                .ToList());

// Batch operations
public async Task PublishConfigsAsync(List<Config> configs)
{
    await _context.Configs.AddRangeAsync(configs);
    await _context.SaveChangesAsync();
}
```

## Conclusion

This migration maintains 100% backward compatibility with existing clients while modernizing the technology stack. The phased approach minimizes risk and allows for gradual adoption. All protocol contracts are preserved, ensuring seamless operation during and after the migration.
