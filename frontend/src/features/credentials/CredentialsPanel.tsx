import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Copy, KeyRound, Plus, ShieldAlert } from "lucide-react";
import { Button, Card, Dialog, DialogActions, EmptyState, ErrorState, Input, LoadingState, Select, StatusBadge } from "../../shared/components/ui";
import { errorMessage, formatDate } from "../../shared/format";
import { createCredential, credentialsQuery, revokeCredential, scopesQuery, type CreatedCredential, type Credential } from "./credentialsApi";

const expirationOptions = [
  { value: "30", label: "30 days" },
  { value: "90", label: "90 days" },
  { value: "365", label: "1 year" },
  { value: "never", label: "Never expires" },
] as const;

export function CredentialsPanel({ applicationId, applicationActive }: { applicationId: string; applicationActive: boolean }) {
  const queryClient = useQueryClient();
  const credentials = useQuery(credentialsQuery);
  const scopes = useQuery(scopesQuery);
  const [createOpen, setCreateOpen] = useState(false);
  const [name, setName] = useState("");
  const [expiration, setExpiration] = useState("90");
  const [selectedScopes, setSelectedScopes] = useState<string[]>([]);
  const [createdCredential, setCreatedCredential] = useState<CreatedCredential | null>(null);
  const [credentialToRevoke, setCredentialToRevoke] = useState<Credential | null>(null);
  const [copied, setCopied] = useState(false);

  const applicationCredentials = useMemo(
    () => credentials.data?.filter((credential) => credential.applicationId === applicationId) ?? [],
    [applicationId, credentials.data],
  );

  const createMutation = useMutation({
    mutationFn: () => createCredential(applicationId, {
      name,
      expiresAt: expirationDate(expiration),
      scopeIds: selectedScopes,
    }),
    onSuccess: async (credential) => {
      setCreatedCredential(credential);
      await queryClient.invalidateQueries({ queryKey: credentialsQuery.queryKey });
    },
  });

  const revokeMutation = useMutation({
    mutationFn: revokeCredential,
    onSuccess: async () => {
      setCredentialToRevoke(null);
      await queryClient.invalidateQueries({ queryKey: credentialsQuery.queryKey });
    },
  });

  function openCreateDialog() {
    setName("");
    setExpiration("90");
    setSelectedScopes([]);
    setCreatedCredential(null);
    setCopied(false);
    createMutation.reset();
    setCreateOpen(true);
  }

  function closeCreateDialog() {
    if (!createMutation.isPending) {
      setCreateOpen(false);
    }
  }

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
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600">Access</p>
          <h2 className="mt-2 font-display text-3xl font-semibold text-plum-950">API keys</h2>
          <p className="mt-1 text-sm text-plum-500">Credentials issued for this application.</p>
        </div>
        {applicationActive && <Button onClick={openCreateDialog}><Plus className="size-4" /> Create API key</Button>}
      </div>

      {!applicationActive && <p className="rounded-2xl border border-brand-100 bg-brand-50 p-4 text-sm text-brand-700">Reactivate the application before creating a new key.</p>}

      {credentials.isPending && <LoadingState />}
      {credentials.isError && <ErrorState error={credentials.error} />}
      {credentials.data && (
        <Card className="overflow-hidden">
          {applicationCredentials.length === 0 ? (
            <EmptyState title="No API keys yet" description="Create a key when this application is ready to call an API." />
          ) : (
            <div className="divide-y divide-plum-100">
              {applicationCredentials.map((credential) => (
                <div key={credential.id} className="flex flex-col justify-between gap-4 p-5 transition hover:bg-canvas/50 sm:flex-row sm:items-center sm:px-6">
                  <div className="flex min-w-0 items-start gap-4">
                    <span className="mt-0.5 grid size-10 shrink-0 place-items-center rounded-2xl bg-plum-50 text-plum-600"><KeyRound className="size-4" /></span>
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-semibold text-plum-900">{credential.name}</p>
                        <StatusBadge active={credential.isActive && applicationActive} activeLabel="Valid" inactiveLabel={credential.isActive ? "Application inactive" : "Revoked or expired"} />
                      </div>
                      <code className="mt-1 block truncate text-sm text-plum-500">{credential.clientId}</code>
                      <p className="mt-1 text-xs text-plum-400">Created {formatDate(credential.createdAt)} · {credential.expiresAt ? `Expires ${formatDate(credential.expiresAt)}` : "Never expires"}</p>
                    </div>
                  </div>
                  {credential.isActive && (
                    <Button variant="ghost" disabled={revokeMutation.isPending} onClick={() => {
                      revokeMutation.reset();
                      setCredentialToRevoke(credential);
                    }}>Revoke</Button>
                  )}
                </div>
              ))}
            </div>
          )}
        </Card>
      )}
      {revokeMutation.isError && !credentialToRevoke && <ErrorState error={revokeMutation.error} />}

      <Dialog open={createOpen} title={createdCredential ? "Your API key is ready" : "Create API key"} description={createdCredential ? "Copy it now. For security, the full key won't be shown again." : "Choose a clear name, expiration period and the minimum required scopes."} onClose={closeCreateDialog}>
        {createdCredential ? (
          <div>
            <div className="rounded-2xl border border-sage-100 bg-sage-100/50 p-4">
              <div className="flex items-center gap-2 text-sm font-semibold text-sage-800"><Check className="size-4" /> Key created successfully</div>
              <code className="mt-4 block overflow-x-auto rounded-xl bg-paper p-3 text-sm text-plum-800 ring-1 ring-plum-100">{createdCredential.apiKey}</code>
            </div>
            <DialogActions>
              <Button type="button" variant="ghost" onClick={closeCreateDialog}>Done</Button>
              <Button type="button" onClick={copyApiKey}>{copied ? <Check className="size-4" /> : <Copy className="size-4" />}{copied ? "Copied" : "Copy API key"}</Button>
            </DialogActions>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="space-y-5">
              <label className="block text-sm font-medium text-plum-700">
                Key name
                <Input className="mt-2" required maxLength={120} value={name} onChange={(event) => setName(event.target.value)} placeholder="e.g. Production backend" autoFocus />
              </label>
              <label className="block text-sm font-medium text-plum-700">
                Expiration
                <Select className="mt-2" value={expiration} onChange={(event) => setExpiration(event.target.value)}>
                  {expirationOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                </Select>
              </label>
              <fieldset>
                <legend className="text-sm font-medium text-plum-700">Scopes</legend>
                <p className="mt-1 text-xs text-plum-400">Select at least one scope and grant only what this integration needs.</p>
                <div className="mt-3 grid gap-2 sm:grid-cols-2">
                  {scopes.data?.map((scope) => (
                    <label key={scope.id} className="flex items-center gap-3 rounded-xl border border-plum-100 p-3 text-sm text-plum-700 transition hover:bg-plum-50">
                      <input type="checkbox" className="size-4 accent-brand-600" checked={selectedScopes.includes(scope.id)} onChange={() => toggleScope(scope.id)} />
                      <code>{scope.name}</code>
                    </label>
                  ))}
                </div>
              </fieldset>
            </div>
            {scopes.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(scopes.error)}</p>}
            {createMutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(createMutation.error)}</p>}
            {selectedScopes.length === 0 && <p className="mt-4 text-sm text-plum-500">Choose at least one scope to create this key.</p>}
            <DialogActions>
              <Button type="button" variant="ghost" onClick={closeCreateDialog}>Cancel</Button>
              <Button type="submit" disabled={createMutation.isPending || scopes.isPending || scopes.isError || selectedScopes.length === 0}>{createMutation.isPending ? "Creating…" : "Create API key"}</Button>
            </DialogActions>
          </form>
        )}
      </Dialog>

      <Dialog open={Boolean(credentialToRevoke)} title="Revoke API key?" description={`“${credentialToRevoke?.name ?? ""}” will stop working immediately. This action cannot be undone.`} onClose={() => setCredentialToRevoke(null)}>
        <div className="flex gap-3 rounded-2xl bg-danger-50 p-4 text-sm text-danger-700">
          <ShieldAlert className="mt-0.5 size-5 shrink-0" /> Any service using this key will lose access to the APIs.
        </div>
        {revokeMutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(revokeMutation.error)}</p>}
        <DialogActions>
          <Button type="button" variant="ghost" onClick={() => setCredentialToRevoke(null)}>Cancel</Button>
          <Button type="button" variant="danger" disabled={revokeMutation.isPending} onClick={() => credentialToRevoke && revokeMutation.mutate(credentialToRevoke.id)}>
            {revokeMutation.isPending ? "Revoking…" : "Revoke key"}
          </Button>
        </DialogActions>
      </Dialog>
    </section>
  );
}

function expirationDate(expiration: string): string | null {
  if (expiration === "never") {
    return null;
  }

  const date = new Date();
  date.setUTCDate(date.getUTCDate() + Number(expiration));
  return date.toISOString();
}
