-- Dados opcionais para visualizar o dashboard de usage no ambiente local.
-- Execute depois de criar ao menos uma Application ativa:
-- Get-Content database/seed-usage-demo.sql -Raw |
--     docker exec -i api-platform-postgres-1 psql -U api-platform -d api-platform
--
-- O histórico termina ontem para não alterar as métricas reais de hoje.
-- ON CONFLICT torna a execução repetível sem duplicar dados.

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
        EXTRACT(ISODOW FROM day)::integer AS weekday
    FROM generate_series(
        CURRENT_DATE - 364,
        CURRENT_DATE - 1,
        INTERVAL '1 day'
    ) AS day
),
demo_usage AS (
    SELECT
        target_application.id AS application_id,
        target_application.organization_id,
        days.usage_date,
        endpoint.name AS endpoint,
        endpoint.position,
        GREATEST(
            1,
            endpoint.base_requests
                + days.age * endpoint.daily_growth
                + CASE WHEN days.weekday IN (6, 7) THEN -35 ELSE 45 END
                + ((days.age * endpoint.position * 17) % 61)
        )::integer AS request_count
    FROM target_application
    CROSS JOIN days
    CROSS JOIN (
        VALUES
            ('GET /orders', 1, 180, 2),
            ('GET /orders/{id}', 2, 90, 1)
    ) AS endpoint(name, position, base_requests, daily_growth)
)
INSERT INTO api_usage_daily (
    id,
    organization_id,
    application_id,
    api_id,
    endpoint,
    date,
    request_count,
    error_count,
    avg_latency_ms
)
SELECT
    md5(application_id::text || endpoint || usage_date::text)::uuid,
    organization_id,
    application_id,
    '00000000-0000-0000-0000-000000000010',
    endpoint,
    usage_date,
    request_count,
    CASE
        WHEN position = 1 THEN request_count / 80
        ELSE request_count / 55
    END,
    75 + position * 24 + ((EXTRACT(DOY FROM usage_date)::integer * 7) % 65)
FROM demo_usage
ON CONFLICT DO NOTHING;
