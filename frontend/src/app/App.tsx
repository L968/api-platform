import { lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router";
import { ApplicationDetailsPage } from "../features/applications/ApplicationDetailsPage";
import { ApplicationsPage } from "../features/applications/ApplicationsPage";
import { ProtectedRoute } from "../features/auth/ProtectedRoute";
import { LoginPage } from "../features/auth/LoginPage";
import { DashboardPage } from "../features/dashboard/DashboardPage";
import { OrganizationPage } from "../features/organization/OrganizationPage";
import { InvoiceDetailsPage } from "../features/billing/InvoiceDetailsPage";
import { InvoicesPage } from "../features/billing/InvoicesPage";
import { ApiExplorerPage } from "../features/explorer/ApiExplorerPage";
import { LoadingState } from "../shared/components/ui";
import { AppLayout } from "../shared/layout/AppLayout";

const UsagePage = lazy(() => import("../features/usage/UsagePage").then((module) => ({
  default: module.UsagePage,
})));

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="applications" element={<ApplicationsPage />} />
            <Route path="applications/:applicationId" element={<ApplicationDetailsPage />} />
            <Route path="usage" element={<Suspense fallback={<LoadingState />}><UsagePage /></Suspense>} />
            <Route path="invoices" element={<InvoicesPage />} />
            <Route path="explorer" element={<ApiExplorerPage />} />
            <Route path="billing/:invoiceId" element={<InvoiceDetailsPage />} />
            <Route path="organization" element={<OrganizationPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
