import { BrowserRouter, Navigate, Route, Routes } from "react-router";
import { ApplicationDetailsPage } from "../features/applications/ApplicationDetailsPage";
import { ApplicationsPage } from "../features/applications/ApplicationsPage";
import { ProtectedRoute } from "../features/auth/ProtectedRoute";
import { LoginPage } from "../features/auth/LoginPage";
import { DashboardPage } from "../features/dashboard/DashboardPage";
import { OrganizationPage } from "../features/organization/OrganizationPage";
import { UsagePage } from "../features/usage/UsagePage";
import { AppLayout } from "../shared/layout/AppLayout";

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
            <Route path="usage" element={<UsagePage />} />
            <Route path="organization" element={<OrganizationPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
