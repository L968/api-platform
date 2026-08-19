import { useQuery } from "@tanstack/react-query";
import { FileText } from "lucide-react";
import { Link } from "react-router";
import { Card, EmptyState, ErrorState, LoadingState, PageHeader, StatusBadge } from "../../shared/components/ui";
import { formatCurrency, formatDate } from "../../shared/format";
import { invoicesQuery } from "./billingApi";

export function InvoicesPage() {
  const invoices = useQuery(invoicesQuery);

  if (invoices.isPending) {
    return <LoadingState />;
  }

  if (invoices.isError) {
    return <ErrorState error={invoices.error} />;
  }

  return (
    <div className="space-y-7">
      <PageHeader title="Invoices" description="Review your monthly charges and payment status." />
      <Card className="overflow-hidden">
        {invoices.data.length === 0 ? (
          <EmptyState title="No invoices yet" description="Your first monthly invoice will appear here after a billing period closes." />
        ) : (
          <div className="divide-y divide-plum-100">
            {invoices.data.map((invoice) => (
              <Link key={invoice.id} to={`/billing/${invoice.id}`} className="flex flex-col justify-between gap-4 px-6 py-5 transition hover:bg-canvas/60 sm:flex-row sm:items-center">
                <div className="flex items-center gap-4">
                  <span className="grid size-10 place-items-center rounded-xl bg-brand-50 text-brand-700"><FileText className="size-5" /></span>
                  <div>
                    <p className="font-semibold text-plum-950">{invoice.number}</p>
                    <p className="mt-1 text-sm text-plum-500">{formatDate(invoice.periodStart)} – {formatDate(invoice.periodEnd)}</p>
                  </div>
                </div>
                <div className="flex items-center gap-6 sm:text-right">
                  <div>
                    <p className="font-display text-lg font-semibold text-plum-950">{formatCurrency(invoice.totalAmount, 4)}</p>
                    <p className="text-xs text-plum-500">Due {formatDate(invoice.dueAt)}</p>
                  </div>
                  <StatusBadge active={invoice.status === "Paid"} activeLabel="Paid" inactiveLabel="Open" />
                </div>
              </Link>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
