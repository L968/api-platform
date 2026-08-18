# API Platform Portal

SPA administrativa da API Platform.

Para subir toda a plataforma, execute `docker compose up --build -d` na raiz do repositório. O Nginx serve o frontend e encaminha `/api` ao Gateway, que atua como proxy transparente para o PortalApi.

## Executar

Com o PostgreSQL e o `ApiPlatform.PortalApi` ativos:

```bash
npm install
npm run dev
```

Nesse modo avulso, o Portal abre em `http://localhost:3000` e usa o PortalApi local em `http://localhost:5019`.

Para apontar para outro ambiente, copie `.env.example` para `.env.local` e altere `VITE_PORTAL_API_URL`.

## Validação

```bash
npm run lint
npm run test
npm run build
```

## Estrutura

- `src/features`: telas, contratos e operações agrupados por funcionalidade.
- `src/shared/api`: cliente HTTP comum, sempre com o cookie HttpOnly.
- `src/shared/components`: componentes visuais pequenos e reutilizáveis.
- `src/shared/layout`: navegação autenticada do portal.
