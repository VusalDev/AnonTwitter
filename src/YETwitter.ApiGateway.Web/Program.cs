using CorrelationId;
using CorrelationId.DependencyInjection;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using System.Reflection;

using Yarp.ReverseProxy.Transforms;

using YETwitter.ApiGateway.Web.Configuration;
using YETwitter.Web.Common;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var host = builder.Host;
var services = builder.Services;

// logging
host.UseSerilog(configuration, $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{builder.Environment.EnvironmentName?.ToLower().Replace(".", "-")}");
services.AddLogging().AddHttpLogging(opts => { });

services.AddDefaultCorrelationId(opts =>
{
    opts.AddToLoggingScope = true;
    opts.UpdateTraceIdentifier = true;
    opts.IncludeInResponse = true;
    opts.CorrelationIdGenerator = () => Guid.NewGuid().ToString("N");
});

services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

services
    .AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("yarp"));
    //.AddTransforms(ctx =>
    //{
    //    ctx.AddRequestTransform(transform =>
    //    {
    //        string correlationIdKey = CorrelationIdOptions.DefaultHeader;
    //        string correlationId = transform.ProxyRequest.Headers.TryGetValues(correlationIdKey, out var cId) ? (string?)cId.FirstOrDefault() : Guid.NewGuid().ToString("N");

    //        //if (correlationId != null)
    //        //{
    //        //    RequestTransform.AddHeader(transform.ProxyRequest, correlationIdKey, correlationId);
    //        //}
    //        return ValueTask.CompletedTask;
    //    });
    //});

services.AddCors(options =>
{
    options.AddPolicy(CorsOptions.AppDefaultPolicy,
            builder =>
            {
                var corsOptions = new CorsOptions();
                configuration.GetSection("cors").Bind(corsOptions);
                builder.WithOrigins(corsOptions.Origins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials()
                       .WithExposedHeaders("Authorization", "WWW-Authenticate");
            });
});



var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCorrelationId();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

app.MapGet("/", () => "Proxy");
app.MapReverseProxy();

app.UseCors(CorsOptions.AppDefaultPolicy);

try
{
    Serilog.Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Serilog.Log.CloseAndFlush();
}
