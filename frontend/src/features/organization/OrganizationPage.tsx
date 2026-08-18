import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, CircleDollarSign, Edit3 } from "lucide-react";
import { Button, Card, Dialog, DialogActions, EmptyState, ErrorState, Input, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { errorMessage, formatCurrency, formatDate } from "../../shared/format";
import { currentUserQuery } from "../auth/authApi";
import { organizationQuery, updateOrganization } from "./organizationApi";

export function OrganizationPage() {
  const queryClient = useQueryClient();
  const organization = useQuery(organizationQuery);
  const [editOpen, setEditOpen] = useState(false);
  const mutation = useMutation({
    mutationFn: updateOrganization,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: organizationQuery.queryKey }),
        queryClient.invalidateQueries({ queryKey: currentUserQuery.queryKey }),
      ]);
      setEditOpen(false);
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    mutation.mutate(String(data.get("name")));
  }

  return (
    <div className="space-y-8">
      <PageHeader title="Organization" description="Company information linked to your account." />
      {organization.isPending && <LoadingState />}
      {organization.isError && <ErrorState error={organization.error} />}
      {organization.data && (
        <>
          <Card className="p-6 sm:p-7">
            <div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-start">
              <div className="flex items-start gap-4">
                <span className="grid size-11 shrink-0 place-items-center rounded-2xl bg-sage-100 text-sage-800"><Building2 className="size-5" /></span>
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600">Account owner</p>
                  <h2 className="mt-2 font-display text-2xl font-semibold text-plum-950">{organization.data.name}</h2>
                </div>
              </div>
              <Button variant="secondary" onClick={() => {
                mutation.reset();
                setEditOpen(true);
              }}><Edit3 className="size-4" /> Edit</Button>
            </div>
            <dl className="mt-7 grid gap-6 border-t border-plum-100 pt-6 sm:grid-cols-3">
              <div><dt className="text-xs font-semibold uppercase tracking-wider text-plum-400">Status</dt><dd className="mt-2"><StatusBadge active={organization.data.status === 0} /></dd></div>
              <div><dt className="text-xs font-semibold uppercase tracking-wider text-plum-400">Created</dt><dd className="mt-2 font-medium text-plum-900">{formatDate(organization.data.createdAt)}</dd></div>
              <div><dt className="text-xs font-semibold uppercase tracking-wider text-plum-400">Organization ID</dt><dd className="mt-2 break-all font-mono text-xs text-plum-600">{organization.data.id}</dd></div>
            </dl>
          </Card>

          <section>
            <div className="mb-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600">Commercial terms</p>
              <h2 className="mt-2 font-display text-2xl font-semibold text-plum-950">API rates</h2>
              <p className="mt-1 text-sm text-plum-500">Your current price per successful request.</p>
            </div>
            <Card className="overflow-hidden">
              {organization.data.rates.length === 0 ? (
                <EmptyState title="No API rates configured" description="Contact the platform operator for pricing information." />
              ) : (
                <div className="divide-y divide-plum-100">
                  {organization.data.rates.map((rate) => (
                    <div key={rate.apiId} className="flex flex-col justify-between gap-4 px-6 py-5 sm:flex-row sm:items-center">
                      <div className="flex items-center gap-3">
                        <span className="grid size-10 shrink-0 place-items-center rounded-2xl bg-brand-50 text-brand-700"><CircleDollarSign className="size-5" /></span>
                        <div>
                          <p className="font-semibold text-plum-950">{rate.api}</p>
                          <p className="mt-1 text-xs text-plum-500">
                            {rate.effectiveFrom ? `Effective since ${formatDate(rate.effectiveFrom)}` : "Current contracted rate"}
                          </p>
                        </div>
                      </div>
                      <div className="sm:text-right">
                        <p className="font-display text-xl font-semibold text-plum-950">{formatCurrency(rate.pricePerRequest, 4)}</p>
                        <p className="text-xs text-plum-500">per successful request</p>
                        {rate.nextEffectiveFrom && rate.nextPricePerRequest !== null && (
                          <p className="mt-2 text-xs font-medium text-brand-700">
                            Changes to {formatCurrency(rate.nextPricePerRequest, 4)} on {formatDate(rate.nextEffectiveFrom)}
                          </p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          </section>

          <Dialog open={editOpen} title="Edit organization" description="Update the company name shown throughout the portal." onClose={() => setEditOpen(false)}>
            <form onSubmit={handleSubmit}>
              <label className="block text-sm font-medium text-plum-700">
                Organization name
                <Input className="mt-2" name="name" required defaultValue={organization.data.name} autoFocus />
              </label>
              {mutation.isError && <p className="mt-4 text-sm text-danger-700" role="alert">{errorMessage(mutation.error)}</p>}
              <DialogActions>
                <Button type="button" variant="ghost" onClick={() => setEditOpen(false)}>Cancel</Button>
                <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? "Saving…" : "Save changes"}</Button>
              </DialogActions>
            </form>
          </Dialog>
        </>
      )}
    </div>
  );
}
