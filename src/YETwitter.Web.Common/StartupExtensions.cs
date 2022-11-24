
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

using YETwitter.Web.Common.Configuration;

namespace YETwitter.Web.Common
{
    public static class StartupExtensions
    {
        public static IHostBuilder UseSerilog(this IHostBuilder host, ConfigurationManager configuration, string indexFormat)
        {
            var elkOpts = new ElasticsearchOptions();
            configuration.GetSection("ElasticConfiguration").Bind(elkOpts);
            return host.UseSerilog((ctx, cfg) =>
            {
                cfg
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithCorrelationId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Debug()
                .WriteTo.Console();
                if (elkOpts.Uri != null)
                {
                    cfg.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elkOpts.Uri)
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
