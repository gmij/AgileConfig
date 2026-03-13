using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace AgileConfig.Server.UI.Blazor.Components.Pages;

public class LoginBase : ComponentBase
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "error")]
    public string? ErrorCode { get; set; }

    protected string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(ErrorCode))
        {
            errorMessage = "Invalid username or password";
        }

        if (HttpMethods.IsPost(HttpContext.Request.Method))
        {
            await HandleLoginAsync();
        }
    }

    private async Task HandleLoginAsync()
    {
        var form = await HttpContext.Request.ReadFormAsync();
        var username = form["username"].ToString();
        var password = form["password"].ToString();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorMessage = "Username and password are required";
            return;
        }

        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5000";
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var json = System.Text.Json.JsonSerializer.Serialize(new { userName = username, password });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("/admin/jwt/login", content);

            if (!response.IsSuccessStatusCode)
            {
                errorMessage = "Invalid username or password";
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<JwtLoginResponse>();
            if (result?.Status != "ok" || string.IsNullOrEmpty(result.Token))
            {
                errorMessage = "Invalid username or password";
                return;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new("token", result.Token)
            };
            var identity = new ClaimsIdentity(claims, "Blazor.Cookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Blazor.Cookie", principal);
            HttpContext.Response.Redirect("/");
        }
        catch
        {
            errorMessage = "Login failed, please try again";
        }
    }

    private record JwtLoginResponse(string? Status, string? Token, string? Type);
}
