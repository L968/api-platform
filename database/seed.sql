-- Seed local/demo do Portal API.
-- Execute depois da migration, por exemplo:
-- psql "$DATABASE_URL" -f database/seed.sql
--
-- Login inicial:
-- email: admin@example.com
-- senha: ChangeMe123!
-- Troque essa senha antes de usar fora de desenvolvimento.

INSERT INTO organization (id, name, status, created_at)
VALUES ('00000000-0000-0000-0000-000000000001', 'Acme Corp', 'Active', CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;

INSERT INTO portal_user (id, organization_id, email, password_hash, status, created_at, updated_at)
VALUES (
    '00000000-0000-0000-0000-000000000002',
    '00000000-0000-0000-0000-000000000001',
    'admin@example.com',
    'pbkdf2-sha256$120000$V6A2hF3PTPL1dp1A3oj4Mw==$+9Y6dsVxl4fVMvJQgJSJGduiTFJs53/04G2uDqEe0s4=',
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

INSERT INTO organization_api_pricing (id, organization_id, api_id, price_per_request, created_at, updated_at)
VALUES
    ('00000000-0000-0000-0000-000000000030', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000010', 0.0100, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('00000000-0000-0000-0000-000000000031', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000011', 0.0200, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;
