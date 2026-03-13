using AgileConfig.Server.UI.Blazor.Components;
using AgileConfig.Server.UI.Blazor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add AntDesign
builder.Services.AddAntDesign();

// Add HttpContextAccessor for cookie-based auth persistence across F5 refresh
builder.Services.AddHttpContextAccessor();

// Add HTTP Client and Services
builder.Services.AddScoped<AuthHttpHandler>();
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var handler = sp.GetRequiredService<AuthHttpHandler>();
    handler.InnerHandler = new HttpClientHandler();

    var httpClient = new HttpClient(handler);
    var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
    httpClient.BaseAddress = new Uri(baseUrl);
    return httpClient;
});
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<WebSocketService>();

// Add Authentication and Authorization
// For Blazor Server with Interactive components, we need authentication services
// for the initial HTTP request handling, even though actual authentication
// is managed via AuthenticationStateProvider for Blazor components
builder.Services.AddAuthentication("Blazor.Cookie")
    .AddCookie("Blazor.Cookie", options =>
    {
        options.Cookie.Name = "Blazor.Cookie";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Authentication and Authorization middleware
// Required for Blazor Web Apps to handle [Authorize] attributes on initial page loads
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Logout: clear cookie → redirect to login (needs real HTTP request)
app.MapGet("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync("Blazor.Cookie");
    httpContext.Response.Redirect("/login");
}).AllowAnonymous();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
