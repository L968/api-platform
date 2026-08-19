# API Platform Portal

React single-page application for the API Platform Developer Portal.

To start the complete platform, run `docker compose up --build -d` from the repository root. Nginx serves the frontend and proxies `/api` requests to the Gateway, which transparently forwards Portal API routes.

## Development

With PostgreSQL and `ApiPlatform.PortalApi` running:

```bash
npm install
npm run dev
```

The standalone frontend runs at `http://localhost:3000` and uses the local Portal API at `http://localhost:5019`.

To use a different environment, copy `.env.example` to `.env.local` and update `VITE_PORTAL_API_URL`.

## Validation

```bash
npm run lint
npm run test
npm run build
```

## Structure

- `src/features`: pages, contracts and operations grouped by feature.
- `src/shared/api`: shared HTTP client with HttpOnly cookie support.
- `src/shared/components`: small reusable UI components.
- `src/shared/layout`: authenticated Portal navigation.
