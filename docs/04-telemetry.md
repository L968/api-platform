# 04. Telemetria e Consumo

## 1. O que é medido

Para cada requisição autorizada e encaminhada pelo Gateway:

```text
organization.id
application.id
api.id
endpoint normalizado
latência
status de erro
```

## 2. Pipeline atual

O caminho foi mantido simples e fora da resposta ao cliente:

1. o middleware coloca um evento em uma fila limitada em memória;
2. um `BackgroundService` agrega os eventos;
3. a cada intervalo, o lote é persistido por upsert em `api_usage_daily`;
4. o Portal consulta essa tabela para consumo e billing.

O endpoint é normalizado para evitar alta cardinalidade. Por exemplo, `/orders/123` vira `GET /orders/{id}`.

Se o banco falhar, o lote agregado permanece em memória para nova tentativa. Se a fila lotar, eventos novos são descartados e um warning é registrado, protegendo o caminho crítico e a memória do Gateway.

## 3. Evolução somente quando necessária

O worker em processo atende ao MVP de instância única. Quando houver múltiplas instâncias ou a perda de eventos em uma queda de processo for inaceitável, `IUsageSink` poderá publicar em um ingest durável. OpenTelemetry, Prometheus e Grafana continuam como evoluções separadas de observabilidade.

---

**Anterior:** [03-auth.md](./03-auth.md) · **Próximo:** [05-decisions.md](./05-decisions.md)
