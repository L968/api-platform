# Checklist — Portal API

Checklist de retomada para concluir o backend do Developer Portal.

Legenda: `[x]` concluído · `[ ]` pendente

## Estado atual

- [x] Projeto ASP.NET Core criado
- [x] PostgreSQL configurado via Entity Framework Core
- [x] Entidades de domínio criadas
- [x] Configurações do Entity Framework criadas
- [x] Migration inicial criada
- [x] Swagger configurado em Development
- [x] CORS configurado para o frontend local
- [x] Endpoint `/health` criado
- [x] Casos de uso e endpoints do Portal implementados

## 1. Base técnica

- [ ] Corrigir os erros atuais de compilação nos mocks de Orders e Payments
- [x] Gerar migration para o modelo atual do Portal
- [x] Criar seed SQL de APIs, Scopes, Organization, PortalUser e preços
- [ ] Definir tratamento global de erros
- [x] Adicionar validações básicas de entrada
- [x] Usar logging padrão estruturado do ASP.NET Core

## 2. Autenticação do PortalUser

- [x] Criar endpoint de login por email e senha
- [x] Validar usuário ativo e Organization ativa
- [x] Armazenar senhas somente com hash PBKDF2
- [x] Emitir sessão por cookie HttpOnly
- [x] Proteger endpoints com autenticação
- [x] Garantir isolamento por `OrganizationId`
- [x] Criar endpoint de logout e expiração da sessão

## 3. Organizations

- [x] Criar o primeiro PortalUser junto com a Organization via seed SQL
- [x] Consultar a Organization do usuário autenticado
- [x] Atualizar o nome da Organization
- [ ] Suspender, desativar e reativar Organization via endpoint de operador

## 4. Applications

- [x] Criar Application para a Organization autenticada
- [x] Listar Applications da própria Organization
- [x] Consultar Application por id
- [x] Renomear Application
- [x] Alterar tipo da Application
- [x] Desativar e reativar Application
- [x] Impedir acesso a Applications de outra Organization

## 5. Credentials / API Keys

- [x] Gerar `ClientId` e segredo aleatório
- [x] Armazenar somente o hash do segredo
- [x] Mostrar o segredo em texto puro somente na criação
- [x] Criar Credential vinculada a uma Application
- [x] Permitir definir `ExpiresAt`
- [x] Listar Credentials sem expor o segredo
- [x] Revogar Credential
- [x] Impedir criação de Credential para Application de outro tenant
- [x] Registrar nome/descrição da Credential

## 6. Scopes

- [x] Criar seed dos Scopes disponíveis
- [x] Listar Scopes disponíveis para o PortalUser
- [x] Permitir selecionar Scopes ao criar Credential
- [x] Persistir relações em `CredentialScope`
- [x] Retornar os Scopes na criação da Credential

## 7. Consumo e billing

- [x] Consultar consumo por Organization/Application
- [x] Consultar consumo por API e endpoint
- [x] Exibir erros e latência agregada
- [x] Consultar preços por Organization/API
- [x] Calcular valor devido no período
- [x] Expor resumo de billing para o Portal

## 8. Integração e segurança

- [ ] Criar contratos de resposta consistentes
- [ ] Adicionar paginação nas listagens
- [ ] Adicionar testes unitários do domínio
- [ ] Adicionar testes de integração dos endpoints
- [ ] Testar isolamento entre Organizations
- [ ] Configurar HTTPS/autenticação adequadamente fora de Development
- [x] Endpoints ficam disponíveis no Swagger em Development

## Ordem recomendada

1. Corrigir compilação e configurar migration/seed.
2. Implementar login e isolamento por Organization.
3. Implementar Applications.
4. Implementar Credentials e Scopes.
5. Integrar o Gateway com a validação das API Keys.
6. Implementar consumo e billing.
7. Adicionar testes e endurecer segurança.

## Fora do Portal API

Estas tarefas são necessárias para o produto completo, mas pertencem a outros componentes:

- [ ] Implementar autenticação, autorização e rate limiting no Gateway
- [ ] Criar job de telemetria e agregação de consumo
- [ ] Adicionar OpenTelemetry, Collector, Prometheus e Grafana
- [ ] Criar Developer Portal em Next.js
- [ ] Completar `docker-compose.yml` com todos os serviços

## Validação desta retomada

- [x] Solução completa compila com `dotnet build backend/ApiPlatform.slnx --no-restore`
- [ ] Aplicar migration e executar seed contra PostgreSQL
- [ ] Testar login e endpoints com PostgreSQL real

> A validação com Docker ficou pendente porque o Docker Desktop não estava disponível no ambiente durante esta execução.
