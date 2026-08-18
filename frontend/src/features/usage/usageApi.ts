import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface UsageFilters {
  applicationId?: string;
  from?: string;
  to?: string;
}

export type UsageGranularity = "day" | "week" | "month";

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
  endpoint: string;
  requests: number;
  errors: number;
  billableRequests: number;
  pricePerRequest: number;
  amount: number;
}

export interface Billing {
  from: string;
  to: string;
  total: number;
  items: BillingItem[];
}

export interface UsageTimelinePoint {
  periodStart: string;
  requests: number;
  errors: number;
  averageLatencyMs: number;
  cost: number;
}

export interface UsageTimeline {
  from: string;
  to: string;
  granularity: UsageGranularity;
  items: UsageTimelinePoint[];
}

export function usageQuery(filters: UsageFilters = {}) {
  return queryOptions({
    queryKey: ["usage", filters],
    queryFn: () => http<UsageItem[]>(`/usage${queryString(filters)}`),
  });
}

export function billingQuery(filters: UsageFilters = {}) {
  return queryOptions({
    queryKey: ["billing", filters],
    queryFn: () => http<Billing>(`/billing${queryString(filters)}`),
  });
}

export function usageTimelineQuery(
  filters: UsageFilters,
  granularity: UsageGranularity,
) {
  return queryOptions({
    queryKey: ["usage-timeline", filters, granularity],
    queryFn: () => http<UsageTimeline>(`/usage/timeline${queryString({ ...filters, granularity })}`),
  });
}

function queryString(filters: object): string {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      params.set(key, value);
    }
  });
  const value = params.toString();
  return value ? `?${value}` : "";
}
