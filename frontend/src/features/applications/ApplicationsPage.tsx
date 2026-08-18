import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Plus, X } from "lucide-react";
import { Link } from "react-router";
import { Button, Card, EmptyState, ErrorState, Input, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { applicationTypeLabels, errorMessage, formatApplicationType, formatDate } from "../../shared/format";
import { applicationsQuery, createApplication } from "./applicationsApi";

export function ApplicationsPage() {
  const queryClient = useQueryClient();
  const applications = useQuery(applicationsQuery);
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState("");
  const [type, setType] = useState(0);

  const createMutation = useMutation({
    mutationFn: createApplication,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: applicationsQuery.queryKey });
      setName("");
      setType(0);
      setShowForm(false);
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createMutation.mutate({ name, type });
  }

  return (
    <div className="space-y-8">
      <PageHeader
        title="Applications"
        description="Gerencie os sistemas que consomem as APIs da plataforma."
        action={<Button onClick={() => setShowForm((current) => !current)}>{showForm ? <X className="size-4" /> : <Plus className="size-4" />}{showForm ? "Cancelar" : "Nova Application"}</Button>}
      />

      {showForm && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold text-slate-950">Criar Application</h2>
          <p className="mt-1 text-sm text-slate-500">Use um nome que identifique claramente o sistema consumidor.</p>
          <form className="mt-5 grid gap-4 sm:grid-cols-[1fr_220px_auto] sm:items-end" onSubmit={handleSubmit}>
            <label className="text-sm font-medium text-slate-700">
              Nome
              <Input className="mt-2" required maxLength={120} value={name} onChange={(event) => setName(event.target.value)} placeholder="Ex.: ERP Produção" />
            </label>
            <label className="text-sm font-medium text-slate-700">
              Tipo
              <select className="mt-2 min-h-11 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm" value={type} onChange={(event) => setType(Number(event.target.value))}>
                {applicationTypeLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}
              </select>
            </label>
            <Button type="submit" disabled={createMutation.isPending}>{createMutation.isPending ? "Criando…" : "Criar"}</Button>
          </form>
          {createMutation.isError && <p className="mt-4 text-sm text-red-700" role="alert">{errorMessage(createMutation.error)}</p>}
        </Card>
      )}

      {applications.isPending && <LoadingState />}
      {applications.isError && <ErrorState error={applications.error} />}
      {applications.data && (
        <Card className="overflow-hidden">
          {applications.data.length === 0 ? (
            <EmptyState title="Nenhuma Application" description="Crie a primeira Application para gerar uma API Key." />
          ) : (
            <div className="divide-y divide-slate-100">
              {applications.data.map((application) => (
                <Link key={application.id} to={`/applications/${application.id}`} className="flex items-center justify-between gap-4 p-5 transition hover:bg-slate-50 sm:px-6">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="truncate font-semibold text-slate-900">{application.name}</p>
                      <StatusBadge active={application.isActive} />
                    </div>
                    <p className="mt-1 text-sm text-slate-500">{formatApplicationType(application.type)} · Criada em {formatDate(application.createdAt)}</p>
                  </div>
                  <ArrowRight className="size-5 shrink-0 text-slate-400" />
                </Link>
              ))}
            </div>
          )}
        </Card>
      )}
    </div>
  );
}
