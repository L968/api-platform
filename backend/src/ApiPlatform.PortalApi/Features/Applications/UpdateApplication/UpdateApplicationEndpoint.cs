using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Applications.UpdateApplication;

public static class UpdateApplicationEndpoint
{
    public static void MapUpdateApplicationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/applications/{id:guid}", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, Guid id, UpdateApplicationRequest request, PortalDbContext db)
    {
        Application? application = await db.Applications.SingleOrDefaultAsync(a => a.Id == id && a.OrganizationId == EndpointHelpers.OrganizationId(principal));
        if (application is null || string.IsNullOrWhiteSpace(request.Name) || !Enum.IsDefined(request.Type))
        {
            return Results.BadRequest();
        }

        application.Rename(request.Name.Trim());
        application.ChangeType(request.Type);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
