import { useQuery } from "@tanstack/react-query";
import { ArrowRight, ChartNoAxesCombined, CircleDollarSign, KeyRound, Layers3 } from "lucide-react";
import { Link } from "react-router";
import { applicationsQuery } from "../applications/applicationsApi";
import { credentialsQuery } from "../credentials/credentialsApi";
import { billingQuery, usageQuery } from "../usage/usageApi";
import { Card, ErrorState, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { formatApplicationType, formatCurrency, formatNumber } from "../../shared/format";

export function DashboardPage() {
  const applications = useQuery(applicationsQuery);
  const credentials = useQuery(credentialsQuery);
  const usage = useQuery(usageQuery());
  const billing = useQuery(billingQuery());

  if (applications.isPending || credentials.isPending || usage.isPending || billing.isPending) {
    return <LoadingState />;
  }

  const error = applications.error ?? credentials.error ?? usage.error ?? billing.error;
  if (error) {
    return <ErrorState error={error} />;
  }

  const applicationItems = applications.data ?? [];
  const credentialItems = credentials.data ?? [];
  const usageItems = usage.data ?? [];
  const billingSummary = billing.data!;
  const totalRequests = usageItems.reduce((sum, item) => sum + item.requests, 0);
  const activeApplicationIds = new Set(applicationItems.filter((application) => application.isActive).map((application) => application.id));
  const activeCredentials = credentialItems.filter((credential) => credential.isActive && activeApplicationIds.has(credential.applicationId)).length;

  return (
    <div className="space-y-8">
      <PageHeader title="Overview" description="A snapshot of your integrations and usage for the current period." />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <SummaryCard icon={Layers3} label="Applications" value={formatNumber(applicationItems.length)} />
        <SummaryCard icon={KeyRound} label="Valid API keys" value={formatNumber(activeCredentials)} />
        <SummaryCard icon={ChartNoAxesCombined} label="Requests" value={formatNumber(totalRequests)} />
        <SummaryCard icon={CircleDollarSign} label="Estimated cost" value={formatCurrency(billingSummary.total, 4)} />
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.4fr_0.6fr]">
        <Card className="overflow-hidden">
          <div className="flex items-center justify-between border-b border-plum-100 px-6 py-5">
            <div>
              <h2 className="font-display text-xl font-semibold text-plum-950">Recent applications</h2>
              <p className="mt-1 text-sm text-plum-500">Systems registered to this organization.</p>
            </div>
            <Link className="text-sm font-semibold text-brand-600 hover:text-brand-700" to="/applications">View all</Link>
          </div>
          {applicationItems.length === 0 ? (
            <div className="px-6 py-12 text-center text-sm text-plum-500">No applications registered.</div>
          ) : (
            <div className="divide-y divide-plum-100">
              {applicationItems.slice(0, 5).map((application) => (
                <Link key={application.id} to={`/applications/${application.id}`} className="group flex items-center justify-between gap-4 px-6 py-4 transition hover:bg-canvas/60">
                  <div>
                    <div className="flex items-center gap-2">
                      <p className="font-medium text-plum-900">{application.name}</p>
                      <StatusBadge active={application.isActive} />
                    </div>
                    <p className="mt-1 text-sm text-plum-500">{formatApplicationType(application.type)}</p>
                  </div>
                  <ArrowRight className="size-4 text-plum-300 transition group-hover:translate-x-1 group-hover:text-brand-600" />
                </Link>
              ))}
            </div>
          )}
        </Card>
        <Card className="relative overflow-hidden p-6">
          <span className="absolute -right-8 -top-8 size-32 rounded-full bg-sage-100" />
          <h2 className="relative font-display text-xl font-semibold text-plum-950">Current billing</h2>
          <p className="relative mt-1 text-sm text-plum-500">{billingSummary.from} to {billingSummary.to}</p>
          <p className="relative mt-8 font-display text-4xl font-semibold tracking-tight text-plum-950">{formatCurrency(billingSummary.total, 4)}</p>
          <p className="relative mt-2 text-sm leading-6 text-plum-500">Calculated from aggregated API usage in real time.</p>
          <Link to="/usage" className="relative mt-8 inline-flex items-center gap-2 text-sm font-semibold text-brand-600">View details <ArrowRight className="size-4" /></Link>
        </Card>
      </div>
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value }: { icon: typeof Layers3; label: string; value: string }) {
  return (
    <Card className="p-5">
      <span className="grid size-10 place-items-center rounded-2xl bg-brand-50 text-brand-600"><Icon className="size-5" /></span>
      <p className="mt-5 text-sm text-plum-500">{label}</p>
      <p className="mt-1 font-display text-3xl font-semibold text-plum-950">{value}</p>
    </Card>
  );
}
