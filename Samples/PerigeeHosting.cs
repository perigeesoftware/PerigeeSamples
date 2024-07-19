using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Perigee
{
    public static class PerigeeHosting
    {
        /// <summary>
        /// Add a minimal API with route configuration
        /// </summary>
        /// <param name="c">Registery config</param>
        /// <param name="apiName">Name of the API</param>
        /// <param name="HTTPSPort">HTTPS Port, the HTTP Port will be port - 1</param>
        /// <param name="RouteConfig">Configure routes here</param>
        /// <param name="serviceProvider">Configure the builder and service provider</param>
        /// <param name="CORS">If true, CORS is auto added</param>
        /// <param name="Started">To start the thread automatically or not</param>
        /// <returns></returns>
        public static ThreadRegistry AddMinimalAPI(this ThreadRegistry c, string apiName, int HTTPSPort, Action<WebApplication> RouteConfig, Action<WebApplicationBuilder, IServiceCollection>? serviceProvider = null, bool CORS = true, bool Started = true)
        {
            c.Add(apiName, (ct, l) => {

                var builder = WebApplication.CreateBuilder();
                builder.Configuration["Kestrel:Endpoints:Http:Url"] = $"http://localhost:{HTTPSPort - 1}";
                builder.Configuration["Kestrel:Endpoints:Https:Url"] = $"https://localhost:{HTTPSPort}";
                Serilog.SerilogHostBuilderExtensions.UseSerilog(builder.Host, (hbc, lc) => lc.MinimumLevel.Warning());
                if (CORS) builder.Services.AddCors((c) => c.AddPolicy(c.DefaultPolicyName, (p) => p.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()));
                if (serviceProvider != null) serviceProvider?.Invoke(builder, builder.Services);
                var app = builder.Build();
                Serilog.SerilogApplicationBuilderExtensions.UseSerilogRequestLogging(app);
                RouteConfig?.Invoke(app);

                try
                {
                    app.StartAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    l.LogCritical(ex, "Failed to start Minimal API Application {name}", apiName);
                }
                while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                try
                {
                    app.StopAsync().GetAwaiter().GetResult();
                }
                catch { }

            }, started: Started);



            return c;
        }
    }
}
