# AgileConfig Blazor UI Setup Guide

This document explains how to run the AgileConfig system with the new Blazor UI.

## Architecture Overview

The AgileConfig system consists of two applications:

1. **API Site** (`AgileConfig.Server.Apisite`) - Port 5000
   - Backend APIs and services
   - Legacy React/Vue UI (served from `/ui`)
   - JWT authentication

2. **Blazor UI** (`AgileConfig.Server.UI.Blazor`) - Port 7184 (HTTPS) / 5113 (HTTP)
   - New Blazor Server UI
   - Makes API calls to the API Site
   - Component-level authentication using `AuthenticationStateProvider`

## Running Both Applications

### Option 1: Run Separately (Development)

**Terminal 1 - Start API Site:**
```bash
cd src/AgileConfig.Server.Apisite
dotnet run
```
The API site will start on `http://localhost:5000`

**Terminal 2 - Start Blazor UI:**
```bash
cd src/AgileConfig.Server.UI.Blazor
dotnet run
```
The Blazor UI will start on:
- HTTPS: `https://localhost:7184`
- HTTP: `http://localhost:5113`

### Option 2: Configure API Site to Redirect to Blazor UI

Edit `src/AgileConfig.Server.Apisite/appsettings.json` and set the `blazorUiUrl`:

```json
{
  "adminConsole": true,
  "blazorUiUrl": "https://localhost:7184",
  ...
}
```

Now when you access `http://localhost:5000`, it will automatically redirect to the Blazor UI.

## Configuration

### API Site Configuration

**File:** `src/AgileConfig.Server.Apisite/appsettings.json`

Key settings:
- `urls`: "http://*:5000" - API server port
- `adminConsole`: true - Enable admin console mode
- `blazorUiUrl`: "" - Set to Blazor UI URL to enable automatic redirects (e.g., "https://localhost:7184")

### Blazor UI Configuration

**File:** `src/AgileConfig.Server.UI.Blazor/appsettings.json`

Key settings:
- `ApiBaseUrl`: "http://localhost:5000" - API server URL (can be configured, defaults to localhost:5000)

The `ApiBaseUrl` is read in `Program.cs` when configuring the `HttpClient`:
```csharp
var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
httpClient.BaseAddress = new Uri(baseUrl);
```

## Initial Setup Flow

When running for the first time:

1. Start both applications (API Site and Blazor UI)
2. Access the Blazor UI at `https://localhost:7184` or `http://localhost:5113`
3. You'll be redirected to `/initpassword` page (if no super admin exists)
4. Set the initial administrator password
5. You'll be redirected to `/login`
6. Login with username `admin` and your password
7. Access the dashboard at `/`

## Authentication Flow

1. **Login**: User enters credentials on Blazor UI login page
2. **API Call**: Blazor UI calls `POST /api/admin/jwt` on API Site with Basic auth
3. **JWT Token**: API Site validates credentials and returns JWT token
4. **Token Storage**: Blazor UI stores token in encrypted `ProtectedSessionStorage`
5. **API Requests**: Subsequent API calls include `Authorization: Bearer {token}` header

## Troubleshooting

### Issue: IAuthenticationService Error

**Error:**
```
InvalidOperationException: Unable to find the required 'IAuthenticationService' service.
```

**Solution:**
This error occurs if `UseAuthentication()` and `UseAuthorization()` middleware are present in `Program.cs` without calling `AddAuthentication()`. For Blazor Server applications, these middleware calls are not needed because authentication is handled at the component level via `AuthenticationStateProvider`.

**Verify:** Check `src/AgileConfig.Server.UI.Blazor/Program.cs` - it should NOT have:
```csharp
app.UseAuthentication();  // Remove this
app.UseAuthorization();   // Remove this
```

### Issue: 404 Error on /initpassword

**Error:**
```
http://localhost:5000/initpassword returns 404
```

**Solution:**
The `/initpassword` route only exists in the Blazor UI application, not in the API Site. You have two options:

1. **Access Blazor UI directly**: Navigate to `https://localhost:7184/initpassword`
2. **Configure redirect**: Set `blazorUiUrl` in API Site's `appsettings.json`:
   ```json
   "blazorUiUrl": "https://localhost:7184"
   ```

### Issue: HttpClient Not Configured

**Error:**
```
HttpClient making requests to relative URLs fails
```

**Solution:**
Ensure `Program.cs` sets the `BaseAddress`:
```csharp
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClient = new HttpClient();
    var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
    httpClient.BaseAddress = new Uri(baseUrl);
    return httpClient;
});
```

## Production Deployment

For production deployment:

1. Build both applications
2. Deploy API Site to your server (e.g., `https://api.yourdomain.com`)
3. Deploy Blazor UI to your server (e.g., `https://admin.yourdomain.com`)
4. Configure Blazor UI's `appsettings.json`:
   ```json
   {
     "ApiBaseUrl": "https://api.yourdomain.com"
   }
   ```
5. Configure API Site's `appsettings.json`:
   ```json
   {
     "blazorUiUrl": "https://admin.yourdomain.com"
   }
   ```

## Key Files

### Blazor UI
- `Program.cs` - Application startup and service configuration
- `Services/ApiClient.cs` - HTTP client wrapper for API calls
- `Services/CustomAuthenticationStateProvider.cs` - Authentication state management
- `Services/AuthenticationService.cs` - Login/logout logic
- `Components/Pages/InitPassword.razor` - Initial password setup page
- `Components/Pages/Login.razor` - Login page
- `Components/Routes.razor` - Routing with authentication

### API Site
- `Startup.cs` - Application startup and middleware configuration
- `Controllers/HomeController.cs` - Handles redirects
- `Controllers/AdminController.cs` - Admin APIs including `/api/admin/jwt` and `/admin/InitPassword`
- `Appsettings.cs` - Configuration helper
- `appsettings.json` - Application configuration

## API Endpoints

### Authentication
- `POST /api/admin/jwt` - Login and get JWT token (Basic auth)
- `POST /admin/InitPassword` - Set initial password

### System
- `GET /` - Root endpoint (redirects based on configuration)
- `GET /home/sys` - System information
- `GET /home/echo` - Health check endpoint

## Notes

- The Blazor UI uses Server-side rendering with SignalR for interactivity
- Authentication state is persisted using `ProtectedSessionStorage` (encrypted, server-side)
- The legacy React/Vue UI can still be accessed at `http://localhost:5000/ui` if needed
- Both UIs can coexist - set `blazorUiUrl` to empty string to use legacy UI
