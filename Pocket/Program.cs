using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Pocket;
using Pocket.Application;
using Pocket.Application.Demo;
using Pocket.Data;
using Pocket.Infrastructure.Routing;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

var services = builder.Services;
var isDevelopment = builder.Environment.IsDevelopment();
var isDemo = builder.Environment.IsDemo();
var configuration = builder.Configuration;

services.AddSerilog(o => o
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:yyyy-MM-ddTHH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"));

var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(o => o
    .UseNpgsql(connectionString, b => b.UseNodaTime())
    .UseSnakeCaseNamingConvention()
    .EnableDetailedErrors(isDevelopment)
    .EnableSensitiveDataLogging(isDevelopment));

services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

services.AddAuthentication()
    .AddCookie(o =>
    {
        o.LoginPath = "/sign-in";
        o.LogoutPath = "/sign-out";
        o.Cookie.Name = "auth";
        o.ExpireTimeSpan = TimeSpan.FromDays(2);
    });

if (isDemo)
{
    services.AddAuthorizationBuilder()
        .AddDefaultPolicy("DefaultDemo", b => b.RequireAssertion(_ => true));

    services.AddHostedService<DemoCleanupService>();
}

services.AddRazorPages(o =>
{
    o.Conventions.Add(new PageRouteTransformerConvention(new SlugifyParameterTransformer()));
});

services.AddAntiforgery(o =>
{
    o.Cookie.Name = "antiforgery";
});

services.AddSingleton<IClock>(SystemClock.Instance);

services.AddPocket();

var app = builder.Build();

var newTokenPrincipal = configuration.GetValue<string>("NewTokenPrincipal");
if (!string.IsNullOrWhiteSpace(newTokenPrincipal))
{
    using var serviceScope = app.Services.CreateScope();
    var accessTokenHandler = serviceScope.ServiceProvider.GetRequiredService<AccessTokenHandler>();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CreateAccessToken");

    var (accessToken, accessTokenHash) = accessTokenHandler.GenerateAccessToken(newTokenPrincipal, "Cli");
    logger.LogInformation("Created new access token {AccessToken} with hash {AccessTokenHash}", accessToken, accessTokenHash);
}

if (isDevelopment)
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.MapRazorPages();

app.Run();

public partial class Program;
