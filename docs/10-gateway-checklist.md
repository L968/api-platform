# Checklist — API Gateway

Checklist simples para retomada do Gateway.

## Concluído

- [x] Autenticar por `X-Api-Key: ClientId.Secret`
- [x] Buscar Credential pelo `ClientId` indexado
- [x] Validar segredo PBKDF2 em tempo constante
- [x] Bloquear chave inválida, expirada ou revogada
- [x] Bloquear Application desativada e Organization inativa
- [x] Cache curto de autenticação sem guardar segredo em texto puro
- [x] Projetar Organization, Application, Credential e Scopes em Claims
- [x] Exigir scopes de leitura/escrita por rota e método
- [x] Remover `X-Api-Key` antes de encaminhar a requisição
- [x] Rate limiting configurável por Application
- [x] Rotear Orders e Payments via YARP
- [x] Enfileirar consumo fora do caminho crítico
- [x] Agregar consumo e fazer upsert periódico em `ApiUsageDaily`
- [x] Normalizar endpoints para evitar alta cardinalidade
- [x] Usar rate limiting nativo do ASP.NET Core, particionado por Application
- [x] Usar metadata, policies, transforms e timeouts nativos do YARP
- [x] Consultar autenticação com Npgsql direto, sem modelo EF duplicado
- [x] Manter o código em 7 arquivos C# de produção
- [x] Testes unitários de parsing, hash e normalização de endpoint
- [x] Smoke test real com Portal, Gateway, Orders, Payments e PostgreSQL

## Próximas evoluções

- [ ] Exportar traces e métricas via OpenTelemetry Collector
- [ ] Adicionar Prometheus e Grafana
- [ ] Usar ingest durável quando houver múltiplas instâncias
- [ ] Usar cache distribuído/invalidação imediata quando houver múltiplas instâncias
- [ ] Executar teste de carga e definir limites por plano
- [ ] Configurar rede interna e HTTPS para produção
