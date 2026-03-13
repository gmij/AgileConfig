# Blazor Authentication Implementation Guide

## Overview

This document describes the authentication and authorization implementation for the AgileConfig Blazor UI, which has been updated to properly integrate with the existing JWT-based authentication system from the AgileConfig Apisite.

## Architecture

### Authentication Flow

```
User → Login Page → Basic Auth → AdminController → JWT Token → AuthenticationStateProvider → Protected Pages
                                     ↓
                              ProtectedSessionStorage
                                  (Persistent)
```

### Components

#### 1. CustomAuthenticationStateProvider
**Location**: `Services/CustomAuthenticationStateProvider.cs`

**Purpose**: Manages authentication state across the application using ASP.NET Core's built-in authentication system.

**Features**:
- Integrates with `ProtectedSessionStorage` for secure, encrypted token storage
- Persists authentication state across page reloads
- Properly cascades authentication state to all components
- Implements `AuthenticationStateProvider` base class

**Key Methods**:
- `GetAuthenticationStateAsync()`: Retrieves current authentication state from session storage
- `MarkUserAsAuthenticated(token, username)`: Stores token and notifies of authentication
- `MarkUserAsLoggedOut()`: Clears token and notifies of logout

#### 2. AuthenticationService
**Location**: `Services/AuthenticationService.cs`

**Purpose**: Handles login/logout operations and communicates with the backend API.

**Features**:
- Exchanges username/password for JWT token via Basic auth
- Delegates token storage to `CustomAuthenticationStateProvider`
- Provides logout functionality

**Login Flow**:
1. User submits credentials
2. Service sets Basic auth header on ApiClient
3. Calls `/api/admin/jwt` endpoint
4. Receives JWT token in response
5. Stores token via AuthenticationStateProvider
6. ApiClient is configured with Bearer token for subsequent requests

#### 3. ApiClient
**Location**: `Services/ApiClient.cs`

**Purpose**: HTTP client wrapper for API communication.

**Authentication Methods**:
- `SetBasicAuth(username, password)`: Sets Basic authentication header
- `SetAuthToken(token)`: Sets Bearer token for JWT authentication

#### 4. Route Protection

**Routes.razor**: Implements authorization routing
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

**Protected Pages**: All main pages except Login use `@attribute [Authorize]`
- Home.razor
- Apps/AppList.razor
- Configs/ConfigList.razor
- Users/UserList.razor
- Services/ServiceList.razor
- Nodes/NodeList.razor
- Logs/LogList.razor
- Clients/ClientList.razor

**Public Pages**: Login page uses `@attribute [AllowAnonymous]`
- Login.razor

#### 5. MainLayout

**Location**: `Components/Layout/MainLayout.razor`

**Features**:
- Uses `<AuthorizeView>` to conditionally render navigation and header
- Displays username from authentication state: `@context.User.Identity?.Name`
- Provides logout functionality via dropdown menu

### Configuration

**Program.cs** is configured with:
```csharp
// Authentication and Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<AuthenticationService>();
```

## Security Features

### 1. Persistent Token Storage
- Uses `ProtectedSessionStorage` which provides:
  - Encryption of stored data
  - Per-session isolation
  - Server-side storage (Blazor Server)
  - Automatic cleanup on logout

### 2. Route Protection
- All routes protected by default via `AuthorizeRouteView`
- Unauthorized users automatically redirected to login
- Login page explicitly allows anonymous access

### 3. Token Management
- JWT tokens automatically added to API requests via ApiClient
- Token stored securely in encrypted session storage
- Token cleared on logout with proper state notification

### 4. State Synchronization
- Authentication state cascaded to all components
- State changes trigger UI updates via `NotifyAuthenticationStateChanged`
- Components can use `<AuthorizeView>` for conditional rendering

## Comparison: React vs Blazor Authentication

| Feature | React Implementation | Blazor Implementation |
|---------|---------------------|----------------------|
| **Token Storage** | localStorage (browser) | ProtectedSessionStorage (encrypted) |
| **Persistence** | Cross-browser tabs | Per-session only |
| **Security** | Client-side, plain text | Server-side, encrypted |
| **State Management** | Redux store | AuthenticationStateProvider |
| **Route Guards** | Custom HOC components | AuthorizeRouteView |
| **Login Flow** | POST → JWT → localStorage | POST → JWT → SessionStorage |
| **Logout** | Clear localStorage, navigate | Clear storage, notify state change |
| **Authorization** | Client-side permission checks | Server-side + client-side attributes |

