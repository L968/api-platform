import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface UsageFilters {
  applicationId?: string;
  from?: string;
  to?: string;
}

export interface UsageItem {
  apiId: string;
  applicationId: string;
  endpoint: string;
  requests: number;
  errors: number;
  averageLatencyMs: number;
}

export interface BillingItem {
  apiId: string;
  api: string;
  requests: number;
  pricePerRequest: number;
  amount: number;
}

export interface Billing {
  from: string;
  to: string;
  total: number;
  items: BillingItem[];
}

export function usageQuery(filters: UsageFilters = {}) {
  return queryOptions({
    queryKey: ["usage", filters],
    queryFn: () => http<UsageItem[]>(`/usage${queryString(filters)}`),
  });
}

export function billingQuery(filters: Pick<UsageFilters, "from" | "to"> = {}) {
  return queryOptions({
    queryKey: ["billing", filters],
    queryFn: () => http<Billing>(`/billing${queryString(filters)}`),
  });
}

function queryString(filters: UsageFilters): string {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });
  const value = params.toString();
  return value ? `?${value}` : "";
}
