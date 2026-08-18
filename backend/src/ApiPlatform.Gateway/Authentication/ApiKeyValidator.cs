using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace ApiPlatform.Gateway.Authentication;

public sealed class ApiKeyValidator
{
    private const char Separator = '.';
    private const int MaximumPartLength = 100;

    private readonly ICredentialStore _credentialStore;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _validCacheDuration;
    private readonly TimeSpan _invalidCacheDuration;

    public ApiKeyValidator(
        ICredentialStore credentialStore,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _credentialStore = credentialStore;
        _cache = cache;
        _validCacheDuration = CacheDuration(configuration, "ApiKeyCache:ValidForSeconds", 30);
        _invalidCacheDuration = CacheDuration(configuration, "ApiKeyCache:InvalidForSeconds", 5);
    }

    public async Task<ApplicationIdentity?> ValidateAsync(
        string apiKey,
        CancellationToken cancellationToken)
    {
        string cacheKey = CreateCacheKey(apiKey);
        if (_cache.TryGetValue(cacheKey, out CacheEntry? cached))
        {
            return cached?.Identity;
        }

        ApiKeyParts? parts = ParseApiKey(apiKey);
        if (parts is null)
        {
            CacheInvalid(cacheKey);
            return null;
        }

        CredentialData? credential = await _credentialStore.FindByClientIdAsync(
            parts.ClientId,
            cancellationToken);

        if (!IsValid(credential, parts.Secret))
        {
            CacheInvalid(cacheKey);
            return null;
        }

        ApplicationIdentity identity = new(
            credential!.OrganizationId,
            credential.ApplicationId,
            credential.CredentialId,
            credential.Scopes);

        CacheValid(cacheKey, identity, credential.ExpiresAt);
        return identity;
    }

    public static ApiKeyParts? ParseApiKey(string apiKey)
    {
        int separatorIndex = apiKey.IndexOf(Separator);
        if (separatorIndex <= 0 || separatorIndex != apiKey.LastIndexOf(Separator))
        {
            return null;
        }

        string clientId = apiKey[..separatorIndex];
        string secret = apiKey[(separatorIndex + 1)..];

        if (clientId.Length > MaximumPartLength ||
            secret.Length > MaximumPartLength ||
            !clientId.StartsWith("app_", StringComparison.Ordinal) ||
            !secret.StartsWith("sk_", StringComparison.Ordinal))
        {
            return null;
        }

        return new ApiKeyParts(clientId, secret);
    }

    public static bool VerifySecret(string secret, string encodedHash)
    {
        string[] parts = encodedHash.Split('$');
        if (parts.Length != 4 ||
            parts[0] != "pbkdf2-sha256" ||
            !int.TryParse(parts[1], out int iterations))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                secret,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsValid(CredentialData? credential, string secret)
    {
        if (credential is null ||
            credential.RevokedAt is not null ||
            credential.ExpiresAt <= DateTime.UtcNow ||
            !credential.ApplicationIsActive ||
            credential.OrganizationStatus != "Active")
        {
            return false;
        }

        return VerifySecret(secret, credential.SecretHash);
    }

    private void CacheValid(
        string cacheKey,
        ApplicationIdentity identity,
        DateTime? credentialExpiration)
    {
        DateTimeOffset cacheExpiration = DateTimeOffset.UtcNow.Add(_validCacheDuration);
        if (credentialExpiration is not null && credentialExpiration < cacheExpiration)
        {
            cacheExpiration = credentialExpiration.Value;
        }

        _cache.Set(
            cacheKey,
            new CacheEntry(identity),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = cacheExpiration,
                Size = 1
            });
    }

    private void CacheInvalid(string cacheKey)
    {
        _cache.Set(
            cacheKey,
            new CacheEntry(null),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _invalidCacheDuration,
                Size = 1
            });
    }

    private static TimeSpan CacheDuration(
        IConfiguration configuration,
        string key,
        int defaultSeconds)
    {
        int seconds = configuration.GetValue(key, defaultSeconds);
        if (seconds <= 0)
        {
            throw new InvalidOperationException($"{key} deve ser maior que zero.");
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private static string CreateCacheKey(string apiKey)
    {
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(digest);
    }

    private sealed record CacheEntry(ApplicationIdentity? Identity);
}

public sealed record ApiKeyParts(string ClientId, string Secret);

public sealed record ApplicationIdentity(
    Guid OrganizationId,
    Guid ApplicationId,
    Guid CredentialId,
    IReadOnlyCollection<string> Scopes);