## Usage Examples

### 1. Protecting a New Page

```razor
@page "/my-new-page"
@attribute [Authorize]
@inherits MyPageBase

<h1>My Protected Page</h1>
```

### 2. Conditional Rendering Based on Auth

```razor
<AuthorizeView>
    <Authorized>
        <p>Welcome, @context.User.Identity?.Name</p>
    </Authorized>
    <NotAuthorized>
        <p>Please log in</p>
    </NotAuthorized>
</AuthorizeView>
```

### 3. Checking Authentication in Code

```csharp
[Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }

protected override async Task OnInitializedAsync()
{
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    var isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

    if (isAuthenticated)
    {
        var username = authState.User.Identity.Name;
        // Load user-specific data
    }
}
```

### 4. Custom Logout Implementation

```csharp
[Inject] protected AuthenticationService AuthService { get; set; }

private async Task HandleLogout()
{
    await AuthService.Logout();
    // User is automatically redirected to /login
}
```

## Backend Integration

### JWT Token Generation

The backend `AdminController` at `/api/admin/jwt` endpoint:
1. Validates Basic auth credentials
2. Generates JWT token with claims:
   - `jti`: Token ID
   - `id`: User ID
   - `username`: Username
   - `admin`: IsAdmin flag
3. Returns token in response

**Sample Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### API Request Authentication

All API requests include the JWT token:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

The backend validates the token using:
- JWT Bearer middleware
- Token validation parameters (Issuer, Audience, SigningKey)
- Custom authorization filters for permission checks

## Troubleshooting

### Issue: User logged out after page refresh

**Cause**: CustomAuthenticationStateProvider not properly registered or session storage failed

**Solution**: Verify Program.cs has correct service registration:
```csharp
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
```

### Issue: Infinite redirect to login

**Cause**: Login page not marked with [AllowAnonymous]

**Solution**: Ensure Login.razor has:
```razor
@attribute [AllowAnonymous]
```

### Issue: Authentication state not updating UI

**Cause**: Not using AuthorizeView or not awaiting GetAuthenticationStateAsync

**Solution**: Use `<AuthorizeView>` component or properly await authentication state in code-behind

## Migration Notes

### Removed Components

The following components/features were replaced:
- In-memory token storage → ProtectedSessionStorage
- Manual OnAuthStateChanged event → AuthenticationStateProvider notifications
- Direct AuthService usage in layout → AuthorizeView context

### Breaking Changes

**AuthenticationService API Changes**:
- `CurrentUser` property removed → Use `AuthenticationState.User.Identity.Name`
- `IsAuthenticated` property removed → Use `AuthenticationState.User.Identity.IsAuthenticated`
- `Logout()` now returns `Task` (async)

**MainLayout Changes**:
- Must use `<AuthorizeView>` wrapper
- Access username via `context.User.Identity?.Name` instead of `AuthService.CurrentUser`

## Best Practices

1. **Always use [Authorize] attribute** on protected pages
2. **Use [AllowAnonymous]** only for login/public pages
3. **Leverage AuthorizeView** for conditional UI rendering
4. **Don't store sensitive data** in session storage (tokens are encrypted automatically)
5. **Always await** authentication state operations
6. **Use dependency injection** for AuthenticationService and AuthenticationStateProvider
7. **Handle logout properly** via AuthenticationService.Logout()

## Future Enhancements

Potential improvements for production:
1. **Token Refresh**: Implement refresh token pattern for long-lived sessions
2. **Remember Me**: Option for persistent authentication across browser sessions
3. **Role-Based UI**: Show/hide UI elements based on user roles
4. **Permission-Based Authorization**: Implement custom authorization policies
5. **Multi-Factor Authentication**: Add MFA support
6. **OAuth/OIDC**: Support external authentication providers
7. **Session Timeout Warning**: Notify users before token expiration

## References

- [ASP.NET Core Authentication](https://docs.microsoft.com/aspnet/core/security/authentication/)
- [Blazor Authentication and Authorization](https://docs.microsoft.com/aspnet/core/blazor/security/)
- [ProtectedSessionStorage](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.components.server.protectedbrowserstorage.protectedsessionstorage)
- [JWT Bearer Authentication](https://docs.microsoft.com/aspnet/core/security/authentication/jwt-authn)
