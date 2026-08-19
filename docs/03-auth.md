# Authentication and authorization

## API key format

Business API clients send one header:

```http
X-Api-Key: app_<client-id>.sk_<secret>
```

The public client ID allows an indexed Credential lookup. The secret is verified against its PBKDF2 hash using constant-time comparison.

## Validation flow

For each uncached key, the Gateway:

1. Parses the client ID and secret.
2. Loads the Credential, Application, Organization and Scopes.
3. Verifies the secret hash.
4. Rejects expired or revoked Credentials.
5. Rejects inactive Applications or Organizations.
6. Creates claims for the Credential, Application, Organization and Scopes.

YARP routes use authorization policies such as `orders.read`, `orders.write`, `payments.read` and `payments.write`.

## Cache

Valid results are cached in memory for 30 seconds and invalid results for 5 seconds. The cache key is a SHA-256 digest of the complete API key, so the plaintext secret is not stored in cache.

Revocation or Application deactivation can therefore take up to the valid cache TTL to affect an already cached key. This trade-off avoids a database lookup on every request in the current single-Gateway deployment.

## Portal session

Portal Users authenticate separately with email and password. The Portal API creates an HttpOnly cookie with a sliding expiration. Organization ID is taken from the authenticated session rather than from request input, enforcing tenant isolation.

Previous: [Domain model](./02-domain-model.md) · Next: [Metering and billing](./04-telemetry.md)
