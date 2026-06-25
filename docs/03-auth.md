# 03. Autenticação

## 1. Autenticação via API Key (implementada no Gateway)

Interface principal, implementada **dentro do Gateway** (como Authentication Handler do ASP.NET Core):

```csharp
public interface IApiKeyValidator
{
    Task<ApplicationContext?> ValidateAsync(HttpRequest request);
}
```

Única implementação por ora:

- ApiKeyValidator (default)

O resultado (`ApplicationContext`) é projetado em Claims, usadas pelas Authorization Policies do Gateway para checar Scopes por rota antes do roteamento via YARP. As APIs de negócio nunca implementam essa interface nem têm acesso a ela.

> A plugabilidade de provider (Entra ID, Keycloak, etc.) foi removida do escopo das APIs de negócio. Caso volte a fazer sentido no futuro, será reavaliada (ver [05-decisions.md](./05-decisions.md)).

---

## 2. Applications como único cliente das APIs

O Gateway não precisa diferenciar tipos de chamador — todo chamador autenticado é uma Application. As APIs de negócio, por sua vez, não enxergam esse contexto algum — apenas recebem a requisição já validada e autorizada pelo Gateway.

O Gateway resolve, para uso interno seu (Claims/Policy):

```
ApplicationContext
```

Exemplo:

```
ApplicationId = erp-xpto
OrganizationId = org-acme
Scopes = [orders.read]
```

---

**Anterior:** [02-domain-model.md](./02-domain-model.md) · **Próximo:** [04-telemetry.md](./04-telemetry.md)
