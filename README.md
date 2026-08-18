# API Platform (Portfolio Project)

Plataforma de APIs multi-tenant com suporte a integrações externas (Applications), autenticação via API Key, gateway centralizado, observabilidade completa e rastreamento de consumo por cliente.

Este documento foi dividido em arquivos menores, organizados pela ordem de leitura recomendada. Comece pelo `00-overview.md`.

## Índice

| Arquivo | Conteúdo |
|---|---|
| [00-overview.md](./00-overview.md) | O que é o projeto, objetivo, as duas camadas de autenticação |
| [01-architecture.md](./01-architecture.md) | Diagrama de arquitetura global e fluxos de request |
| [02-domain-model.md](./02-domain-model.md) | Modelagem de domínio (tabelas) e ApplicationContext |
| [03-auth.md](./03-auth.md) | Autenticação via API Key e papel das Applications |
| [04-telemetry.md](./04-telemetry.md) | Telemetria, consumo e o usage metering pipeline |
| [05-decisions.md](./05-decisions.md) | Decisões arquiteturais (porquês) e pontos de extensibilidade |
| [06-stack-and-value.md](./06-stack-and-value.md) | Stack sugerida e valor do projeto como portfólio |
| [07-functional-requirements.md](./07-functional-requirements.md) | Requisitos Funcionais (RF01–RF35) |
| [08-summary.md](./08-summary.md) | Resumo final |
| [09-portal-api-checklist.md](./09-portal-api-checklist.md) | Checklist de implementação do Portal API |
| [09-portal-api-checklist.md](./09-portal-api-checklist.md) | Checklist de implementação do Portal API |

## Nota para retomada de contexto

Se esta conversa for resetada e o contexto se perder, **leia os arquivos na ordem do índice acima antes de propor qualquer implementação.** As decisões mais importantes, que não devem ser revertidas sem motivo explícito, são:

1. Existem **duas camadas de autenticação separadas e independentes**: login simples do Developer Portal (humano, PortalUser) e API Key das Applications (máquina). Elas não se misturam.
2. **Não existe conceito de "User" autenticando nas APIs de negócio** — apenas Applications. O `ApplicationContext` (Organization, ApplicationId, Scopes) é resolvido **somente no Gateway**.
3. **As APIs de negócio (Orders, Payments) não implementam nenhuma autenticação ou autorização** — são mocks ultra finos, sem banco, sem lógica. Toda confiança é delegada ao Gateway (YARP).
4. **Applications, Credentials e Scopes são geridos self-service pelo PortalUser** no Portal — a equipe operadora não gerencia isso manualmente um a um. Apenas o catálogo de Scopes é mantido via seed/migration no código.
5. **Telemetria de consumo (`ApiUsageDaily`) é populada por um job assíncrono em background**, nunca por escrita síncrona do Gateway durante o processamento de uma request (padrão usage metering pipeline / Ingest → Meter → Invoice).

Para detalhamento de cada um desses pontos, ver `05-decisions.md`.
