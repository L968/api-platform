# 01. Arquitetura

Toda chamada de uma Application a uma API de negócio passa primeiro pelo **Gateway** — uma "porta de entrada" única, que: confere se a API Key é válida, verifica se ela tem permissão (Scope) para aquele endpoint, controla quantas chamadas por minuto são permitidas (rate limiting), e registra dados de uso para fins de monitoramento e cobrança. Só depois dessas verificações a chamada é roteada para a API de negócio correspondente (Orders, Payments, etc.).

## 1. Arquitetura Global

```
                    +----------------------+
                    | Developer Portal     |
                    | (Next.js)            |
                    | Login: user/senha    |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | Portal Backend       |
                    | (login fixo,         |
                    |  sem plugabilidade)  |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | API Gateway (YARP)   |
                    |                      |
                    | - API Key Auth       |
                    | - Authorization      |
                    | - Rate Limiting      |
                    | - Telemetry Enrich   |
                    +----------+-----------+
                               |
              +----------------+-----------------+
              |                |                 |
              v                v                 v

 +-------------------+  +-------------------+  +-----------------------+
 | Orders API        |  | Payments API      |  | OpenTelemetry Collector|
 +-------------------+  +-------------------+  +-----------+------------+
                                                            |
                                               +-------------+-------------+
                                               |                           |
                                               v                           v

                                       +-------------------+      +----------------------+
                                       | Prometheus         |      | Grafana              |
                                       +-------------------+      +----------------------+
```

> **Nota:** a telemetria é emitida **pelo Gateway**, não pelas APIs Orders/Payments. Como essas APIs são mocks "ultra finos" (zero lógica, zero auth), instrumentá-las com OpenTelemetry seria esforço/dependência desnecessária — o Gateway já captura tudo que importa (organization, application, endpoint, latência, status), pois é ali que o `ApplicationContext` é resolvido. Detalhamento do critério em [05-decisions.md](./05-decisions.md).

---

## 2. Fluxo de Request

### 2.1 Chamada à API de negócio (Application)

```
Application (cliente externo)
  |
  v
API Gateway (YARP)
  |
  +-- Validate API Key
  +-- Resolve ApplicationContext
  +-- Check Scopes
  +-- Rate Limiting
  |
  +----------------------------------------+
  |                                        |
  v                                        v
Business APIs (Orders / Payments / etc)    OpenTelemetry Collector
  |                                        |
  v                                        v
Resposta ao Application               Prometheus + Grafana
```

> O envio de telemetria ao Collector parte do **Gateway**, em paralelo ao roteamento para a Business API — não é um passo sequencial após a resposta. As Business APIs não emitem telemetria própria.

### 2.2 Login no Developer Portal (humano)

```
PortalUser (humano)
  |
  v
Portal Backend
  |
  +-- Validate email/senha
  +-- Criar sessão/JWT de portal
  |
  v
Acesso às telas de Applications / Credentials / Métricas
```

> O login do Portal **não passa pelo Gateway de APIs de negócio** e não gera Scopes nem telemetria de consumo — é apenas a porta de entrada administrativa.

---

**Anterior:** [00-overview.md](./00-overview.md) · **Próximo:** [02-domain-model.md](./02-domain-model.md)