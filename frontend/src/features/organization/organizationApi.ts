import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface Organization {
  id: string;
  name: string;
  status: number;
  createdAt: string;
  rates: OrganizationRate[];
}

export interface OrganizationRate {
  apiId: string;
  api: string;
  pricePerRequest: number;
  effectiveFrom: string | null;
  nextPricePerRequest: number | null;
  nextEffectiveFrom: string | null;
}

export const organizationQuery = queryOptions({
  queryKey: ["organization"],
  queryFn: () => http<Organization>("/organization"),
});

export function updateOrganization(name: string) {
  return http<void>("/organization", {
    method: "PUT",
    body: JSON.stringify({ name }),
  });
}
