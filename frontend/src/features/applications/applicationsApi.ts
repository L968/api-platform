import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface Application {
  id: string;
  name: string;
  type: number;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ApplicationRequest {
  name: string;
  type: number;
}

export const applicationsQuery = queryOptions({
  queryKey: ["applications"],
  queryFn: () => http<Application[]>("/applications"),
});

export function applicationQuery(id: string) {
  return queryOptions({
    queryKey: ["applications", id],
    queryFn: () => http<Application>(`/applications/${id}`),
  });
}

export function createApplication(request: ApplicationRequest) {
  return http<Application>("/applications", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function updateApplication(id: string, request: ApplicationRequest) {
  return http<void>(`/applications/${id}`, {
    method: "PUT",
    body: JSON.stringify(request),
  });
}

export function setApplicationActive(id: string, active: boolean) {
  const action = active ? "reactivate" : "disable";
  return http<void>(`/applications/${id}/${action}`, { method: "POST" });
}

export function deleteApplication(id: string) {
  return http<void>(`/applications/${id}`, { method: "DELETE" });
}
