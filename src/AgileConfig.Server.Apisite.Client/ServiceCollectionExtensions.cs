using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Apisite.Client;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册所有 Apisite 类型化客户端。
    /// 调用方需先在 DI 中注册 AuthenticationStateProvider。
    /// </summary>
    public static IServiceCollection AddAgileConfigApiClients(
        this IServiceCollection services,
        string? baseUrl = null)
    {
        // BearerTokenHandler 负责从当前 HttpContext.User 读取 JWT 并注入 Authorization 头
        services.AddTransient<BearerTokenHandler>();

        void Configure(IServiceProvider sp, HttpClient client)
        {
            var url = baseUrl
                ?? sp.GetRequiredService<IConfiguration>()["ApiBaseUrl"]
                ?? "http://localhost:5000";
            client.BaseAddress = new Uri(url);
        }

        services.AddHttpClient<AdminApiClient>(Configure);

        services.AddHttpClient<AppApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<UserApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<NodeApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<ConfigApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<ServiceApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<SysLogApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient<ReportApiClient>(Configure)
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
