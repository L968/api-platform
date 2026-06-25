using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PortalDb")
    ?? throw new InvalidOperationException("Connection string 'PortalDb' não foi configurada.");

builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS — o Developer Portal (Next.js) é o único consumidor desta API.
// Ajustar a origin conforme o endereço real do front em desenvolvimento.
const string PortalCorsPolicy = "PortalCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(PortalCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(PortalCorsPolicy);

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
