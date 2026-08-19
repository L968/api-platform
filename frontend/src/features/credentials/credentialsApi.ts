import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface Credential {
  id: string;
  name: string;
  clientId: string;
  applicationId: string;
  createdAt: string;
  expiresAt: string | null;
  revokedAt: string | null;
  scopes: string[];
  isActive: boolean;
}

export interface Scope {
  id: string;
  name: string;
}

export interface CreateCredentialRequest {
  name: string;
  expiresAt: string | null;
  scopeIds: string[];
}

export interface CreatedCredential {
  id: string;
  name: string;
  clientId: string;
  apiKey: string;
  expiresAt: string | null;
  scopes: string[];
}

export const credentialsQuery = queryOptions({
  queryKey: ["credentials"],
  queryFn: () => http<Credential[]>("/credentials"),
});

export const scopesQuery = queryOptions({
  queryKey: ["scopes"],
  queryFn: () => http<Scope[]>("/scopes"),
  staleTime: 5 * 60_000,
});

export function createCredential(applicationId: string, request: CreateCredentialRequest) {
  return http<CreatedCredential>(`/applications/${applicationId}/credentials`, {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function revokeCredential(id: string) {
  return http<void>(`/credentials/${id}/revoke`, { method: "POST" });
}
