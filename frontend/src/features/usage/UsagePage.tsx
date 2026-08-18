import { useMemo, useState, type FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { ChartNoAxesCombined, Clock3, ReceiptText, RotateCcw, Search, TriangleAlert } from "lucide-react";
import { applicationsQuery } from "../applications/applicationsApi";
import { Button, Card, EmptyState, ErrorState, Input, LoadingState, PageHeader } from "../../shared/components/ui";
import { formatAmount, formatNumber } from "../../shared/format";
import { billingQuery, usageQuery, type UsageFilters } from "./usageApi";

export function UsagePage() {
  const [formFilters, setFormFilters] = useState<UsageFilters>({});
  const [filters, setFilters] = useState<UsageFilters>({});
  const applications = useQuery(applicationsQuery);
  const usage = useQuery(usageQuery(filters));
  const billing = useQuery(billingQuery({ from: filters.from, to: filters.to }));

  const totals = useMemo(() => ({
    requests: usage.data?.reduce((sum, item) => sum + item.requests, 0) ?? 0,
    errors: usage.data?.reduce((sum, item) => sum + item.errors, 0) ?? 0,
    latency: usage.data?.length ? usage.data.reduce((sum, item) => sum + item.averageLatencyMs, 0) / usage.data.length : 0,
  }), [usage.data]);

  const applicationNames = new Map(applications.data?.map((application) => [application.id, application.name]));
  const apiNames = new Map(billing.data?.items.map((item) => [item.apiId, item.api]));

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilters(formFilters);
  }

  function clearFilters() {
    setFormFilters({});
    setFilters({});
  }

  return (
    <div className="space-y-8">
      <PageHeader title="Consumo e billing" description="Acompanhe chamadas, erros, latência e valor acumulado no período." />

      <Card className="p-5">
        <form className="grid gap-4 sm:grid-cols-2 lg:grid-cols-[1fr_180px_180px_auto] lg:items-end" onSubmit={handleSubmit}>
          <label className="text-sm font-medium text-slate-700">Application<select className="mt-2 min-h-11 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm" value={formFilters.applicationId ?? ""} onChange={(event) => setFormFilters((current) => ({ ...current, applicationId: event.target.value || undefined }))}><option value="">Todas</option>{applications.data?.map((application) => <option key={application.id} value={application.id}>{application.name}</option>)}</select></label>
          <label className="text-sm font-medium text-slate-700">De<Input className="mt-2" type="date" value={formFilters.from ?? ""} onChange={(event) => setFormFilters((current) => ({ ...current, from: event.target.value || undefined }))} /></label>
          <label className="text-sm font-medium text-slate-700">Até<Input className="mt-2" type="date" min={formFilters.from} value={formFilters.to ?? ""} onChange={(event) => setFormFilters((current) => ({ ...current, to: event.target.value || undefined }))} /></label>
          <div className="flex gap-2"><Button type="submit"><Search className="size-4" /> Filtrar</Button><Button type="button" variant="ghost" onClick={clearFilters} aria-label="Limpar filtros"><RotateCcw className="size-4" /></Button></div>
        </form>
      </Card>

      {(usage.isPending || billing.isPending) && <LoadingState />}
      {(usage.isError || billing.isError) && <ErrorState error={usage.error ?? billing.error} />}
      {usage.data && billing.data && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <MetricCard icon={ChartNoAxesCombined} label="Requisições" value={formatNumber(totals.requests)} />
            <MetricCard icon={TriangleAlert} label="Erros" value={formatNumber(totals.errors)} />
            <MetricCard icon={Clock3} label="Latência média" value={`${formatAmount(totals.latency)} ms`} />
            <MetricCard icon={ReceiptText} label="Valor da Organization" value={formatAmount(billing.data.total, 4)} />
          </div>

          <Card className="overflow-hidden">
            <div className="border-b border-slate-100 px-6 py-5"><h2 className="font-semibold text-slate-950">Consumo por endpoint</h2></div>
            {usage.data.length === 0 ? <EmptyState title="Sem consumo no período" description="As requisições encaminhadas pelo Gateway aparecerão aqui." /> : (
              <div className="overflow-x-auto"><table className="w-full min-w-[760px] text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-6 py-3">API / endpoint</th><th className="px-6 py-3">Application</th><th className="px-6 py-3 text-right">Requests</th><th className="px-6 py-3 text-right">Erros</th><th className="px-6 py-3 text-right">Latência</th></tr></thead><tbody className="divide-y divide-slate-100">{usage.data.map((item) => <tr key={`${item.applicationId}-${item.apiId}-${item.endpoint}`}><td className="px-6 py-4"><p className="font-medium text-slate-900">{apiNames.get(item.apiId) ?? "API"}</p><code className="text-xs text-slate-500">{item.endpoint}</code></td><td className="px-6 py-4 text-slate-600">{applicationNames.get(item.applicationId) ?? item.applicationId.slice(0, 8)}</td><td className="px-6 py-4 text-right font-medium">{formatNumber(item.requests)}</td><td className="px-6 py-4 text-right">{formatNumber(item.errors)}</td><td className="px-6 py-4 text-right">{formatAmount(item.averageLatencyMs)} ms</td></tr>)}</tbody></table></div>
            )}
          </Card>

          <Card className="overflow-hidden">
            <div className="border-b border-slate-100 px-6 py-5"><h2 className="font-semibold text-slate-950">Resumo de billing da Organization</h2><p className="mt-1 text-sm text-slate-500">{billing.data.from} a {billing.data.to} · consolidado para todas as Applications</p></div>
            {billing.data.items.length === 0 ? <EmptyState title="Nenhum valor calculado" description="Ainda não há consumo tarifado neste período." /> : <div className="divide-y divide-slate-100">{billing.data.items.map((item) => <div key={item.apiId} className="grid gap-2 px-6 py-4 text-sm sm:grid-cols-[1fr_auto_auto] sm:items-center sm:gap-8"><div><p className="font-semibold text-slate-900">{item.api}</p><p className="text-slate-500">{formatNumber(item.requests)} requests × {formatAmount(item.pricePerRequest, 4)}</p></div><span className="text-slate-500">Subtotal</span><strong className="text-right text-slate-950">{formatAmount(item.amount, 4)}</strong></div>)}</div>}
          </Card>
        </>
      )}
    </div>
  );
}

function MetricCard({ icon: Icon, label, value }: { icon: typeof ChartNoAxesCombined; label: string; value: string }) {
  return <Card className="p-5"><span className="grid size-10 place-items-center rounded-xl bg-brand-50 text-brand-600"><Icon className="size-5" /></span><p className="mt-5 text-sm text-slate-500">{label}</p><p className="mt-1 text-2xl font-bold text-slate-950">{value}</p></Card>;
}
