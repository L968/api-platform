import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Edit3, Power, Trash2 } from "lucide-react";
import { Link, useNavigate, useParams } from "react-router";
import { CredentialsPanel } from "../credentials/CredentialsPanel";
import { Button, Dialog, DialogActions, ErrorState, Input, LoadingState, Select, StatusBadge } from "../../shared/components/ui";
import { applicationTypeLabels, errorMessage, formatApplicationType, formatDate } from "../../shared/format";
import { applicationQuery, applicationsQuery, deleteApplication, setApplicationActive, updateApplication, type ApplicationRequest } from "./applicationsApi";

export function ApplicationDetailsPage() {
  const { applicationId = "" } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const application = useQuery(applicationQuery(applicationId));
  const [editOpen, setEditOpen] = useState(false);
  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  async function refreshApplication() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["applications", applicationId] }),
      queryClient.invalidateQueries({ queryKey: applicationsQuery.queryKey }),
    ]);
  }

  const updateMutation = useMutation({
    mutationFn: (request: ApplicationRequest) => updateApplication(applicationId, request),
    onSuccess: async () => {
      await refreshApplication();
      setEditOpen(false);
    },
  });
  const statusMutation = useMutation({
    mutationFn: (active: boolean) => setApplicationActive(applicationId, active),
    onSuccess: async () => {
      await refreshApplication();
      setStatusDialogOpen(false);
    },
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteApplication(applicationId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: applicationsQuery.queryKey }),
        queryClient.invalidateQueries({ queryKey: ["credentials"] }),
      ]);
      navigate("/applications", { replace: true });
    },
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

  const item = application.data;

  return (
    <div className="space-y-8">
      <Link to="/applications" className="inline-flex items-center gap-2 text-sm font-semibold text-plum-500 transition hover:text-plum-950">
        <ArrowLeft className="size-4" /> Back to applications
      </Link>

      <header className="flex flex-col justify-between gap-5 border-b border-plum-200 pb-7 sm:flex-row sm:items-end">
        <div>
          <h1 className="font-display text-4xl font-semibold tracking-tight text-plum-950">{item.name}</h1>
          <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-2 text-sm text-plum-500">
            <span>{formatApplicationType(item.type)}</span>
            <span className="size-1 rounded-full bg-plum-300" />
            <span>Created {formatDate(item.createdAt)}</span>
            <StatusBadge active={item.isActive} />
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="secondary" onClick={() => {
            updateMutation.reset();
            setEditOpen(true);
          }}>
            <Edit3 className="size-4" /> Edit
          </Button>
          <Button variant="ghost" onClick={() => {
            statusMutation.reset();
            setStatusDialogOpen(true);
          }}>
            <Power className="size-4" /> {item.isActive ? "Disable" : "Reactivate"}
          </Button>
          <Button variant="ghost" className="text-danger-600" onClick={() => {
            deleteMutation.reset();
            setDeleteDialogOpen(true);
          }}>
            <Trash2 className="size-4" /> Delete
          </Button>
        </div>
      </header>

      <CredentialsPanel applicationId={applicationId} applicationActive={item.isActive} />

      <Dialog open={editOpen} title="Edit application" description="Update how this application is identified in the portal." onClose={() => setEditOpen(false)}>
        <form onSubmit={handleSubmit}>
          <div className="space-y-5">
            <label className="block text-sm font-medium text-plum-700">
              Name
              <Input className="mt-2" name="name" required maxLength={120} defaultValue={item.name} autoFocus />
            </label>
            <label className="block text-sm font-medium text-plum-700">
              Type
              <Select className="mt-2" name="type" defaultValue={item.type}>
                {applicationTypeLabels.map((label, index) => <option key={label} value={index}>{label}</option>)}
              </Select>
            </label>
          </div>
          {updateMutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(updateMutation.error)}</p>}
          <DialogActions>
            <Button type="button" variant="ghost" onClick={() => setEditOpen(false)}>Cancel</Button>
            <Button type="submit" disabled={updateMutation.isPending}>{updateMutation.isPending ? "Saving…" : "Save changes"}</Button>
          </DialogActions>
        </form>
      </Dialog>

      <Dialog
        open={statusDialogOpen}
        title={item.isActive ? "Disable application?" : "Reactivate application?"}
        description={item.isActive
          ? "All API keys for this application will stop working after the Gateway cache expires. You can reactivate it later."
          : "Active API keys for this application will be allowed to call APIs again."}
        onClose={() => setStatusDialogOpen(false)}
      >
        {statusMutation.isError && <p className="text-sm text-danger-700" role="alert">{errorMessage(statusMutation.error)}</p>}
        <DialogActions>
          <Button type="button" variant="ghost" onClick={() => setStatusDialogOpen(false)}>Cancel</Button>
          <Button type="button" variant={item.isActive ? "danger" : "primary"} disabled={statusMutation.isPending} onClick={() => statusMutation.mutate(!item.isActive)}>
            {statusMutation.isPending ? "Updating…" : item.isActive ? "Disable application" : "Reactivate application"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={deleteDialogOpen}
        title="Delete application?"
        description={`“${item.name}” and all of its API keys will be permanently deleted. Historical usage and billing data will be preserved.`}
        onClose={() => {
          if (!deleteMutation.isPending) {
            setDeleteDialogOpen(false);
          }
        }}
      >
        <div className="rounded-2xl bg-danger-50 p-4 text-sm leading-6 text-danger-700">
          Any service using this application's keys will lose access. This action cannot be undone.
        </div>
        {deleteMutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(deleteMutation.error)}</p>}
        <DialogActions>
          <Button type="button" variant="ghost" disabled={deleteMutation.isPending} onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button type="button" variant="danger" disabled={deleteMutation.isPending} onClick={() => deleteMutation.mutate()}>
            {deleteMutation.isPending ? "Deleting…" : "Delete application"}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}
