# 02. Modelagem de Domínio

> Convenção: toda tabela que representa uma entidade mutável (pode ser editada após criada) tem `CreatedAt` e `UpdatedAt`. `ApiUsageDaily` é exceção — é registro agregado, não editado manualmente (ver nota na própria seção).

---

## 1. Organizations (multi-tenant)

Representa uma empresa-cliente (tenant) da plataforma. É a entidade raiz: toda outra entidade do sistema pertence a uma Organization.

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único da Organization. |
| `Name` | string | Nome da empresa-cliente, exibido no Portal. |
| `Status` | enum (`Active`, `Suspended`, `Disabled`) | Situação da conta. `Suspended`/`Disabled` bloqueiam login dos PortalUsers e uso das API Keys da Organization. |
| `CreatedAt` | datetime | Data/hora de criação do registro. |

---

## 2. PortalUsers (login humano do Developer Portal)

Acesso administrativo de uma pessoa da empresa-cliente ao Developer Portal. Criado manualmente pela equipe operadora (seed/script), sem self-signup, sem provider plugável. Uma Organization pode ter múltiplos PortalUsers (ver racional em `05-decisions.md`).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único do PortalUser. |
| `OrganizationId` | Guid (FK) | Organization à qual este login pertence. Define o isolamento de acesso — um PortalUser só vê dados da própria Organization. |
| `Email` | string | Email usado para login. Único no sistema. |
| `PasswordHash` | string | Hash da senha (nunca a senha em texto puro). Algoritmo recomendado: bcrypt ou Argon2id. |
| `Status` | enum (`Active`, `Disabled`) | Se `Disabled`, o login é bloqueado mesmo com credenciais corretas (ex.: pessoa saiu da empresa-cliente). |
| `CreatedAt` | datetime | Data/hora de criação do registro. |
| `UpdatedAt` | datetime | Data/hora da última atualização (ex.: troca de senha, mudança de `Status`). |

---

## 3. Apis (catálogo de APIs de negócio)

Catálogo das APIs de negócio oferecidas pela plataforma (ex.: Orders, Payments). Mantido via seed/migration pela equipe operadora, junto do deploy de cada nova API — não é tela administrativa (mesmo padrão de `Scopes`, ver `07-functional-requirements.md`, RF14). Referenciada por FK em `ApiUsageDaily` e `OrganizationApiPricing`, em vez de string solta — evita inconsistência de nome (ex.: "Orders" vs "order" não casarem silenciosamente).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único da API. |
| `Name` | string | Nome da API de negócio, exibido no Portal e em relatórios (ex.: `Orders`, `Payments`). |

---

## 4. Applications (sistemas / integrações)

Representa um sistema externo da empresa-cliente que consome as APIs de negócio (ex.: o ERP da Acme, ou um job que roda à noite). É o único tipo de cliente autenticável nas APIs — cadastrado pelo próprio PortalUser, self-service (ver `07-functional-requirements.md`, RF07).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único da Application. |
| `OrganizationId` | Guid (FK) | Organization à qual esta Application pertence. |
| `Name` | string | Nome dado pelo cliente para identificar o sistema (ex.: "ERP Acme — Financeiro"). |
| `Type` | enum (`Web`, `ERP`, `Job`, `Mobile`) | Categoria do sistema, apenas informativo/organizacional — não afeta autenticação ou autorização. |
| `CreatedAt` | datetime | Data/hora de criação do registro. |
| `UpdatedAt` | datetime | Data/hora da última atualização (ex.: troca de `Name` ou `Type`). |

---

## 5. Credentials (API Keys das Applications)

Representa uma API Key gerada para uma Application. Uma Application pode ter **múltiplas Credentials** (relação 1-para-muitos). Isso é intencional, não acidental — permite:

- **Rotação sem downtime**: gerar uma nova Credential, validar, só então revogar a antiga.
- **Granularidade de permissão dentro do mesmo sistema**: ex.: o ERP da Acme pode ter uma Credential só com `orders.read` (módulo de relatórios) e outra com `orders.read` + `orders.write` (módulo financeiro) — mesma Application, raio de dano menor se uma delas for comprometida.
- **Revogação seletiva**: se uma chave específica vaza, revoga só ela, sem afetar o resto da integração.

É o mesmo modelo usado por Stripe (restricted keys), AWS IAM (access keys) e GitHub (personal access tokens).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único da Credential. |
| `OrganizationId` | Guid (FK) | Organization à qual esta Credential pertence (redundante com `ApplicationId.OrganizationId`, mas mantido para permitir filtros diretos sem JOIN). |
| `ApplicationId` | Guid (FK) | Application à qual esta Credential está vinculada. |
| `ClientId` | string | Parte **pública** e não-secreta da API Key — funciona como identificador de busca rápida (indexado). Permite ao Gateway localizar a Credential no banco em O(1) antes de validar o segredo, e permite ao Portal exibir "qual chave é essa" (ex.: `app_8f3a2b...`) sem nunca expor o segredo de novo após a criação. |
| `SecretHash` | string | Hash da parte **secreta** da API Key — nunca reversível, nunca exibido após a criação. O valor em texto puro só é mostrado ao PortalUser uma única vez, no momento da geração. |
| `CreatedAt` | datetime | Data/hora de criação da Credential. |
| `ExpiresAt` | datetime (nullable) | Data/hora a partir da qual a Credential deixa de ser válida, mesmo sem revogação explícita. Nulo = sem expiração definida. |
| `RevokedAt` | datetime (nullable) | Data/hora em que a Credential foi revogada manualmente pelo PortalUser. Nulo = ainda não revogada. |

