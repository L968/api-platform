import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { applicationsQuery } from "../applications/applicationsApi";
import { Card, EmptyState, ErrorState, LoadingState, PageHeader, Select } from "../../shared/components/ui";
import { formatAmount, formatCurrency, formatDate, formatNumber } from "../../shared/format";
import { billingQuery, usageQuery, usageTimelineQuery, type UsageFilters, type UsageGranularity, type UsageTimelinePoint } from "./usageApi";

type PeriodPreset = "currentMonth" | "30d" | "3m" | "12m";
type ChartView = "usage" | "cost";
type BreakdownView = "endpoints" | "cost";

const periods: Array<{ value: PeriodPreset; label: string; granularity: UsageGranularity }> = [
  { value: "currentMonth", label: "This month", granularity: "day" },
  { value: "30d", label: "Last 30 days", granularity: "day" },
  { value: "3m", label: "Last 3 months", granularity: "week" },
  { value: "12m", label: "Last 12 months", granularity: "month" },
];

export function UsagePage() {
  const [applicationId, setApplicationId] = useState("");
  const [period, setPeriod] = useState<PeriodPreset>("currentMonth");
  const [chartView, setChartView] = useState<ChartView>("usage");
  const [breakdownView, setBreakdownView] = useState<BreakdownView>("endpoints");
  const selectedPeriod = periods.find((item) => item.value === period)!;
  const filters = useMemo<UsageFilters>(() => ({
    ...periodRange(period),
    applicationId: applicationId || undefined,
  }), [applicationId, period]);

  const applications = useQuery(applicationsQuery);
  const usage = useQuery(usageQuery(filters));
  const billing = useQuery(billingQuery(filters));
  const timeline = useQuery(usageTimelineQuery(filters, selectedPeriod.granularity));

  const totals = useMemo(() => {
    const points = timeline.data?.items ?? [];
    const requests = points.reduce((sum, item) => sum + item.requests, 0);
    const errors = points.reduce((sum, item) => sum + item.errors, 0);
    const weightedLatency = points.reduce(
      (sum, item) => sum + item.averageLatencyMs * item.requests,
      0,
    );

    return {
      requests,
      errors,
      errorRate: requests === 0 ? 0 : (errors / requests) * 100,
      averageLatency: requests === 0 ? 0 : weightedLatency / requests,
    };
  }, [timeline.data]);

  const applicationNames = new Map(applications.data?.map((application) => [application.id, application.name]));
  const apiNames = new Map(billing.data?.items.map((item) => [item.apiId, item.api]));
  const endpointItems = [...(usage.data ?? [])].sort((left, right) => right.requests - left.requests);
  const isPending = usage.isPending || billing.isPending || timeline.isPending;
  const error = usage.error ?? billing.error ?? timeline.error;

  return (
    <div className="space-y-7">
      <PageHeader title="Usage & billing" description="Understand traffic, reliability and cost over time." />

      <Card className="flex flex-col gap-4 p-3 lg:flex-row lg:items-center lg:justify-between">
        <label className="flex flex-col items-start gap-2 text-sm font-medium text-plum-600 sm:flex-row sm:items-center sm:gap-3">
          <span className="shrink-0 pl-2">Application</span>
          <Select className="w-full sm:min-w-52" value={applicationId} onChange={(event) => setApplicationId(event.target.value)}>
            <option value="">All applications</option>
            {applications.data?.map((application) => <option key={application.id} value={application.id}>{application.name}</option>)}
          </Select>
        </label>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
          <span className="px-2 text-xs text-plum-400">{formatDate(filters.from)} – {formatDate(filters.to)}</span>
          <div className="flex rounded-full bg-plum-50 p-1" aria-label="Usage period">
            {periods.map((item) => (
              <button
                key={item.value}
                type="button"
                className={`rounded-full px-3 py-2 text-xs font-semibold transition ${period === item.value ? "bg-paper text-plum-950 shadow-sm" : "text-plum-500 hover:text-plum-900"}`}
                aria-pressed={period === item.value}
                onClick={() => setPeriod(item.value)}
              >
                {item.label}
              </button>
            ))}
          </div>
        </div>
      </Card>

      {isPending && <LoadingState />}
      {error && <ErrorState error={error} />}
      {timeline.data && usage.data && billing.data && (
        <>
          <Card className="grid gap-px overflow-hidden bg-plum-100 sm:grid-cols-2 xl:grid-cols-4">
            <SummaryMetric label="Requests" value={formatNumber(totals.requests)} />
            <SummaryMetric label="Error rate" value={`${formatAmount(totals.errorRate)}%`} />
            <SummaryMetric label="Average latency" value={`${formatAmount(totals.averageLatency)} ms`} />
            <SummaryMetric label="Estimated cost" value={formatCurrency(billing.data.total, 4)} accent />
          </Card>

          <Card className="overflow-hidden">
            <div className="flex flex-col justify-between gap-4 border-b border-plum-100 px-6 py-5 sm:flex-row sm:items-center">
              <div>
                <h2 className="font-display text-xl font-semibold text-plum-950">Trend over time</h2>
                <p className="mt-1 text-sm text-plum-500">Grouped by {selectedPeriod.granularity}.</p>
              </div>
              <div className="flex items-center gap-4">
                <div className="text-right">
                  <p className="text-[0.65rem] font-semibold uppercase tracking-[0.14em] text-plum-400">Period total</p>
                  <p className="mt-1 font-display text-xl font-semibold text-plum-950">
                    {chartView === "usage" ? `${formatNumber(totals.requests)} requests` : formatCurrency(billing.data.total, 4)}
                  </p>
                </div>
                <ViewTabs
                  value={chartView}
                  items={[{ value: "usage", label: "Usage" }, { value: "cost", label: "Cost" }]}
                  onChange={(value) => setChartView(value as ChartView)}
                />
              </div>
            </div>
            <div className="p-4 sm:p-6">
              {chartView === "usage" ? (
                totals.requests === 0
                  ? <EmptyState title="No requests in this period" description="Traffic routed through the Gateway will appear here." />
                  : <UsageChart items={timeline.data.items} granularity={timeline.data.granularity} />
              ) : (
                billing.data.total === 0
                  ? <EmptyState title="No cost in this period" description="Billable API usage will appear here." />
                  : <CostChart items={timeline.data.items} granularity={timeline.data.granularity} />
              )}
            </div>
          </Card>

          <Card className="overflow-hidden">
            <div className="flex flex-col justify-between gap-4 border-b border-plum-100 px-6 py-5 sm:flex-row sm:items-center">
              <div>
                <h2 className="font-display text-xl font-semibold text-plum-950">Breakdown</h2>
                <p className="mt-1 text-sm text-plum-500">Inspect only the detail you need.</p>
              </div>
              <ViewTabs
                value={breakdownView}
                items={[{ value: "endpoints", label: "Endpoints" }, { value: "cost", label: "Cost by endpoint" }]}
                onChange={(value) => setBreakdownView(value as BreakdownView)}
              />
            </div>
            {breakdownView === "endpoints"
              ? <EndpointBreakdown items={endpointItems} apiNames={apiNames} applicationNames={applicationNames} />
              : <CostBreakdown items={billing.data.items} />}
          </Card>
        </>
      )}
    </div>
  );
}

