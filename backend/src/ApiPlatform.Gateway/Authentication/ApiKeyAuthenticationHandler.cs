using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ApiPlatform.Gateway.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly ApiKeyValidator _validator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ApiKeyValidator validator)
        : base(options, logger, encoder)
    {
        _validator = validator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out StringValues values) ||
            values.Count != 1 ||
            string.IsNullOrWhiteSpace(values[0]))
        {
            return AuthenticateResult.NoResult();
        }

        ApplicationIdentity? application = await _validator.ValidateAsync(
            values[0]!,
            Context.RequestAborted);

        if (application is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        List<Claim> claims =
        [
            new Claim(GatewayClaims.OrganizationId, application.OrganizationId.ToString()),
            new Claim(GatewayClaims.ApplicationId, application.ApplicationId.ToString()),
            new Claim(GatewayClaims.CredentialId, application.CredentialId.ToString())
        ];

        claims.AddRange(application.Scopes.Select(scope => new Claim(GatewayClaims.Scope, scope)));

        ClaimsIdentity identity = new(claims, AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, AuthenticationScheme);
        return AuthenticateResult.Success(ticket);
    }
}
