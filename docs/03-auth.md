# 03. Autenticação

## 1. API Key no Gateway

O cliente envia uma única chave:

```http
X-Api-Key: app_identificador.sk_segredo
```

O `ClientId` público vem embutido na chave e permite localizar a Credential por um índice do banco. O segredo é validado em tempo constante contra o hash PBKDF2 armazenado. O header é removido antes de a requisição chegar à API de negócio.

O Authentication Handler chama o `ApiKeyValidator`, que:

1. separa `ClientId` e segredo;
2. consulta Credential, Application, Organization e Scopes em uma única query;
3. verifica expiração, revogação e status;
4. cria Claims usadas pelas policies do YARP.

Credenciais válidas ficam em cache de memória por 30 segundos. A chave do cache é o SHA-256 da API Key completa, portanto o segredo em texto puro não é guardado. Revogações podem levar até esse TTL para aparecer em cada instância; os tempos são configuráveis em `ApiKeyCache`.

## 2. Application como cliente

Todo chamador autenticado é uma Application. Internamente, o Gateway resolve:

```text
OrganizationId
ApplicationId
CredentialId
Scopes = [orders.read, ...]
```

As APIs de Orders e Payments não validam chaves e não acessam esse contexto. Elas recebem somente requisições já autenticadas e autorizadas pelo Gateway.

---

**Anterior:** [02-domain-model.md](./02-domain-model.md) · **Próximo:** [04-telemetry.md](./04-telemetry.md)
