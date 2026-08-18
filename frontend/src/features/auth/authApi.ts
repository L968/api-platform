import { queryOptions } from "@tanstack/react-query";
import { http } from "../../shared/api/http";

export interface CurrentUser {
  id: string;
  email: string;
  status: number;
  organization: {
    id: string;
    name: string;
    status: number;
  };
}

export interface LoginRequest {
  email: string;
  password: string;
}

export const currentUserQuery = queryOptions({
  queryKey: ["current-user"],
  queryFn: () => http<CurrentUser>("/me"),
  retry: false,
});

export function login(request: LoginRequest) {
  return http("/auth/login", {
    method: "POST",
    body: JSON.stringify(request),
  });
}

export function logout() {
  return http<void>("/auth/logout", { method: "POST" });
}
