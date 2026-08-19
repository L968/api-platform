import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface InvoiceSummary {
  id: string;
  number: string;
  periodStart: string;
  periodEnd: string;
  status: "Open" | "Paid";
  totalAmount: number;
  dueAt: string;
  paidAt: string | null;
}

export interface InvoiceLine {
  id: string;
  apiId: string;
  api: string;
  endpoint: string;
  requests: number;
  errors: number;
  billableRequests: number;
  pricePerRequest: number;
  priceEffectiveFrom: string;
  amount: number;
}

export interface InvoiceDetail extends InvoiceSummary {
  issuedAt: string;
  lines: InvoiceLine[];
}

export const invoicesQuery = queryOptions({
  queryKey: ["billing-invoices"],
  queryFn: () => http<InvoiceSummary[]>("/billing/invoices"),
});

export function invoiceQuery(invoiceId: string) {
  return queryOptions({
    queryKey: ["billing-invoice", invoiceId],
    queryFn: () => http<InvoiceDetail>(`/billing/invoices/${invoiceId}`),
  });
}

export function payInvoice(invoiceId: string) {
  return http<InvoiceDetail>(`/billing/invoices/${invoiceId}/pay`, { method: "POST" });
}
