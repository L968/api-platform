-- Dados opcionais para visualizar usage e pricing no ambiente local.
-- Remove somente os registros gerados por este próprio script.
-- Get-Content database/seed-usage-demo.sql -Raw |
--     docker exec -i api-platform-postgres-1 psql -U api-platform -d api-platform

WITH target_application AS (
    SELECT id, organization_id
    FROM application
    WHERE is_active = TRUE
    ORDER BY created_at
    LIMIT 1
)
DELETE FROM api_usage_daily usage
USING target_application
WHERE usage.application_id = target_application.id
  AND usage.date < CURRENT_DATE
  AND usage.id = md5(usage.application_id::text || usage.endpoint || usage.date::text)::uuid;

UPDATE organization_api_pricing
SET price_per_request = 0.0100,
    effective_from = CURRENT_DATE - 364
WHERE id = '00000000-0000-0000-0000-000000000030';

UPDATE organization_api_pricing
SET effective_from = CURRENT_DATE - 364
WHERE id = '00000000-0000-0000-0000-000000000031';

INSERT INTO organization_api_pricing (
    id, organization_id, api_id, price_per_request, effective_from, created_at
)
VALUES
    ('00000000-0000-0000-0000-000000000032', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000010', 0.2000, DATE '2026-01-01', CURRENT_TIMESTAMP),
    ('00000000-0000-0000-0000-000000000033', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000010', 0.1000, DATE '2026-05-01', CURRENT_TIMESTAMP),
    ('00000000-0000-0000-0000-000000000034', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000010', 0.0300, DATE '2026-08-01', CURRENT_TIMESTAMP)
ON CONFLICT (id) DO UPDATE SET
    price_per_request = EXCLUDED.price_per_request,
    effective_from = EXCLUDED.effective_from;

WITH target_application AS (
    SELECT id, organization_id
    FROM application
    WHERE is_active = TRUE
    ORDER BY created_at
    LIMIT 1
),
days AS (
    SELECT
        day::date AS usage_date,
        (day::date - (CURRENT_DATE - 364))::integer AS age,
        EXTRACT(ISODOW FROM day)::integer AS weekday,
        EXTRACT(DOY FROM day)::integer AS day_of_year
    FROM generate_series(CURRENT_DATE - 364, CURRENT_DATE - 1, INTERVAL '1 day') AS day
),
endpoints AS (
    SELECT *
    FROM (VALUES
        ('00000000-0000-0000-0000-000000000010'::uuid, 'GET /orders', 1, 180, 2, 0.012, 74),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'GET /orders/{id}', 2, 95, 1, 0.018, 112),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'GET /orders/search', 3, 70, 1, 0.026, 168),
        ('00000000-0000-0000-0000-000000000010'::uuid, 'POST /orders', 4, 42, 0, 0.021, 221),
        ('00000000-0000-0000-0000-000000000011'::uuid, 'GET /payments/{id}', 5, 58, 0, 0.014, 136)
    ) AS value(api_id, endpoint, position, base_requests, daily_growth, error_rate, base_latency)
),
request_values AS (
    SELECT
        target_application.id AS application_id,
        target_application.organization_id,
        endpoints.api_id,
        endpoints.endpoint,
        endpoints.position,
        days.usage_date,
        GREATEST(
            2,
            endpoints.base_requests
                + days.age * endpoints.daily_growth
                + CASE WHEN days.weekday IN (6, 7) THEN -55 WHEN days.weekday = 1 THEN 35 ELSE 10 END
                + ((days.day_of_year * (endpoints.position + 3)) % 91)
                + CASE WHEN days.day_of_year % 47 = 0 THEN 260 WHEN days.day_of_year % 31 = 0 THEN -80 ELSE 0 END
        )::integer AS request_count,
        endpoints.error_rate,
        endpoints.base_latency,
        days.day_of_year
    FROM target_application
    CROSS JOIN days
    CROSS JOIN endpoints
    WHERE NOT (endpoints.position = 4 AND days.weekday IN (6, 7))
      AND NOT (endpoints.position = 5 AND days.day_of_year % 5 = 0)
),
demo_usage AS (
    SELECT
        *,
        GREATEST(0, (request_count * error_rate)::integer + CASE WHEN day_of_year % 29 = 0 THEN 3 ELSE 0 END)::integer AS error_count
    FROM request_values
)
INSERT INTO api_usage_daily (
    id, organization_id, application_id, api_id, endpoint, date,
    request_count, error_count, avg_latency_ms
)
SELECT
    md5(application_id::text || endpoint || usage_date::text)::uuid,
    organization_id,
    application_id,
    api_id,
    endpoint,
    usage_date,
    request_count,
    LEAST(error_count, request_count),
    base_latency + ((day_of_year * position * 13) % 97)
FROM demo_usage
ON CONFLICT DO NOTHING;