---

## 6. Scopes (autorização)

Catálogo de permissões possíveis na plataforma (ex.: `orders.read`, `payments.write`). Mantido via seed/migration pela equipe operadora, não por tela administrativa (ver `07-functional-requirements.md`, RF14).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único do Scope. |
| `Name` | string | Nome do Scope, no formato `recurso.ação` (ex.: `orders.read`, `payments.write`). Usado pelo Gateway para checagem de autorização. |

---

### CredentialScopes (tabela de associação)

Associa uma Credential a um ou mais Scopes — define o que aquela API Key específica está autorizada a fazer.

| Campo | Tipo | Descrição |
|---|---|---|
| `CredentialId` | Guid (FK) | Credential à qual o Scope está sendo concedido. |
| `ScopeId` | Guid (FK) | Scope concedido a essa Credential. |

---

## 7. ApiUsageDaily (consumo agregado, billing-ready)

Agregado diário de uso por Organization/Application/Endpoint — não logs brutos por requisição. Populado por job assíncrono em background, nunca por escrita síncrona do Gateway. Ver mecanismo completo de população em [04-telemetry.md](./04-telemetry.md).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único da linha de agregação. |
| `OrganizationId` | Guid (FK) | Organization à qual este consumo pertence. |
| `ApplicationId` | Guid (FK) | Application que gerou esse consumo. |
| `ApiId` | Guid (FK) | API de negócio à qual este consumo se refere (`Apis.Id`). |
| `Endpoint` | string | Endpoint específico chamado (ex.: `/orders`, `/orders/{id}`). |
| `Date` | date | Dia a que esse resumo se refere (sem horário — granularidade diária). |
| `RequestCount` | int | Total de requisições bem-sucedidas e com erro, somadas, nesse dia/Application/Endpoint. |
| `ErrorCount` | int | Subconjunto de `RequestCount` que resultou em erro (status 4xx/5xx). |
| `AvgLatencyMs` | int | Latência média, em milissegundos, das requisições desse dia/Application/Endpoint. |

---

## 8. OrganizationApiPricing (billing)

Define o preço por chamada cobrado de uma Organization, por API. Permite contratos diferenciados: cada empresa-cliente pode ter um preço próprio, e o preço pode variar entre APIs (ex.: Orders mais barato que Payments). Mantido pela equipe operadora — não é self-service do PortalUser (preço é decisão comercial, não técnica).

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | Guid | Identificador único do registro de pricing. |
| `OrganizationId` | Guid (FK) | Organization à qual este preço se aplica. |
| `ApiId` | Guid (FK) | API de negócio à qual este preço se refere (`Apis.Id`). |
| `PricePerRequest` | decimal | Valor cobrado por chamada bem-sucedida a essa API, para essa Organization. |
| `EffectiveFrom` | date | Primeiro dia em que este preço deve ser aplicado. |
| `CreatedAt` | datetime | Data/hora de criação do registro. |
| `UpdatedAt` | datetime | Data/hora da última atualização (ex.: reajuste de preço). |

> **Nota:** não existe tabela de fatura fechada (`Invoices`). O valor devido é **calculado em tempo real**, sempre que a tela de billing do Portal é acessada — não há "fechamento de mês" persistido. Cada alteração comercial cria uma nova linha de preço com `EffectiveFrom`; preços anteriores não são sobrescritos. O cálculo usa apenas chamadas bem-sucedidas (`RequestCount - ErrorCount`) e o preço vigente na data do consumo, preservando o valor histórico.

---

## 9. ApplicationContext (Core Concept — não é uma tabela)

Não é uma entidade persistida no banco — é a estrutura **em memória**, resolvida pelo Gateway a cada requisição autenticada, usada para checagem de Scopes via Claims/Authorization Policy.

| Campo | Tipo | Descrição |
|---|---|---|
| `OrganizationId` | Guid | Organization da Application autenticada, resolvida a partir da Credential validada. |
| `ApplicationId` | Guid | Application autenticada nesta requisição. |
| `Scopes` | string[] | Lista de Scopes que a Credential usada possui, resolvida via `CredentialScopes`. |

O `ApplicationContext` existe e é validado **apenas no Gateway** (via Claims/Authorization Policy). A API de negócio não resolve, não valida e não enxerga esse contexto — ela apenas recebe a requisição já aprovada. Não existe conceito de "Principal" genérico (User | Application) — apenas Application.

---

**Anterior:** [01-architecture.md](./01-architecture.md) · **Próximo:** [03-auth.md](./03-auth.md)