function SummaryMetric({ label, value, accent = false }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="bg-paper p-5">
      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-plum-400">{label}</p>
      <p className={`mt-2 font-display text-3xl font-semibold ${accent ? "text-brand-600" : "text-plum-950"}`}>{value}</p>
    </div>
  );
}

function UsageChart({ items, granularity }: { items: UsageTimelinePoint[]; granularity: UsageGranularity }) {
  const ticks = chartTicks(items);

  return (
    <div>
      <div className="mb-3 flex justify-end gap-4 text-xs font-medium text-plum-500">
        <span className="inline-flex items-center gap-2"><span className="size-2 rounded-full bg-brand-600" /> Requests</span>
        <span className="inline-flex items-center gap-2"><span className="size-2 rounded-full bg-plum-500" /> Errors</span>
      </div>
      <div className="h-80 w-full">
        <ResponsiveContainer width="100%" height="100%">
        <LineChart data={items} margin={{ top: 12, right: 12, left: 0, bottom: 0 }} accessibilityLayer>
          <CartesianGrid vertical={false} stroke="#e9e0e9" />
          <XAxis dataKey="periodStart" ticks={ticks} interval={0} tickFormatter={(value) => shortPeriod(value, granularity)} tick={{ fill: "#725e74", fontSize: 12 }} tickLine={false} axisLine={false} tickMargin={10} />
          <YAxis allowDecimals={false} domain={[0, (maximum: number) => Math.max(maximum, 1)]} tickCount={3} tick={{ fill: "#725e74", fontSize: 12 }} tickLine={false} axisLine={false} width={42} />
          <Tooltip labelFormatter={(value) => fullPeriod(String(value), granularity)} itemSorter={(item) => item.dataKey === "requests" ? 0 : 1} contentStyle={tooltipStyle} />
          <Line type="linear" dataKey="requests" name="Requests" stroke="#0f766e" strokeWidth={2.5} dot={{ r: 2.5, fill: "#0f766e", strokeWidth: 0 }} activeDot={{ r: 5 }} />
          <Line type="linear" dataKey="errors" name="Errors" stroke="#725e74" strokeWidth={2} dot={{ r: 2, fill: "#725e74", strokeWidth: 0 }} activeDot={{ r: 4 }} />
        </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

function CostChart({ items, granularity }: { items: UsageTimelinePoint[]; granularity: UsageGranularity }) {
  const ticks = chartTicks(items);

  return (
    <div className="h-80 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={items} margin={{ top: 12, right: 12, left: 0, bottom: 0 }} accessibilityLayer>
          <CartesianGrid vertical={false} stroke="#e9e0e9" />
          <XAxis dataKey="periodStart" ticks={ticks} interval={0} tickFormatter={(value) => shortPeriod(value, granularity)} tick={{ fill: "#725e74", fontSize: 12 }} tickLine={false} axisLine={false} tickMargin={10} />
          <YAxis tickFormatter={(value) => formatCurrency(Number(value))} tick={{ fill: "#725e74", fontSize: 12 }} tickLine={false} axisLine={false} width={72} />
          <Tooltip labelFormatter={(value) => fullPeriod(String(value), granularity)} formatter={(value) => [formatCurrency(Number(value), 4), "Cost"]} contentStyle={tooltipStyle} />
          <Line type="linear" dataKey="cost" name="Cost" stroke="#5b5bd6" strokeWidth={2.5} dot={{ r: 2.5, fill: "#5b5bd6", strokeWidth: 0 }} activeDot={{ r: 5 }} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}

function EndpointBreakdown({
  items,
  apiNames,
  applicationNames,
}: {
  items: Array<{ apiId: string; applicationId: string; endpoint: string; requests: number; errors: number; averageLatencyMs: number }>;
  apiNames: Map<string, string>;
  applicationNames: Map<string, string>;
}) {
  if (items.length === 0) {
    return <EmptyState title="No endpoint activity" description="There are no requests to break down for this period." />;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] text-left text-sm">
        <thead className="bg-canvas/70 text-xs uppercase tracking-wide text-plum-500">
          <tr><th className="px-6 py-3">Endpoint</th><th className="px-6 py-3">Application</th><th className="px-6 py-3 text-right">Requests</th><th className="px-6 py-3 text-right">Errors</th><th className="px-6 py-3 text-right">Latency</th></tr>
        </thead>
        <tbody className="divide-y divide-plum-100">
          {items.map((item) => (
            <tr key={`${item.applicationId}-${item.apiId}-${item.endpoint}`}>
              <td className="px-6 py-4"><p className="font-medium text-plum-900">{apiNames.get(item.apiId) ?? "API"}</p><code className="text-xs text-plum-500">{item.endpoint}</code></td>
              <td className="px-6 py-4 text-plum-600">{applicationNames.get(item.applicationId) ?? item.applicationId.slice(0, 8)}</td>
              <td className="px-6 py-4 text-right font-medium">{formatNumber(item.requests)}</td>
              <td className="px-6 py-4 text-right">{formatNumber(item.errors)}</td>
              <td className="px-6 py-4 text-right">{formatAmount(item.averageLatencyMs)} ms</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function CostBreakdown({ items }: { items: Array<{ apiId: string; api: string; endpoint: string; requests: number; errors: number; billableRequests: number; pricePerRequest: number; amount: number }> }) {
  if (items.length === 0) {
    return <EmptyState title="No billable usage" description="There are no API charges for this period." />;
  }

  const sortedItems = [...items].sort((left, right) => right.amount - left.amount);

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] text-left text-sm">
        <thead className="bg-canvas/70 text-xs uppercase tracking-wide text-plum-500">
          <tr><th className="px-6 py-3">Endpoint</th><th className="px-6 py-3 text-right">Billable requests</th><th className="px-6 py-3 text-right">Effective rate</th><th className="px-6 py-3 text-right">Cost</th></tr>
        </thead>
        <tbody className="divide-y divide-plum-100">
          {sortedItems.map((item) => (
            <tr key={`${item.apiId}-${item.endpoint}`}>
              <td className="px-6 py-4"><p className="font-medium text-plum-900">{item.api}</p><code className="text-xs text-plum-500">{item.endpoint}</code></td>
              <td className="px-6 py-4 text-right"><p className="font-medium text-plum-900">{formatNumber(item.billableRequests)}</p>{item.errors > 0 && <p className="text-xs text-plum-400">{formatNumber(item.errors)} errors not charged</p>}</td>
              <td className="px-6 py-4 text-right text-plum-600">{formatCurrency(item.pricePerRequest, 4)}</td>
              <td className="px-6 py-4 text-right font-display text-lg font-semibold text-plum-950">{formatCurrency(item.amount, 4)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ViewTabs({ value, items, onChange }: { value: string; items: Array<{ value: string; label: string }>; onChange: (value: string) => void }) {
  return (
    <div className="flex rounded-full bg-plum-50 p-1">
      {items.map((item) => (
        <button key={item.value} type="button" className={`rounded-full px-3 py-1.5 text-xs font-semibold transition ${value === item.value ? "bg-paper text-plum-950 shadow-sm" : "text-plum-500 hover:text-plum-900"}`} aria-pressed={value === item.value} onClick={() => onChange(item.value)}>{item.label}</button>
      ))}
    </div>
  );
}

function periodRange(period: PeriodPreset): Pick<UsageFilters, "from" | "to"> {
  const today = new Date();
  const end = new Date(Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate()));
  const start = new Date(end);

  if (period === "currentMonth") {
    start.setUTCDate(1);
  } else if (period === "30d") {
    start.setUTCDate(start.getUTCDate() - 29);
  } else if (period === "12m") {
    start.setUTCMonth(start.getUTCMonth() - 11, 1);
  } else {
    start.setUTCMonth(start.getUTCMonth() - 2, 1);
  }

  return { from: isoDate(start), to: isoDate(end) };
}

function chartTicks(items: UsageTimelinePoint[]): string[] {
  const maximumTicks = 14;
  const step = Math.ceil(items.length / maximumTicks);
  return items
    .filter((_, index) => index % step === 0)
    .map((item) => item.periodStart);
}

function isoDate(date: Date): string {
  return date.toISOString().slice(0, 10);
}

function shortPeriod(value: string, granularity: UsageGranularity): string {
  return new Intl.DateTimeFormat("en-US", granularity === "month"
    ? { month: "short" }
    : { month: "short", day: "numeric" }).format(new Date(`${value}T00:00:00`));
}

function fullPeriod(value: string, granularity: UsageGranularity): string {
  const date = formatDate(value);
  return granularity === "week" ? `Week of ${date}` : date;
}

const tooltipStyle = {
  border: "1px solid #e9e0e9",
  borderRadius: "14px",
  background: "#fffdf8",
  boxShadow: "0 18px 40px -24px rgba(48, 29, 52, 0.5)",
};
