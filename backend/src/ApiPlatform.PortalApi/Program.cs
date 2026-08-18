using ApiPlatform.PortalApi.Features.Applications.CreateApplication;
using ApiPlatform.PortalApi.Features.Applications.DisableApplication;
using ApiPlatform.PortalApi.Features.Applications.GetApplication;
using ApiPlatform.PortalApi.Features.Applications.ListApplications;
using ApiPlatform.PortalApi.Features.Applications.ReactivateApplication;
using ApiPlatform.PortalApi.Features.Applications.UpdateApplication;
using ApiPlatform.PortalApi.Features.Auth.GetCurrentUser;
using ApiPlatform.PortalApi.Features.Auth.Login;
using ApiPlatform.PortalApi.Features.Auth.Logout;
using ApiPlatform.PortalApi.Features.Credentials.CreateCredential;
using ApiPlatform.PortalApi.Features.Credentials.ListCredentials;
using ApiPlatform.PortalApi.Features.Credentials.ListScopes;
using ApiPlatform.PortalApi.Features.Credentials.RevokeCredential;
using ApiPlatform.PortalApi.Features.Organizations.GetOrganization;
using ApiPlatform.PortalApi.Features.Organizations.UpdateOrganization;
using ApiPlatform.PortalApi.Features.Usage.GetBilling;
using ApiPlatform.PortalApi.Features.Usage.GetUsage;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using ApiPlatform.PortalApi.Security;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PortalDb")
    ?? throw new InvalidOperationException("Connection string 'PortalDb' não foi configurada.");

builder.Services.AddDbContext<PortalDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddAuthentication("PortalCookie")
    .AddCookie("PortalCookie", options =>
    {
        options.Cookie.Name = "api_platform_portal";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

const string portalCorsPolicy = "PortalCorsPolicy";
builder.Services.AddCors(options => options.AddPolicy(portalCorsPolicy, policy =>
    policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:Initialize"))
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    PortalDbContext db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
    await db.Database.MigrateAsync();

    string? seedFile = builder.Configuration["Database:SeedFile"];
    if (!string.IsNullOrWhiteSpace(seedFile) && File.Exists(seedFile))
    {
        string seedSql = await File.ReadAllTextAsync(seedFile);
        await db.Database.ExecuteSqlRawAsync(seedSql);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(portalCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapLoginEndpoint();
app.MapLogoutEndpoint();
app.MapGetCurrentUserEndpoint();
app.MapGetOrganizationEndpoint();
app.MapUpdateOrganizationEndpoint();
app.MapListApplicationsEndpoint();
app.MapCreateApplicationEndpoint();
app.MapGetApplicationEndpoint();
app.MapUpdateApplicationEndpoint();
app.MapDisableApplicationEndpoint();
app.MapReactivateApplicationEndpoint();
app.MapListScopesEndpoint();
app.MapListCredentialsEndpoint();
app.MapCreateCredentialEndpoint();
app.MapRevokeCredentialEndpoint();
app.MapGetUsageEndpoint();
app.MapGetBillingEndpoint();

await app.RunAsync();
