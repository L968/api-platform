using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.CreateApplication;

public static class CreateApplicationEndpoint
{
    public static void MapCreateApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/applications", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, CreateApplicationRequest request, PortalDbContext db)
    {
        Guid organizationId = EndpointHelpers.OrganizationId(principal);
        if (string.IsNullOrWhiteSpace(request.Name) || !Enum.IsDefined(request.Type))
        {
            return Results.BadRequest();
        }

        if (!await db.Organizations.AnyAsync(o => o.Id == organizationId && o.Status == OrganizationStatus.Active))
        {
            return Results.NotFound();
        }

        Application application = new(organizationId, request.Name.Trim(), request.Type);
        db.Applications.Add(application);
        await db.SaveChangesAsync();
        CreateApplicationResponse response = new(
            application.Id,
            application.Name,
            application.Type,
            application.IsActive);

        return Results.Created($"/applications/{application.Id}", response);
    }
}
