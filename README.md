<div align="center">

# API Platform

Managed access, usage metering and billing for B2B APIs.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-149ECA?logo=react&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-Strict-3178C6?logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

</div>

![Developer Portal overview](./.github/assets/dashboard.png)

API Platform provides the pieces needed to expose business APIs to external organizations: self-service applications, scoped API keys, a central Gateway, asynchronous usage metering and monthly billing.

Organizations manage their integrations through the Developer Portal. Their systems call Orders and Payments through the Gateway, which applies the platform policies before forwarding each request.

## From API key to invoice

```text
Create Application
  -> issue a scoped API key
  -> call an API through the Gateway
  -> meter usage asynchronously
  -> calculate cost with the active rate
  -> generate the monthly invoice
```

The platform covers that complete flow without requiring an external identity provider, telemetry service or payment processor.

## Architecture

```mermaid
flowchart LR
    Browser[Developer Portal] --> Nginx[Nginx]
    Client[API client] --> Gateway[Gateway / YARP]
    Nginx --> Gateway

    Gateway --> PortalApi[Portal API]
    Gateway --> Orders[Orders API]
    Gateway --> Payments[Payments API]

    Gateway --> Channel[Usage channel]
    Channel --> Worker[Metering worker]
    Worker --> Database[(PostgreSQL)]
    PortalApi --> Database
```

The Gateway is the public backend entry point. Orders, Payments and the Portal API remain inside the Docker network.

For business routes, the Gateway validates the API key, checks scopes, applies rate limiting, records usage and removes `X-Api-Key` before proxying the request. Portal routes pass through the same Gateway as a transparent proxy and use a separate human login session.

## Application credentials

Each API key belongs to an Application and receives only the scopes selected at creation. The full key is displayed once; only its public client ID and a PBKDF2 hash of the secret are stored.

Multiple keys per Application allow credential rotation and selective revocation. Disabling an Application works as a reversible kill switch for all of its keys.

![Application API keys](./.github/assets/application-details.png)

## Usage and cost

The Gateway publishes usage events to a bounded in-memory channel. A background worker aggregates and persists them, keeping PostgreSQL writes out of the request path.

The Portal exposes request volume, errors, latency and cost over time. Pricing uses effective dates, so rate changes do not rewrite historical usage.

| Traffic and reliability | Cost and pricing history |
|---|---|
| [![Request and error chart](./.github/assets/usage.png)](./.github/assets/usage.png) | [![Cost and pricing chart](./.github/assets/usage-cost.png)](./.github/assets/usage-cost.png) |

## Monthly invoices

A background service creates an invoice for each completed month. Lines are grouped by endpoint and pricing period, which keeps an issued invoice stable even when rates change later.

Payment is simulated locally by marking an open invoice as paid.

![Monthly invoice](./.github/assets/invoice-details.png)

## API Explorer

Orders and Payments can be tested from the Portal without opening another client. Requests still go through the Gateway and use the same API key and scope validation as an external integration.

![API Explorer](./.github/assets/api-explorer.png)

## Stack

| Area | Technology |
|---|---|
| Frontend | React 19, TypeScript, Vite, TanStack Query, Recharts, Tailwind CSS |
| Backend | .NET 10, ASP.NET Core, Entity Framework Core |
| Gateway | YARP Reverse Proxy |
| Database | PostgreSQL |
| Infrastructure | Docker Compose, Nginx |
| Tests | xUnit, Vitest, Testing Library |

## Run locally

Docker Desktop is the only requirement.

```bash
docker compose up --build -d
```

| Service | URL |
|---|---|
| Developer Portal | http://localhost:3000 |
| API Gateway | http://localhost:5290 |
| Portal API through the Gateway | http://localhost:3000/api |
| PostgreSQL | localhost:5432 |

Local login:

```text
Email: developer@acme.test
Password: DemoAccess123!
```

Migrations and the development seed are applied automatically by the Portal API.

```bash
docker compose ps
docker compose logs -f
docker compose down
```

`docker compose down` preserves the database volume. Use `docker compose down -v` to recreate the local database from scratch.

## Call an API

Business APIs are accessed through the Gateway:

```bash
curl http://localhost:5290/orders \
  -H "X-Api-Key: app_<client-id>.sk_<secret>"
```

## Repository layout

```text
backend/
  src/
    ApiPlatform.Gateway/
    ApiPlatform.PortalApi/
    ApiPlatform.OrdersApi/
    ApiPlatform.PaymentsApi/
  tests/
frontend/
  src/features/
database/
docs/
```

The Portal API and frontend are organized by feature. Gateway code is divided by responsibility: authentication, authorization, rate limiting, metering and proxy configuration.

## Technical documentation

- [Overview](./docs/00-overview.md)
- [Architecture](./docs/01-architecture.md)
- [Domain model](./docs/02-domain-model.md)
- [Authentication](./docs/03-auth.md)
- [Metering and billing](./docs/04-telemetry.md)
- [Architecture decisions](./docs/05-decisions.md)
