using System.Security.Claims;
using ApiPlatform.PortalApi.Security;

namespace ApiPlatform.PortalApi.Features.Shared;

internal static class EndpointHelpers
{
    public static Guid OrganizationId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(PortalClaims.OrganizationId)!);
}
