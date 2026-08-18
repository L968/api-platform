using System.Security.Claims;
using System.Threading.RateLimiting;
using ApiPlatform.Gateway.Authentication;
using ApiPlatform.Gateway.Metering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;
using Yarp.ReverseProxy.Transforms;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PortalDb")
    ?? throw new InvalidOperationException("Connection string 'PortalDb' não foi configurada.");

int permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 60);
int windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
if (permitLimit <= 0 || windowSeconds <= 0)
{
    throw new InvalidOperationException("As configurações de rate limiting devem ser maiores que zero.");
}

builder.Services.AddSingleton<NpgsqlDataSource>(
    _ => NpgsqlDataSource.Create(connectionString));
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);
builder.Services.AddRequestTimeouts();
builder.Services.AddSingleton<ICredentialStore, CredentialStore>();
builder.Services.AddScoped<ApiKeyValidator>();
builder.Services.AddSingleton<UsageAggregationWorker>();
builder.Services.AddSingleton<IUsageSink>(provider => provider.GetRequiredService<UsageAggregationWorker>());
builder.Services.AddHostedService<UsageAggregationWorker>(
    provider => provider.GetRequiredService<UsageAggregationWorker>());

builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.AuthenticationScheme,
        null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("orders.read", policy => policy.RequireClaim(GatewayClaims.Scope, "orders.read"));
    options.AddPolicy("orders.write", policy => policy.RequireClaim(GatewayClaims.Scope, "orders.write"));
    options.AddPolicy("payments.read", policy => policy.RequireClaim(GatewayClaims.Scope, "payments.read"));
    options.AddPolicy("payments.write", policy => policy.RequireClaim(GatewayClaims.Scope, "payments.write"));
});

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Limite de requisições excedido." },
            cancellationToken);
    };

    options.AddPolicy("per-application", context =>
    {
        string applicationId = context.User.FindFirstValue(GatewayClaims.ApplicationId) ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(
            applicationId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context => context.AddRequestHeaderRemove(ApiKeyAuthenticationHandler.HeaderName));

WebApplication app = builder.Build();

app.UseRouting();
app.UseRequestTimeouts();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<UsageMeterMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapReverseProxy().RequireAuthorization();

app.Run();
