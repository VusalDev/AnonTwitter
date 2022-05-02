
using CorrelationId;
using CorrelationId.DependencyInjection;

using Hellang.Middleware.ProblemDetails;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

using System.Reflection;

using YETwitter.Identity.Web.Configuration;
using YETwitter.Identity.Web.Data;
using YETwitter.Web.Common;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var host = builder.Host;
var services = builder.Services;

// Add services to the container.
var jwtSection = configuration.GetSection("JWT");
services.AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

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
    options.UseSqlServer(configuration.GetConnectionString("IdentityDb"),
    sqlServerOptionsAction: sqlOptions =>
    {
        //sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }));

// For Identity
services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
        var jwtOptions = jwtSection.Get<JwtOptions>();
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
services.AddControllers();

services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(configuration.GetConnectionString("IdentityDb"),
        name: "IdentityDB-check",
        tags: new string[] { "IdentityDB" });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
//services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCorrelationId();

app.UseProblemDetails();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

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