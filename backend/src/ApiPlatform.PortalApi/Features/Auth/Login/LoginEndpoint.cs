using System.Security.Claims;
using ApiPlatform.PortalApi.Domain.Entities;
using ApiPlatform.PortalApi.Infrastructure.Persistence;
using ApiPlatform.PortalApi.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace ApiPlatform.PortalApi.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/auth/login", Handle).AllowAnonymous();

    private static async Task<IResult> Handle(LoginRequest request, PortalDbContext db, PasswordHasher hasher, HttpContext http)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        PortalUser? user = await db.PortalUsers.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || user.Status != PortalUserStatus.Active || !hasher.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        Organization? organization = await db.Organizations.FindAsync(user.OrganizationId);
        if (organization is null || organization.Status != OrganizationStatus.Active)
        {
            return Results.Unauthorized();
        }

        Claim[] claims = [new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email), new(PortalClaims.OrganizationId, user.OrganizationId.ToString())];
        await http.SignInAsync("PortalCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "PortalCookie")));
        LoginResponse response = new(
            user.Id,
            user.Email,
            organization.Id,
            organization.Name);

        return Results.Ok(response);
    }
}
