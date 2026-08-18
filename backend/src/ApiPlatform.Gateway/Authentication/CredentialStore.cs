using Npgsql;

namespace ApiPlatform.Gateway.Authentication;

public interface ICredentialStore
{
    Task<CredentialData?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken);
}

public sealed class CredentialStore : ICredentialStore
{
    private const string FindCredentialSql = """
        SELECT
            credential.id,
            credential.organization_id,
            credential.application_id,
            credential.secret_hash,
            credential.expires_at,
            credential.revoked_at,
            application.is_active,
            organization.status,
            COALESCE(
                array_agg(scope.name) FILTER (WHERE scope.name IS NOT NULL),
                ARRAY[]::character varying[])
        FROM credential
        INNER JOIN application
            ON application.id = credential.application_id
            AND application.organization_id = credential.organization_id
        INNER JOIN organization
            ON organization.id = credential.organization_id
        LEFT JOIN credential_scope
            ON credential_scope.credential_id = credential.id
        LEFT JOIN scope
            ON scope.id = credential_scope.scope_id
        WHERE credential.client_id = $1
        GROUP BY
            credential.id,
            application.is_active,
            organization.status;
        """;

    private readonly NpgsqlDataSource _dataSource;

    public CredentialStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<CredentialData?> FindByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlCommand command = _dataSource.CreateCommand(FindCredentialSql);
        command.Parameters.AddWithValue(clientId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        DateTime? expiresAt = await reader.IsDBNullAsync(4, cancellationToken)
            ? null
            : await reader.GetFieldValueAsync<DateTime>(4, cancellationToken);
        DateTime? revokedAt = await reader.IsDBNullAsync(5, cancellationToken)
            ? null
            : await reader.GetFieldValueAsync<DateTime>(5, cancellationToken);
        string[] scopes = await reader.GetFieldValueAsync<string[]>(8, cancellationToken);

        return new CredentialData(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetGuid(2),
            reader.GetString(3),
            expiresAt,
            revokedAt,
            reader.GetBoolean(6),
            reader.GetString(7),
            scopes);
    }
}

public sealed record CredentialData(
    Guid CredentialId,
    Guid OrganizationId,
    Guid ApplicationId,
    string SecretHash,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    bool ApplicationIsActive,
    string OrganizationStatus,
    IReadOnlyCollection<string> Scopes);
