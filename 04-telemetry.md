# 04. Telemetria e Consumo

## 1. O que é capturado

No Gateway:

```
organization.id
application.id
endpoint
latency
status
```

Enviado via OpenTelemetry.

### Consultas possíveis:

- consumo por cliente (Organization)
- consumo por aplicação
- consumo por API
- erros por endpoint
- latência por tenant

---

## 2. Pipeline de agregação (Ingest → Meter → Invoice)

Em vez de logs brutos por request, o consumo é agregado diariamente em `ApiUsageDaily`, através de 3 etapas (justificativa de cada decisão em [05-decisions.md](./05-decisions.md)):

1. **Ingest**: o Gateway envia telemetria ao OpenTelemetry Collector a cada request, assíncrono, fora do caminho crítico da resposta.
2. **Meter**: um **job em background** (ex.: a cada hora ou 1x por dia) lê os dados do Collector/Prometheus e faz upsert de uma linha resumida por dia/Organization/Application/Endpoint em `ApiUsageDaily`.
3. **Invoice/Consulta**: o Portal e qualquer relatório de billing consultam apenas `ApiUsageDaily` — pronta, sem somar eventos brutos na hora.

O Postgres nunca é escrito durante o processamento da requisição — só recebe as escritas pequenas e periódicas do job. Isso isola o caminho de billing do caminho rápido do Gateway.

---

**Anterior:** [03-auth.md](./03-auth.md) · **Próximo:** [05-decisions.md](./05-decisions.md)