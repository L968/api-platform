# 00. Overview

O objetivo principal desse projeto é servir como portfólio para um desenvolvedor senior backend de .NET + ReactJS. Ele precisa ser extremamente próximo de um sistema production-ready, para mostrar a experiência do desenvolvedor.

Não deve haver dependências externas, como chaves de Azure ou Twilio — a aplicação deve rodar completa com `docker-compose up` e nada mais. Isso garante que um recrutador consiga ver o sistema funcionando sem precisar configurar nada.

O sistema deve ser extensível e modular nos pontos que fazem sentido para esse estágio do projeto (ver `05-decisions.md`).

---

## 1. Objetivo do Projeto

### 1.1 O que é esse sistema, em termos simples

É um modelo **B2B** (empresa para empresa): quem opera a plataforma é a equipe operadora, e quem consome é o cliente (outra empresa) — não existe usuário final/consumidor individual (B2C) neste projeto.

Existem dois papéis bem distintos, e é importante não misturá-los:

- **A equipe operadora** define **quais APIs existem** na plataforma — por exemplo, uma API de pedidos (Orders) e uma de pagamentos (Payments), parecido com o que a Stripe faz para pagamentos. Isso é o catálogo de produto: o cliente não cria uma API nova, ele só consome o que já existe.
- **A empresa-cliente** (ex.: "Acme Corp") cadastra os **sistemas dela** que vão consumir essas APIs — cada sistema é uma **Application** (ex.: o ERP da Acme, ou um job que roda à noite). Isso é decisão do cliente, não da operadora: só ele sabe quais sistemas internos ele tem.

O fluxo da empresa-cliente, então, é:

1. Ter uma conta na plataforma — isso é uma **Organization** (criada pela operadora, ver seção 1.2 a seguir).
2. Cadastrar suas próprias Applications, dentro das APIs que a plataforma já oferece.
3. Gerar uma **chave de API** (API Key) para cada Application, para que ela consiga se autenticar e chamar as APIs.
4. Acessar um **Developer Portal** (painel web) para gerenciar tudo isso — ver suas Applications, gerar/revogar chaves, ver quanto consumiu.

Todas essas chamadas passam por um **Gateway** central, que decide o que aceitar ou rejeitar antes de chegar nas APIs de negócio. As APIs de negócio (Orders, Payments) em si são bem simples — propositalmente, pois o foco do projeto não é a regra de negócio delas, e sim toda essa infraestrutura de plataforma em volta. Detalhamento de como o Gateway funciona em [01-architecture.md](./01-architecture.md).

Em resumo: é uma versão simplificada de algo como a Stripe — não para processar pagamentos de verdade, mas para demonstrar como se constrói a infraestrutura por trás de uma plataforma de APIs B2B (autenticação, autorização, multi-tenant, observabilidade e billing-ready).

> **Importante — duas camadas de autenticação distintas:**
> 1. **Portal Login** (humano): acesso da empresa-cliente ao Developer Portal. Usuário/senha simples; o primeiro PortalUser de cada Organization é criado manualmente pela equipe operadora (via script/seed no banco) — é o equivalente a "abrir a conta". A partir daí, o próprio PortalUser opera de forma autônoma.
> 2. **API Auth** (Application): autenticação das chamadas às APIs de negócio (Orders, Payments). Feita exclusivamente via **API Key**, vinculada a uma Application, com Scopes e rate limiting via Gateway. Applications, Credentials e seus Scopes são geridos **self-service pelo próprio PortalUser** no Developer Portal — a equipe operadora só mantém o catálogo de Scopes disponíveis (igual a Stripe: o cliente gera suas próprias "restricted keys").
>
> Essas duas camadas não se misturam. O conceito de "Principal" (User + Application autenticando na mesma API) foi descartado por ora — a API só conhece Applications.

### 1.2 Objetivos técnicos

Construir uma **API Platform estilo Stripe / API Management simplificado**, com foco em:

- Multi-tenancy (Organizations)
- Applications como único tipo de cliente das APIs de negócio
- Gateway centralizado (YARP)
- Auth via API Key para as APIs de negócio
- Login simples (usuário/senha) para o Developer Portal
- Telemetria e consumo por cliente
- Arquitetura extensível nos pontos relevantes
- Observabilidade (Prometheus + Grafana + OpenTelemetry)

---

**Próximo:** [01-architecture.md](./01-architecture.md)