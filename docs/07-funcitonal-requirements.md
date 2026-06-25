# 07. Requisitos Funcionais

> Este arquivo é autocontido o suficiente para ser usado como input direto de implementação (ex.: para Claude Code). Para contexto de arquitetura e decisões por trás de cada bloco, ver os arquivos anteriores do índice em [README.md](./README.md).

## 1. Organizations

- RF01: O sistema deve permitir criar uma Organization (tenant). (Definir como será criado)
- RF02: O sistema deve permitir consultar, atualizar e desativar uma Organization.
- RF03: Toda entidade do sistema (Applications, Credentials, PortalUsers, etc.) deve estar vinculada a uma Organization.

## 2. PortalUsers (login do Developer Portal)

- RF04: O sistema deve permitir autenticar um PortalUser via email/senha.
- RF05: O sistema deve permitir que a equipe operadora crie PortalUsers manualmente (seed/script), sem fluxo de self-signup.
- RF06: O acesso de um PortalUser deve ser restrito à sua própria Organization.

## 3. Applications

- RF07: O sistema deve permitir que o PortalUser cadastre Applications (integrações/sistemas externos) para a sua própria Organization, via Developer Portal — self-service, sem intervenção da equipe operadora.
- RF08: O sistema deve permitir que o PortalUser consulte, atualize e desative as Applications da própria Organization.
- RF09: O último uso de uma Application é derivável de `ApiUsageDaily` (data mais recente com `RequestCount > 0`) — não é necessário um campo dedicado na tabela `Applications` para isso.

## 4. Credentials (API Keys)

- RF10: O sistema deve permitir que o PortalUser gere Credentials (API Key) para uma Application da própria Organization, via Developer Portal — self-service.
- RF11: O sistema deve permitir que o PortalUser revogue uma Credential da própria Organization.
- RF12: O sistema deve permitir definir expiração (`ExpiresAt`) para uma Credential.
- RF13: O sistema deve impedir o uso de Credentials revogadas ou expiradas.

## 5. Scopes e Autorização

- RF14: O sistema deve manter um catálogo de Scopes disponíveis (ex.: `orders.read`, `payments.write`), versionado via seed/migration no código — sem necessidade de uma tela admin dedicada. Novos Scopes são adicionados pela equipe operadora quando uma nova API é incorporada à plataforma, junto do deploy correspondente.
- RF15: O sistema deve permitir que o PortalUser, ao gerar uma Credential, selecione quais Scopes (dentre os disponíveis no catálogo) ela terá — sem precisar de aprovação manual da equipe operadora.
- RF16: O Gateway deve validar se a Application possui o Scope necessário antes de liberar acesso ao endpoint.

## 6. Gateway (YARP) — Autenticação, Autorização e Roteamento

Toda autenticação e autorização do sistema acontece **exclusivamente no Gateway**. As APIs de negócio não validam nada — apenas confiam no que chega.

- RF17: O Gateway deve validar toda requisição destinada às APIs de negócio via API Key (header).
- RF18: O Gateway deve resolver toda requisição autenticada em um `ApplicationContext` (Organization, ApplicationId, Scopes), via Claims, e disponibilizá-lo para a Authorization Policy da rota.
- RF19: O Gateway deve rejeitar (401/403) requisições sem API Key válida, ou com a Key revogada ou expirada, **antes** de rotear para a API de negócio.
- RF20: O Gateway deve validar, por rota, se o `ApplicationContext` possui o Scope exigido (via Authorization Policy baseada em Claims), rejeitando com 403 caso não possua.
- RF21: O Gateway deve aplicar rate limiting por Application.
- RF22: O Gateway deve enviar dados de telemetria (organization, application, endpoint, latência, status) ao OpenTelemetry Collector em paralelo ao roteamento da requisição — não como um passo sequencial antes ou depois do proxy.
- RF23: O Gateway deve rotear requisições para as APIs corretas (Orders, Payments, etc.) com base no path/configuração, somente após aprovação de autenticação e autorização.

## 7. APIs de Negócio (Orders / Payments)

- RF24: As APIs de negócio devem expor endpoints simulados (mock), sem persistência em banco de dados.
- RF25: As APIs de negócio **não devem implementar nenhuma lógica de autenticação ou autorização** — nenhuma validação de API Key, Scope ou identidade ocorre nelas. Toda a confiança é delegada ao Gateway.
- RF25a (limitação conhecida): como as APIs de negócio não têm auth própria, elas não devem ser expostas publicamente — apenas o Gateway é exposto externamente; as APIs de negócio ficam acessíveis somente via rede interna do Docker Compose.

## 8. Telemetria e Consumo

- RF26: Um job em background (assíncrono, fora do caminho crítico das requisições) deve agregar os dados de telemetria e popular `ApiUsageDaily` periodicamente (ex.: a cada hora ou diariamente) — o Gateway nunca escreve diretamente nessa tabela durante o processamento de uma request. Padrão conhecido como **usage metering pipeline** (Ingest → Meter → Invoice/Consulta), o mesmo usado pela Stripe para billing por uso — mecanismo detalhado em `04-telemetry.md`, justificativa da decisão em `05-decisions.md`.
- RF27: O sistema deve registrar uso agregado diário por Organization e Application (`ApiUsageDaily`).
- RF28: O sistema deve permitir consultar consumo por cliente (Organization), por aplicação e por API.
- RF29: O sistema deve permitir consultar erros por endpoint e latência média por tenant.
- RF30: O sistema deve enviar dados de telemetria via OpenTelemetry para o Collector, como fonte para o job de agregação (RF26) e para observabilidade operacional via Prometheus/Grafana.

## 9. Billing

- RF31: A equipe operadora deve poder definir um preço por chamada (`PricePerRequest`) por Organization e por API (`OrganizationApiPricing`) — não é self-service do PortalUser, é decisão comercial.
- RF32: O sistema deve calcular o valor devido por uma Organization, em tempo real, a partir de `ApiUsageDaily` (RequestCount do período) multiplicado pelo `PricePerRequest` correspondente de cada API — sem persistir fatura fechada (`Invoices`).
- RF33: O cálculo de billing deve poder ser feito para o mês corrente (em andamento) e não apenas para períodos já encerrados, permitindo ao PortalUser acompanhar o gasto antes do fim do mês.

## 10. Developer Portal

- RF34: O Portal deve exigir login (email/senha) do PortalUser antes de qualquer acesso.
- RF35: O Portal deve permitir que o PortalUser visualize sua própria Organization, suas Applications e Credentials.
- RF36: O Portal é o único canal pelo qual o PortalUser realiza as operações self-service descritas em RF07-RF08 (Applications) e RF10-RF11/RF15 (Credentials e Scopes).
- RF37: O Portal deve permitir visualizar métricas de consumo e telemetria (via Grafana ou dashboards próprios).
- RF38: O Portal deve exibir uma tela de billing com o consumo e valor devido do período corrente, calculados conforme RF32-RF33.

---

**Anterior:** [06-stack-and-value.md](./06-stack-and-value.md) · **Próximo:** [08-summary.md](./08-summary.md)