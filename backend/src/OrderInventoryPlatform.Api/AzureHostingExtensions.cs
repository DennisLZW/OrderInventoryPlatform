using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OrderInventoryPlatform.Api;

/// <summary>
/// Azure App Service / Azure Container Apps (and similar reverse proxies) terminate TLS
/// and forward HTTP to the app. Forwarded headers let ASP.NET Core see the original scheme/host.
/// </summary>
public static class AzureHostingExtensions
{
    public static bool IsRunningOnAzureAppService() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    public static bool IsRunningOnAzureContainerApps() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"));

    public static bool ShouldUseForwardedHeaders(IHostEnvironment env, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Azure:UseForwardedHeaders"))
        {
            return true;
        }

        // Auto-detect common Azure PaaS environments
        if (IsRunningOnAzureAppService() || IsRunningOnAzureContainerApps())
        {
            return true;
        }

        return false;
    }

    public static WebApplicationBuilder AddAzureForwardedHeaders(this WebApplicationBuilder builder)
    {
        if (!ShouldUseForwardedHeaders(builder.Environment, builder.Configuration))
        {
            return builder;
        }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Trust the Azure edge / platform proxy
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return builder;
    }

    public static WebApplication UseAzureForwardedHeaders(this WebApplication app)
    {
        if (!ShouldUseForwardedHeaders(app.Environment, app.Configuration))
        {
            return app;
        }

        app.UseForwardedHeaders();
        return app;
    }
}
