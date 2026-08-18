import type { FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Save } from "lucide-react";
import { Button, Card, ErrorState, Input, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { errorMessage, formatDate } from "../../shared/format";
import { currentUserQuery } from "../auth/authApi";
import { organizationQuery, updateOrganization } from "./organizationApi";

export function OrganizationPage() {
  const queryClient = useQueryClient();
  const organization = useQuery(organizationQuery);
  const mutation = useMutation({
    mutationFn: updateOrganization,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: organizationQuery.queryKey }),
        queryClient.invalidateQueries({ queryKey: currentUserQuery.queryKey }),
      ]);
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    mutation.mutate(String(data.get("name")));
  }

  return (
    <div className="space-y-8">
      <PageHeader title="Organization" description="Dados da empresa vinculada à sua conta." />
      {organization.isPending && <LoadingState />}
      {organization.isError && <ErrorState error={organization.error} />}
      {organization.data && (
        <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
          <Card className="p-6">
            <h2 className="text-lg font-semibold text-slate-950">Dados gerais</h2>
            <form key={organization.data.name} className="mt-6 space-y-5" onSubmit={handleSubmit}>
              <label className="block text-sm font-medium text-slate-700">Nome da Organization<Input className="mt-2" name="name" required defaultValue={organization.data.name} /></label>
              {mutation.isError && <p className="text-sm text-red-700" role="alert">{errorMessage(mutation.error)}</p>}
              <Button type="submit" disabled={mutation.isPending}><Save className="size-4" /> {mutation.isPending ? "Salvando…" : "Salvar alterações"}</Button>
            </form>
          </Card>
          <Card className="p-6">
            <span className="grid size-11 place-items-center rounded-xl bg-brand-50 text-brand-600"><Building2 className="size-5" /></span>
            <dl className="mt-6 space-y-5 text-sm">
              <div><dt className="text-slate-500">Status</dt><dd className="mt-1"><StatusBadge active={organization.data.status === 0} activeLabel="Ativa" inactiveLabel="Inativa" /></dd></div>
              <div><dt className="text-slate-500">Criada em</dt><dd className="mt-1 font-medium text-slate-900">{formatDate(organization.data.createdAt)}</dd></div>
              <div><dt className="text-slate-500">ID</dt><dd className="mt-1 break-all font-mono text-xs text-slate-700">{organization.data.id}</dd></div>
            </dl>
          </Card>
        </div>
      )}
    </div>
  );
}
