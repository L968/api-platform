-- Local/demo seed for the Portal API.
-- Run after applying migrations, for example:
-- psql "$DATABASE_URL" -f database/seed.sql
--
-- Local login:
-- email: developer@acme.test
-- password: DemoAccess123!
-- Change this password before using the application outside development.

INSERT INTO organization (id, name, status, created_at)
VALUES ('00000000-0000-0000-0000-000000000001', 'Acme Corp', 'Active', CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

INSERT INTO portal_user (id, organization_id, email, password_hash, status, created_at, updated_at)
VALUES (
    '00000000-0000-0000-0000-000000000002',
    '00000000-0000-0000-0000-000000000001',
    'developer@acme.test',
    'pbkdf2-sha256$120000$+uiKCOn1b1MoH1XMkea62g==$lt3bfCjYY5FqUl9lFScL64J/5/q5ZBp1aKFWzB1fPD0=',
    'Active',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (id) DO NOTHING;

INSERT INTO api (id, name)
VALUES
    ('00000000-0000-0000-0000-000000000010', 'Orders'),
    ('00000000-0000-0000-0000-000000000011', 'Payments')
ON CONFLICT (id) DO NOTHING;

INSERT INTO scope (id, name)
VALUES
    ('00000000-0000-0000-0000-000000000020', 'orders.read'),
    ('00000000-0000-0000-0000-000000000021', 'orders.write'),
    ('00000000-0000-0000-0000-000000000022', 'payments.read'),
    ('00000000-0000-0000-0000-000000000023', 'payments.write')
ON CONFLICT (id) DO NOTHING;

INSERT INTO organization_api_pricing (id, organization_id, api_id, price_per_request, effective_from, created_at)
VALUES
    ('00000000-0000-0000-0000-000000000030', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000010', 0.0100, (CURRENT_DATE - INTERVAL '1 year')::date, CURRENT_TIMESTAMP),
    ('00000000-0000-0000-0000-000000000031', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000011', 0.0200, (CURRENT_DATE - INTERVAL '1 year')::date, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;
