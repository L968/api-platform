using System.Security.Claims;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Auth.GetCurrentUser;

public static class GetCurrentUserEndpoint
{
    public static void MapGetCurrentUserEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/me", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(ClaimsPrincipal principal, PortalDbContext db)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
        {
            return Results.Unauthorized();
        }

        GetCurrentUserResponse? result = await db.PortalUsers
            .Where(user => user.Id == userId)
            .Select(user => new GetCurrentUserResponse(
                user.Id,
                user.Email,
                user.Status,
                new GetCurrentUserOrganizationResponse(
                    user.Organization!.Id,
                    user.Organization.Name,
                    user.Organization.Status)))
            .SingleOrDefaultAsync();

        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
