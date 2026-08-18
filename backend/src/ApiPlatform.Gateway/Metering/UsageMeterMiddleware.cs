using System.Diagnostics;
using System.Security.Claims;
using ApiPlatform.Gateway.Authentication;
using Yarp.ReverseProxy.Model;

namespace ApiPlatform.Gateway.Metering;

public sealed class UsageMeterMiddleware
{
    private const string ApiIdMetadata = "ApiId";
    private readonly RequestDelegate _next;

    public UsageMeterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUsageSink sink)
    {
        if (!TryGetIdentity(context, out MeteringIdentity identity))
        {
            await _next(context);
            return;
        }

        long startedAt = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            if (TryGetApiId(context, out Guid apiId))
            {
                int latencyMs = (int)Math.Ceiling(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
                sink.TryWrite(new UsageEvent(
                    identity.OrganizationId,
                    identity.ApplicationId,
                    apiId,
                    NormalizeEndpoint(context.Request),
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    context.Response.StatusCode >= StatusCodes.Status400BadRequest,
                    latencyMs));
            }
        }
    }

    public static string NormalizeEndpoint(HttpRequest request)
    {
        string[] segments = request.Path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];

        string normalizedPath = segments.Length switch
        {
            0 => "/",
            1 => $"/{segments[0].ToLowerInvariant()}",
            _ => $"/{segments[0].ToLowerInvariant()}/{{id}}"
        };

        return $"{request.Method} {normalizedPath}";
    }

    private static bool TryGetIdentity(
        HttpContext context,
        out MeteringIdentity identity)
    {
        identity = null!;
        string? organizationClaim = context.User.FindFirstValue(GatewayClaims.OrganizationId);
        string? applicationClaim = context.User.FindFirstValue(GatewayClaims.ApplicationId);

        if (!Guid.TryParse(organizationClaim, out Guid organizationId) ||
            !Guid.TryParse(applicationClaim, out Guid applicationId))
        {
            return false;
        }

        identity = new MeteringIdentity(organizationId, applicationId);
        return true;
    }

    private static bool TryGetApiId(HttpContext context, out Guid apiId)
    {
        apiId = default;
        IReverseProxyFeature? proxyFeature = context.Features.Get<IReverseProxyFeature>();
        IReadOnlyDictionary<string, string>? metadata = proxyFeature?.Route.Config.Metadata;

        return metadata is not null &&
            metadata.TryGetValue(ApiIdMetadata, out string? apiIdValue) &&
            Guid.TryParse(apiIdValue, out apiId);
    }

    private sealed record MeteringIdentity(
        Guid OrganizationId,
        Guid ApplicationId);
}

public interface IUsageSink
{
    bool TryWrite(UsageEvent usageEvent);
}

public sealed record UsageEvent(
    Guid OrganizationId,
    Guid ApplicationId,
    Guid ApiId,
    string Endpoint,
    DateOnly Date,
    bool IsError,
    int LatencyMs);
