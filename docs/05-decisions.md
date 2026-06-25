# 05. Decisões Arquiteturais e Extensibilidade

## 1. Decisões Arquiteturais

### Por que Gateway?

Centraliza:

- validação de API Key
- autorização (Scopes)
- observabilidade
- rate limiting

### Por que a API só conhece Applications (sem conceito de Principal genérico)?

Nesta fase do projeto, a API só precisa atender integrações entre sistemas (Applications). Login humano em APIs de negócio (com MFA, sessão, etc.) adiciona complexidade que não tem relação com o foco do portfólio (Auth de API, Gateway, telemetria). O login do Portal continua existindo, mas como uma camada separada e simples (usuário/senha), sem qualquer relação com a autenticação das APIs.

Se no futuro fizer sentido (ex.: demonstrar suporte multi-principal), o conceito pode ser reintroduzido — mas será uma decisão consciente, não um requisito assumido desde o início.

### Por que Organizations e PortalUsers são tabelas separadas?

Uma Organization pode ter **múltiplos PortalUsers** (relação 1-para-muitos) — por exemplo, uma pessoa que gerencia integrações e outra que só acompanha consumo, ambas da mesma empresa-cliente. A Organization é a entidade de negócio (o tenant); cada PortalUser é uma credencial de acesso humano a esse tenant. Fundir as duas em uma única tabela engessaria o modelo em "1 login por empresa", o que não reflete como empresas-cliente reais operam (normalmente mais de uma pessoa precisa de acesso ao painel). Manter separado também mantém consistência com o restante do modelo, onde toda relação Organization → entidade dependente já é 1-para-muitos (Applications, Credentials).

### Por que Orders e Payments são mocks?

Porque o foco do projeto é demonstrar arquitetura de plataforma — auth, gateway, telemetria e rastreabilidade — e não regras de negócio de domínios específicos. Manter essas APIs ultra finas (sem banco, sem lógica) evita ruído e mantém a atenção do avaliador nas partes que de fato importam para o portfólio.

### Por que agregação diária (`ApiUsageDaily`) em vez de logs brutos ou só Prometheus/Grafana?

Logs brutos por request não escalam (custo, volume, consultas lentas conforme a tabela cresce). Prometheus/Grafana não substituem essa necessidade porque têm retenção curta e não são feitos para consulta de negócio por tenant (ex.: "quanto a Acme consumiu em maio, para faturamento"). `ApiUsageDaily` no Postgres é dado permanente, cruzável via SQL com Organization/Application — a base do "billing-ready" (ver [06-stack-and-value.md](./06-stack-and-value.md)).

Esse modelo (agregação assíncrona, populada por job em background, nunca por escrita síncrona do Gateway) não é invenção do projeto — é um padrão real de mercado conhecido como **usage metering pipeline**, a mesma estrutura que a própria Stripe usa para billing por uso (Ingest → Meter → Invoice). Ver o mecanismo técnico completo em [04-telemetry.md](./04-telemetry.md).

### Por que simular billing sem transação financeira real, e por que sem fatura fechada (`Invoices`)?

O objetivo não é processar pagamento de verdade (isso reintroduziria dependência externa, contrariando a decisão de `00-overview.md` de rodar tudo via `docker-compose up`), mas demonstrar a capacidade de billing-by-usage que toda a telemetria do projeto já viabiliza (`ApiUsageDaily` + `OrganizationApiPricing`). A tela de billing no Portal calcula o valor devido em **tempo real**, consultando o consumo do período corrente — não existe um "fechamento de mês" persistido (`Invoices`). Isso reflete um caso de uso real: o cliente quer poder acompanhar o gasto durante o mês, não só descobrir o valor depois que o período já encerrou.

Pricing é definido por **Organization + API** (não por Application, não um valor global único) — permite contratos diferenciados por cliente (ex.: Acme paga menos que outra empresa) e preços diferentes por produto (Orders mais barato que Payments), refletindo como negociação comercial real funciona numa plataforma B2B.

### Por que telemetria só no Gateway, não nas APIs de negócio?

Em arquitetura de microsserviços com lógica de negócio real, é comum instrumentar telemetria em cada camada: o Gateway sabe quem chamou e quanto tempo total levou; cada API sabe o que aconteceu *dentro* dela (tempo de query, chamadas a outros serviços, exceções).

Esse projeto não tem esse cenário. Orders e Payments são mocks "ultra finos" — zero banco, zero lógica, resposta instantânea fake. Não existe processamento interno para medir, então instrumentá-las geraria spans vazios, sem informação nova. Além disso, o dado que realmente importa para o produto (Organization, Application, Scope, billing) só existe no Gateway, pois é ali que o `ApplicationContext` é resolvido — a API mock nunca tem acesso a esse contexto.

**Critério para reabrir essa decisão no futuro:**

- **API é mock / sem lógica real** → telemetria só no Gateway. *(situação atual)*
- **API ganha lógica real** (banco, chamadas a outros serviços, processamento) → instrumentar a API também, para rastrear onde o tempo é gasto dentro dela — telemetria nos dois (Gateway + API).

---

## 2. Extensibilidade

### Auth de APIs

Hoje fixo em API Key. Pontos de extensão futuros, a avaliar conforme necessidade:

- Suporte a múltiplos tipos de credential (ex.: Client Secret/OAuth2 client-credentials)
- Reintrodução de um conceito de Principal genérico, se houver caso de uso real

### Login do Portal

Hoje fixo em usuário/senha, criado manualmente pela equipe operadora. Sem plugabilidade prevista por enquanto — não é o foco do portfólio.

### Observabilidade

Troca fácil:

- Prometheus → Datadog → Azure Monitor

### APIs

Escala horizontal:

- Orders API
- Payments API
- X APIs adicionais

---

**Anterior:** [04-telemetry.md](./04-telemetry.md) · **Próximo:** [06-stack-and-value.md](./06-stack-and-value.md)