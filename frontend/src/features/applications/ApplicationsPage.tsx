import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Plus } from "lucide-react";
import { Link } from "react-router";
import { Button, Card, Dialog, DialogActions, EmptyState, ErrorState, Input, LoadingState, PageHeader, Select, StatusBadge } from "../../shared/components/ui";
import { applicationTypeLabels, errorMessage, formatApplicationType, formatDate } from "../../shared/format";
import { applicationsQuery, createApplication } from "./applicationsApi";

export function ApplicationsPage() {
  const queryClient = useQueryClient();
  const applications = useQuery(applicationsQuery);
  const [createOpen, setCreateOpen] = useState(false);
  const [name, setName] = useState("");
  const [type, setType] = useState(0);

  const createMutation = useMutation({
    mutationFn: createApplication,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: applicationsQuery.queryKey });
      setCreateOpen(false);
    },
  });

  function openCreateDialog() {
    setName("");
    setType(0);
    createMutation.reset();
    setCreateOpen(true);
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    createMutation.mutate({ name, type });
  }

  return (
    <div className="space-y-8">
      <PageHeader
        title="Applications"
        description="Manage the systems that consume your organization's APIs."
        action={<Button onClick={openCreateDialog}><Plus className="size-4" /> Create application</Button>}
      />

      {applications.isPending && <LoadingState />}
      {applications.isError && <ErrorState error={applications.error} />}
      {applications.data && (
        <Card className="overflow-hidden">
          {applications.data.length === 0 ? (
            <EmptyState title="No applications yet" description="Create your first application to start issuing API keys." />
          ) : (
            <div className="divide-y divide-plum-100">
              {applications.data.map((application) => (
                <Link key={application.id} to={`/applications/${application.id}`} className="group flex items-center justify-between gap-4 p-5 transition hover:bg-canvas/60 sm:px-6">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="truncate font-semibold text-plum-900">{application.name}</p>
                      <StatusBadge active={application.isActive} />
                    </div>
                    <p className="mt-1 text-sm text-plum-500">{formatApplicationType(application.type)} · Created {formatDate(application.createdAt)}</p>
                  </div>
                  <ArrowRight className="size-5 shrink-0 text-plum-300 transition group-hover:translate-x-1 group-hover:text-brand-600" />
                </Link>
              ))}
            </div>
          )}
        </Card>
      )}

      <Dialog open={createOpen} title="Create application" description="Use a name that clearly identifies the system consuming your APIs." onClose={() => setCreateOpen(false)}>
        <form onSubmit={handleSubmit}>
          <div className="space-y-5">
            <label className="block text-sm font-medium text-plum-700">
              Name
              <Input className="mt-2" required maxLength={120} value={name} onChange={(event) => setName(event.target.value)} placeholder="e.g. Production ERP" autoFocus />
            </label>
            <label className="block text-sm font-medium text-plum-700">
              Type
              <Select className="mt-2" value={type} onChange={(event) => setType(Number(event.target.value))}>
                {applicationTypeLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}
              </Select>
            </label>
          </div>
          {createMutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(createMutation.error)}</p>}
          <DialogActions>
            <Button type="button" variant="ghost" onClick={() => setCreateOpen(false)}>Cancel</Button>
            <Button type="submit" disabled={createMutation.isPending}>{createMutation.isPending ? "Creating…" : "Create application"}</Button>
          </DialogActions>
        </form>
      </Dialog>
    </div>
  );
}
