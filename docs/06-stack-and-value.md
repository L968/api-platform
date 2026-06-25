# 06. Stack Sugerida e Valor do Projeto

## 1. Stack sugerida

**Backend:**

- .NET 10
- Clean Architecture
- Vertical Slice
- YARP Gateway
- PostgreSQL
- Redis (avaliar necessidade)

**Observability:**

- OpenTelemetry
- Prometheus
- Grafana
- Jaeger

**Frontend:**

- Next.js (Developer Portal)

**Infra:**

- Docker Compose (execução local completa, sem dependências externas)

---

## 2. Valor do Projeto (portfólio)

Demonstra:

- arquitetura distribuída
- multi-tenancy real
- API gateway pattern
- autenticação via API Key bem desenhada
- observability completa
- design pragmático (sem complexidade desnecessária)
- readiness para billing real — com billing-by-usage simulado (preço por Organization/API, cálculo em tempo real, tela no Portal), não apenas como conceito

---

**Anterior:** [05-decisions.md](./05-decisions.md) · **Próximo:** [07-functional-requirements.md](./07-functional-requirements.md)