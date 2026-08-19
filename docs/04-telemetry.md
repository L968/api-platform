# Metering and billing

## Usage collection

The Gateway records authorized requests after routing. Each event contains:

- Organization ID
- Application ID
- API ID
- Normalized endpoint
- Response status
- Latency

Endpoints are normalized to avoid unbounded cardinality. For example, `GET /orders/123` is stored as `GET /orders/{id}`.

## Asynchronous pipeline

```text
Gateway request
  → bounded in-memory Channel
  → background aggregation
  → PostgreSQL upsert
  → api_usage_daily
```

The request does not wait for a database write. The worker aggregates events and flushes batches on a configurable interval. If persistence fails, the aggregate remains in memory for another attempt. If the bounded channel is full, the event is dropped and a warning is logged to protect request latency and process memory.

This implementation is appropriate for the current single-Gateway setup. A durable broker becomes necessary if multiple Gateway instances or zero-loss metering are required.

## Cost calculation

Only successful requests are billable:

```text
billable requests = request count - error count
amount = billable requests × active rate
```

The active rate is selected by Organization, API and usage date. A rate change creates a new effective period instead of updating the previous value.

The current month is calculated live for the dashboard. A background service creates a persisted invoice for the previous completed month. Invoice lines are grouped by endpoint and pricing period, so a mid-month price change produces separate lines at each rate.

Invoices can be marked as paid from the Portal to simulate payment state; no external payment processor is used.

Previous: [Authentication](./03-auth.md) · Next: [Architecture decisions](./05-decisions.md)
