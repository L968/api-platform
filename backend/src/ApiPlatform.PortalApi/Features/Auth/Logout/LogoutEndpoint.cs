using Microsoft.AspNetCore.Authentication;

namespace ApiPlatform.PortalApi.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/auth/logout", (Func<HttpContext, Task<IResult>>)Handle).RequireAuthorization();

    private static async Task<IResult> Handle(HttpContext http)
    {
        await http.SignOutAsync("PortalCookie");
        return Results.NoContent();
    }
}
