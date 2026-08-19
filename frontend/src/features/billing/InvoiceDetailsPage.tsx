import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, CheckCircle2, FileText, Printer } from "lucide-react";
import { Link, useParams } from "react-router";
import { Button, Card, ErrorState, LoadingState, StatusBadge } from "../../shared/components/ui";
import { formatCurrency, formatDate } from "../../shared/format";
import { invoiceQuery, invoicesQuery, payInvoice } from "./billingApi";

export function InvoiceDetailsPage() {
  const { invoiceId = "" } = useParams();
  const queryClient = useQueryClient();
  const invoice = useQuery(invoiceQuery(invoiceId));
  const payment = useMutation({
    mutationFn: () => payInvoice(invoiceId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: invoiceQuery(invoiceId).queryKey }),
        queryClient.invalidateQueries({ queryKey: invoicesQuery.queryKey }),
      ]);
    },
  });

  if (invoice.isPending) {
    return <LoadingState />;
  }

  if (invoice.isError) {
    return <ErrorState error={invoice.error} />;
  }

  const data = invoice.data;
  const isPaid = data.status === "Paid";

  return (
    <div className="space-y-7">
      <Link to="/invoices" className="print:hidden inline-flex items-center gap-2 text-sm font-semibold text-plum-600 hover:text-brand-700"><ArrowLeft className="size-4" /> Back to invoices</Link>
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-brand-600">Invoice</p>
          <h1 className="mt-2 font-display text-3xl font-semibold tracking-tight text-plum-950">{data.number}</h1>
          <p className="mt-2 text-sm text-plum-500">{formatDate(data.periodStart)} – {formatDate(data.periodEnd)}</p>
        </div>
        <div className="flex items-center gap-3">
          <StatusBadge active={isPaid} activeLabel="Paid" inactiveLabel="Open" />
          <Button variant="secondary" className="print:hidden" onClick={() => window.print()}><Printer className="size-4" /> Print invoice</Button>
          {!isPaid && <Button className="print:hidden" onClick={() => {
            if (window.confirm("Mark this invoice as paid? This is a local simulation.")) {
              payment.mutate();
            }
          }} disabled={payment.isPending}><CheckCircle2 className="size-4" /> Mark as paid</Button>}
        </div>
      </div>

      {payment.isError && <ErrorState error={payment.error} />}

      <Card className="overflow-hidden">
        <div className="flex items-start justify-between gap-4 border-b border-plum-100 p-6">
          <div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-2xl bg-brand-50 text-brand-700"><FileText className="size-5" /></span><div><p className="font-semibold text-plum-950">Usage invoice</p><p className="text-sm text-plum-500">Issued {formatDate(data.issuedAt)}</p></div></div>
          <div className="text-right"><p className="text-xs uppercase tracking-wider text-plum-400">Total due</p><p className="mt-1 font-display text-2xl font-semibold text-plum-950">{formatCurrency(data.totalAmount, 4)}</p></div>
        </div>
        <div className="grid gap-6 border-b border-plum-100 px-6 py-5 text-sm sm:grid-cols-3">
          <div><p className="text-xs uppercase tracking-wider text-plum-400">Billing period</p><p className="mt-1 font-medium text-plum-900">{formatDate(data.periodStart)} – {formatDate(data.periodEnd)}</p></div>
          <div><p className="text-xs uppercase tracking-wider text-plum-400">Due date</p><p className="mt-1 font-medium text-plum-900">{formatDate(data.dueAt)}</p></div>
          <div><p className="text-xs uppercase tracking-wider text-plum-400">Paid on</p><p className="mt-1 font-medium text-plum-900">{data.paidAt ? formatDate(data.paidAt) : "Not paid"}</p></div>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full min-w-[850px] text-left text-sm">
            <thead className="bg-canvas/70 text-xs uppercase tracking-wide text-plum-500"><tr><th className="px-6 py-3">Endpoint</th><th className="px-6 py-3 text-right">Billable requests</th><th className="px-6 py-3 text-right">Price period / base rate</th><th className="px-6 py-3 text-right">Rate</th><th className="px-6 py-3 text-right">Amount</th></tr></thead>
            <tbody className="divide-y divide-plum-100">
              {data.lines.map((line) => <tr key={line.id}><td className="px-6 py-4"><p className="font-medium text-plum-900">{line.api}</p><code className="text-xs text-plum-500">{line.endpoint}</code></td><td className="px-6 py-4 text-right">{line.billableRequests.toLocaleString("en-US")}</td><td className="px-6 py-4 text-right text-plum-600">{formatDate(line.priceEffectiveFrom)}</td><td className="px-6 py-4 text-right text-plum-600">{formatCurrency(line.pricePerRequest, 4)}</td><td className="px-6 py-4 text-right font-display text-lg font-semibold text-plum-950">{formatCurrency(line.amount, 4)}</td></tr>)}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
