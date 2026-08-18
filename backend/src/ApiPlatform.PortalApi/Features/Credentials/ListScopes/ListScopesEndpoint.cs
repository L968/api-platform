using ApiPlatform.PortalApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Credentials.ListScopes;

public static class ListScopesEndpoint
{
    public static void MapListScopesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/scopes", Handle).RequireAuthorization();

    private static async Task<IResult> Handle(PortalDbContext db)
    {
        List<ListScopesResponse> scopes = await db.Scopes
            .OrderBy(scope => scope.Name)
            .Select(scope => new ListScopesResponse(
                scope.Id,
                scope.Name))
            .ToListAsync();

        return Results.Ok(scopes);
    }
}
