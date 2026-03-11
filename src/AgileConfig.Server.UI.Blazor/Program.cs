using AgileConfig.Server.UI.Blazor.Components;
using AgileConfig.Server.UI.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add AntDesign
builder.Services.AddAntDesign();

// Add HTTP Client and Services
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient();
    var config = sp.GetRequiredService<IConfiguration>();
    return httpClient;
});
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<WebSocketService>();
builder.Services.AddScoped<AuthenticationService>();

// Add session for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseSession();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
