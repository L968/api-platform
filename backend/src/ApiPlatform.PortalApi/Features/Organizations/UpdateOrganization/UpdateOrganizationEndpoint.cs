using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;

namespace ApiPlatform.PortalApi.Features.Organizations.UpdateOrganization;

public static class UpdateOrganizationEndpoint
{
    public static void MapUpdateOrganizationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/organization", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, UpdateOrganizationRequest request, PortalDbContext db)
    {
        Organization? organization = await db.Organizations.FindAsync(EndpointHelpers.OrganizationId(principal));

        if (organization is null || string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest();
        }

        organization.Rename(request.Name.Trim());
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
