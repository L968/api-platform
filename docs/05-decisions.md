# Architecture decisions

## Centralize business API policies in the Gateway

API key authentication, scopes, rate limiting and usage metering are cross-cutting platform concerns. Keeping them in the Gateway prevents each business API from implementing a different version of the same rules.

Orders and Payments are reachable only inside the Docker network, so they can trust requests forwarded by the Gateway. If those APIs are exposed independently later, that trust boundary must be redesigned.

## Keep Portal authentication separate

Portal Users are humans managing an Organization. Applications are machine clients calling business APIs. Their credentials, lifetimes and authorization models are different, so the two authentication flows remain independent.

## Allow multiple keys per Application

Multiple Credentials support zero-downtime rotation, separate grants for different modules and selective revocation after a leak. Disabling an Application acts as a reversible kill switch for all its keys; revoking a Credential is permanent and affects only that key.

## Meter outside the request path

Billing data does not need to be committed before returning an API response. A bounded in-memory channel keeps PostgreSQL writes away from the hot path while applying an explicit memory limit.

The current design accepts that an abrupt Gateway process failure can lose buffered events. Introducing a durable broker is deferred until deployment requirements justify the operational cost.

## Store daily aggregates in PostgreSQL

Per-request logs are unnecessarily large for customer billing queries. Daily aggregates provide stable, tenant-filtered records for dashboards and invoices. Operational traces and metrics can be added independently; they do not replace durable billing data.

## Version pricing and snapshot invoices

Pricing records have effective dates because commercial rates change over time. Issued invoice lines also store their rate and amount, making invoices auditable and independent of future pricing changes.

## Keep sample APIs small

Orders and Payments exist to exercise the platform boundary. Adding persistence and domain logic to them would not improve the API management flow. They can be replaced by real services without changing the Gateway contract.

Previous: [Metering and billing](./04-telemetry.md)
