
using CorrelationId;
using CorrelationId.DependencyInjection;

using Hellang.Middleware.ProblemDetails;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

using System.Reflection;

using YETwitter.Posts.Web.Data;
using YETwitter.Posts.Web.Services;
using YETwitter.Web.Common;
using YETwitter.Web.Common.Configuration;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var host = builder.Host;
var services = builder.Services;

// logging
host.UseSerilog(configuration, $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}");
services.AddLogging().AddHttpLogging(opts => { });

services.AddDefaultCorrelationId(opts =>
{
    opts.AddToLoggingScope = true;
    opts.UpdateTraceIdentifier = true;
    opts.IncludeInResponse = true;
    opts.CorrelationIdGenerator = () => Guid.NewGuid().ToString("N");
});

services.AddProblemDetails(opts =>
{
#if DEBUG
    // Control when an exception is included
    opts.IncludeExceptionDetails = (ctx, ex) => true;
    opts.ShouldLogUnhandledException = (ctx, ex, arg) => true;
#endif
});

// For Entity Framework
services
    .AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("PostsDb"),
    sqlServerOptionsAction: sqlOptions =>
    {
        //sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }));

// Adding Authentication
services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtOptions = new JwtOptions();
        configuration.GetSection("JWT").Bind(jwtOptions);

        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtOptions.ValidAudience,
            ValidIssuer = jwtOptions.ValidIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });
services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(configuration.GetConnectionString("PostsDb"),
        name: "PostsDb-check",
        tags: new string[] { "PostsDb" });

services.Configure<PostServiceOptions>(configuration.GetSection("Post"));

services.AddScoped<IPostService, PostsService>();
services.AddScoped<IDatabaseService, DatabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();

app.UseCorrelationId();

app.UseProblemDetails();

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthentication().UseAuthorization();

app.MapControllers();

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

// unit testing
public partial class Program { }