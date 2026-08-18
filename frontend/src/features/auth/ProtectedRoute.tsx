import { useQuery } from "@tanstack/react-query";
import { Navigate, Outlet, useLocation } from "react-router";
import { ApiError } from "../../shared/api/http";
import { ErrorState, LoadingState } from "../../shared/components/ui";
import { currentUserQuery } from "./authApi";

export function ProtectedRoute() {
  const location = useLocation();
  const user = useQuery(currentUserQuery);

  if (user.isPending) {
    return <LoadingState />;
  }

  if (user.error instanceof ApiError && user.error.status === 401) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (user.isError) {
    return (
      <main className="mx-auto max-w-xl p-8">
        <ErrorState error={user.error} />
      </main>
    );
  }

  return <Outlet />;
}
