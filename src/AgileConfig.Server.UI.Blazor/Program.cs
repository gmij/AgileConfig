using AgileConfig.Server.UI.Blazor.Components;
using AgileConfig.Server.UI.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add AntDesign
builder.Services.AddAntDesign();

// Add HTTP Client and Services
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClient = new HttpClient();
    var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
    httpClient.BaseAddress = new Uri(baseUrl);
    return httpClient;
});
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<WebSocketService>();

// Add Authentication and Authorization
// For Blazor Web Apps with Interactive Server, we need authentication services
// for the initial HTTP request handling, even though actual authentication
// is managed via AuthenticationStateProvider for Blazor components
builder.Services.AddAuthentication()
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
builder.Services.AddScoped<AuthenticationService>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
