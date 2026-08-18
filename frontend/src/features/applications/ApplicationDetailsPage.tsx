import type { FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Power, Save } from "lucide-react";
import { Link, useParams } from "react-router";
import { CredentialsPanel } from "../credentials/CredentialsPanel";
import { Button, Card, ErrorState, Input, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { applicationTypeLabels, errorMessage, formatDate } from "../../shared/format";
import { applicationQuery, applicationsQuery, setApplicationActive, updateApplication, type ApplicationRequest } from "./applicationsApi";

export function ApplicationDetailsPage() {
  const { applicationId = "" } = useParams();
  const queryClient = useQueryClient();
  const application = useQuery(applicationQuery(applicationId));
  async function refreshApplication() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["applications", applicationId] }),
      queryClient.invalidateQueries({ queryKey: applicationsQuery.queryKey }),
    ]);
  }

  const updateMutation = useMutation({
    mutationFn: (request: ApplicationRequest) => updateApplication(applicationId, request),
    onSuccess: refreshApplication,
  });
  const statusMutation = useMutation({
    mutationFn: (active: boolean) => setApplicationActive(applicationId, active),
    onSuccess: refreshApplication,
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    updateMutation.mutate({
      name: String(data.get("name")),
      type: Number(data.get("type")),
    });
  }

  if (application.isPending) {
    return <LoadingState />;
  }

  if (application.isError) {
    return <ErrorState error={application.error} />;
  }

  return (
    <div className="space-y-10">
      <Link to="/applications" className="inline-flex items-center gap-2 text-sm font-semibold text-slate-500 hover:text-slate-900"><ArrowLeft className="size-4" /> Voltar para Applications</Link>
      <PageHeader title={application.data.name} description={`Criada em ${formatDate(application.data.createdAt)}`} action={<StatusBadge active={application.data.isActive} />} />

      <Card className="p-6">
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
          <div>
            <h2 className="text-lg font-semibold text-slate-950">Configuração</h2>
            <p className="mt-1 text-sm text-slate-500">Identificação e estado desta integração.</p>
          </div>
          <Button
            variant={application.data.isActive ? "danger" : "secondary"}
            disabled={statusMutation.isPending}
            onClick={() => {
              const action = application.data.isActive ? "desativar" : "reativar";
              if (window.confirm(`Deseja ${action} esta Application?`)) {
                statusMutation.mutate(!application.data.isActive);
              }
            }}
          >
            <Power className="size-4" /> {application.data.isActive ? "Desativar" : "Reativar"}
          </Button>
        </div>
        <form key={application.data.updatedAt} className="mt-6 grid gap-4 sm:grid-cols-[1fr_220px_auto] sm:items-end" onSubmit={handleSubmit}>
          <label className="text-sm font-medium text-slate-700">Nome<Input className="mt-2" name="name" required defaultValue={application.data.name} /></label>
          <label className="text-sm font-medium text-slate-700">Tipo<select name="type" className="mt-2 min-h-11 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm" defaultValue={application.data.type}>{applicationTypeLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}</select></label>
          <Button type="submit" disabled={updateMutation.isPending}><Save className="size-4" /> Salvar</Button>
        </form>
        {(updateMutation.isError || statusMutation.isError) && <p className="mt-4 text-sm text-red-700" role="alert">{errorMessage(updateMutation.error ?? statusMutation.error)}</p>}
      </Card>

      <CredentialsPanel applicationId={applicationId} applicationActive={application.data.isActive} />
    </div>
  );
}
