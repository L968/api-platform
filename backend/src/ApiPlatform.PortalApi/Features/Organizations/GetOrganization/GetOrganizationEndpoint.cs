using System.Security.Claims;
using ApiPlatform.PortalApi.Features.Shared;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Organizations.GetOrganization;

public static class GetOrganizationEndpoint
{
    public static void MapGetOrganizationEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/organization", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, PortalDbContext db)
    {
        Guid id = EndpointHelpers.OrganizationId(principal);

        GetOrganizationResponse? organization = await db.Organizations
            .Where(organization => organization.Id == id)
            .Select(organization => new GetOrganizationResponse(
                organization.Id,
                organization.Name,
                organization.Status,
                organization.CreatedAt))
            .SingleOrDefaultAsync();

        return organization is null
            ? Results.NotFound()
            : Results.Ok(organization);
    }
}
