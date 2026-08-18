import { useQuery } from "@tanstack/react-query";
import { ArrowRight, ChartNoAxesCombined, CircleDollarSign, KeyRound, Layers3 } from "lucide-react";
import { Link } from "react-router";
import { applicationsQuery } from "../applications/applicationsApi";
import { credentialsQuery } from "../credentials/credentialsApi";
import { billingQuery, usageQuery } from "../usage/usageApi";
import { Card, ErrorState, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { formatAmount, formatApplicationType, formatNumber } from "../../shared/format";

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
      <PageHeader title="Visão geral" description="Resumo das integrações e do consumo no período atual." />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <SummaryCard icon={Layers3} label="Applications" value={formatNumber(applicationItems.length)} />
        <SummaryCard icon={KeyRound} label="Chaves válidas" value={formatNumber(activeCredentials)} />
        <SummaryCard icon={ChartNoAxesCombined} label="Requisições" value={formatNumber(totalRequests)} />
        <SummaryCard icon={CircleDollarSign} label="Valor estimado" value={formatAmount(billingSummary.total, 4)} />
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.4fr_0.6fr]">
        <Card className="overflow-hidden">
          <div className="flex items-center justify-between border-b border-slate-100 px-6 py-5"><div><h2 className="font-semibold text-slate-950">Applications recentes</h2><p className="mt-1 text-sm text-slate-500">Sistemas cadastrados na Organization.</p></div><Link className="text-sm font-semibold text-brand-600 hover:text-brand-700" to="/applications">Ver todas</Link></div>
          {applicationItems.length === 0 ? <div className="px-6 py-12 text-center text-sm text-slate-500">Nenhuma Application cadastrada.</div> : <div className="divide-y divide-slate-100">{applicationItems.slice(0, 5).map((application) => <Link key={application.id} to={`/applications/${application.id}`} className="flex items-center justify-between gap-4 px-6 py-4 hover:bg-slate-50"><div><div className="flex items-center gap-2"><p className="font-medium text-slate-900">{application.name}</p><StatusBadge active={application.isActive} /></div><p className="mt-1 text-sm text-slate-500">{formatApplicationType(application.type)}</p></div><ArrowRight className="size-4 text-slate-400" /></Link>)}</div>}
        </Card>
        <Card className="p-6">
          <h2 className="font-semibold text-slate-950">Billing atual</h2>
          <p className="mt-1 text-sm text-slate-500">{billingSummary.from} a {billingSummary.to}</p>
          <p className="mt-8 text-4xl font-bold tracking-tight text-slate-950">{formatAmount(billingSummary.total, 4)}</p>
          <p className="mt-2 text-sm text-slate-500">Valor calculado em tempo real com base no consumo agregado.</p>
          <Link to="/usage" className="mt-8 inline-flex items-center gap-2 text-sm font-semibold text-brand-600">Ver detalhes <ArrowRight className="size-4" /></Link>
        </Card>
      </div>
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value }: { icon: typeof Layers3; label: string; value: string }) {
  return <Card className="p-5"><div className="flex items-center justify-between"><span className="grid size-10 place-items-center rounded-xl bg-brand-50 text-brand-600"><Icon className="size-5" /></span></div><p className="mt-5 text-sm text-slate-500">{label}</p><p className="mt-1 text-2xl font-bold text-slate-950">{value}</p></Card>;
}
