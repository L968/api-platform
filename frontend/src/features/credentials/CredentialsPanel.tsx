import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Copy, KeyRound, Plus, X } from "lucide-react";
import { Button, Card, EmptyState, ErrorState, Input, LoadingState, StatusBadge } from "../../shared/components/ui";
import { errorMessage, formatDate } from "../../shared/format";
import { createCredential, credentialsQuery, revokeCredential, scopesQuery, type CreatedCredential } from "./credentialsApi";

export function CredentialsPanel({ applicationId, applicationActive }: { applicationId: string; applicationActive: boolean }) {
  const queryClient = useQueryClient();
  const credentials = useQuery(credentialsQuery);
  const scopes = useQuery(scopesQuery);
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");
  const [expiresAt, setExpiresAt] = useState("");
  const [selectedScopes, setSelectedScopes] = useState<string[]>([]);
  const [createdCredential, setCreatedCredential] = useState<CreatedCredential | null>(null);
  const [copied, setCopied] = useState(false);

  const applicationCredentials = useMemo(
    () => credentials.data?.filter((credential) => credential.applicationId === applicationId) ?? [],
    [applicationId, credentials.data],
  );

  const createMutation = useMutation({
    mutationFn: () => createCredential(applicationId, {
      name,
      expiresAt: expiresAt ? new Date(`${expiresAt}T23:59:59`).toISOString() : null,
      scopeIds: selectedScopes,
    }),
    onSuccess: async (credential) => {
      setCreatedCredential(credential);
      setShowForm(false);
      setName("");
      setExpiresAt("");
      setSelectedScopes([]);
      await queryClient.invalidateQueries({ queryKey: credentialsQuery.queryKey });
    },
  });

  const revokeMutation = useMutation({
    mutationFn: revokeCredential,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: credentialsQuery.queryKey }),
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createMutation.mutate();
  }

  function toggleScope(scopeId: string) {
    setSelectedScopes((current) => current.includes(scopeId)
      ? current.filter((id) => id !== scopeId)
      : [...current, scopeId]);
  }

  async function copyApiKey() {
    if (!createdCredential) {
      return;
    }

    await navigator.clipboard.writeText(createdCredential.apiKey);
    setCopied(true);
  }

  return (
    <section className="space-y-5">
      <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
        <div>
          <h2 className="text-xl font-bold text-slate-950">API Keys</h2>
          <p className="mt-1 text-sm text-slate-500">Credenciais usadas por esta Application.</p>
        </div>
        {applicationActive && <Button onClick={() => setShowForm((current) => !current)}>{showForm ? <X className="size-4" /> : <Plus className="size-4" />}{showForm ? "Cancelar" : "Nova API Key"}</Button>}
      </div>

      {!applicationActive && <p className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">Reative a Application para criar novas chaves.</p>}

      {createdCredential && (
        <Card className="border-brand-100 bg-brand-50 p-5">
          <div className="flex items-start gap-3">
            <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-white text-brand-600"><KeyRound className="size-5" /></span>
            <div className="min-w-0 flex-1">
              <h3 className="font-semibold text-slate-950">Copie sua API Key agora</h3>
              <p className="mt-1 text-sm text-slate-600">Por segurança, ela não será exibida novamente.</p>
              <code className="mt-4 block overflow-x-auto rounded-lg border border-brand-100 bg-white p-3 text-sm text-slate-800">{createdCredential.apiKey}</code>
              <div className="mt-3 flex gap-2">
                <Button type="button" onClick={copyApiKey}>{copied ? <Check className="size-4" /> : <Copy className="size-4" />}{copied ? "Copiada" : "Copiar chave"}</Button>
                <Button type="button" variant="ghost" onClick={() => setCreatedCredential(null)}>Fechar</Button>
              </div>
            </div>
          </div>
        </Card>
      )}

      {showForm && (
        <Card className="p-6">
          <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="text-sm font-medium text-slate-700">Nome<Input className="mt-2" required value={name} onChange={(event) => setName(event.target.value)} placeholder="Ex.: Produção" /></label>
              <label className="text-sm font-medium text-slate-700">Expiração opcional<Input className="mt-2" type="date" min={new Date().toISOString().slice(0, 10)} value={expiresAt} onChange={(event) => setExpiresAt(event.target.value)} /></label>
            </div>
            <fieldset>
              <legend className="text-sm font-medium text-slate-700">Scopes</legend>
              <div className="mt-3 grid gap-2 sm:grid-cols-2">
                {scopes.data?.map((scope) => (
                  <label key={scope.id} className="flex items-center gap-3 rounded-lg border border-slate-200 p-3 text-sm text-slate-700 hover:bg-slate-50">
                    <input type="checkbox" className="size-4 accent-brand-600" checked={selectedScopes.includes(scope.id)} onChange={() => toggleScope(scope.id)} />
                    <code>{scope.name}</code>
                  </label>
                ))}
              </div>
            </fieldset>
            {createMutation.isError && <p className="text-sm text-red-700" role="alert">{errorMessage(createMutation.error)}</p>}
            <Button type="submit" disabled={createMutation.isPending || scopes.isPending}>{createMutation.isPending ? "Criando…" : "Criar API Key"}</Button>
          </form>
        </Card>
      )}

      {credentials.isPending && <LoadingState />}
      {credentials.isError && <ErrorState error={credentials.error} />}
      {credentials.data && (
        <Card className="overflow-hidden">
          {applicationCredentials.length === 0 ? (
            <EmptyState title="Nenhuma API Key" description="Crie uma chave para autenticar esta Application." />
          ) : (
            <div className="divide-y divide-slate-100">
              {applicationCredentials.map((credential) => (
                <div key={credential.id} className="flex flex-col justify-between gap-4 p-5 sm:flex-row sm:items-center sm:px-6">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2"><p className="font-semibold text-slate-900">{credential.name}</p><StatusBadge active={credential.isActive && applicationActive} activeLabel="Válida" inactiveLabel={credential.isActive ? "Application inativa" : "Revogada/expirada"} /></div>
                    <code className="mt-1 block truncate text-sm text-slate-500">{credential.clientId}</code>
                    <p className="mt-1 text-xs text-slate-400">Criada em {formatDate(credential.createdAt)} · {credential.expiresAt ? `Expira em ${formatDate(credential.expiresAt)}` : "Sem expiração"}</p>
                  </div>
                  {credential.isActive && <Button variant="danger" disabled={revokeMutation.isPending} onClick={() => window.confirm("Revogar esta API Key? Essa ação não pode ser desfeita.") && revokeMutation.mutate(credential.id)}>Revogar</Button>}
                </div>
              ))}
            </div>
          )}
        </Card>
      )}
      {revokeMutation.isError && <ErrorState error={revokeMutation.error} />}
    </section>
  );
}
