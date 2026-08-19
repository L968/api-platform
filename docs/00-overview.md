# Overview

API Platform exposes business APIs to external organizations through a central Gateway and a self-service Developer Portal.

## Product model

- An **Organization** is a customer account and the tenant boundary.
- A **Portal User** signs in to manage that Organization.
- An **Application** represents a customer system that consumes the APIs.
- A **Credential** is an API key issued to an Application.
- A **Scope** grants a Credential access to a specific capability, such as `orders.read`.
- An **API** is a product exposed by the platform, currently Orders or Payments.

Organizations manage their own Applications and Credentials. The platform operator owns the API and Scope catalogs and defines pricing for each Organization and API.

## Authentication boundaries

The platform has two independent authentication flows:

| Flow | Actor | Authentication |
|---|---|---|
| Developer Portal | Human user | Email, password and HttpOnly session cookie |
| Business APIs | Application | `X-Api-Key` header |

A Portal session cannot call a business API, and an API key cannot access Portal management routes.

## Customer flow

1. Sign in to the Developer Portal.
2. Create an Application.
3. Issue an API key with the required scopes and expiration.
4. Call Orders or Payments through the Gateway.
5. Monitor requests, errors, latency and cost.
6. Review monthly invoices.

Orders and Payments are intentionally small sample APIs. The platform behavior around them—authentication, authorization, metering, pricing and billing—is the focus of the system.

Next: [Architecture](./01-architecture.md)
