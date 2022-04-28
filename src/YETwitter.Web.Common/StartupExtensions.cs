
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace YETwitter.Web.Common
{
    public static class StartupExtensions
    {
        public static IHostBuilder UseSerilog(this IHostBuilder host, ConfigurationManager configuration, string indexFormat)
        {
            return host.UseSerilog((ctx, cfg) =>
            {
                cfg.Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithCorrelationId()
                .WriteTo.Debug()
                .WriteTo.Console();
                if (configuration["ElasticConfiguration:Uri"] != null)
                {
                    cfg.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = indexFormat
                    });
                }
                cfg.ReadFrom.Configuration(configuration);
            });
        }

        public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
        {
            return app.UseSerilogRequestLogging(options =>
            {
                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });
        }
    }
}
