import { Navigate, Route, Routes } from "react-router-dom";
import Login from "@/pages/login/Login";
import ForgotPassword from "@/pages/forgot-password/ForgotPassword";
import ResetPassword from "@/pages/reset-password/ResetPassword";
import Dashboard from "@/pages/dashboard/Dashboard";
import Users from "@/pages/users/Users";
import CandidateList from "@/pages/candidates/CandidateList";
import CandidateForm from "@/pages/candidates/CandidateForm";
import CandidateDetail from "@/pages/candidates/CandidateDetail";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import { AppLayout } from "@/components/layout/AppLayout";
import { USER_MANAGEMENT_VIEW_ROLES } from "@/constants/roles";
import { CANDIDATE_EDIT_ROLES, CANDIDATE_VIEW_ROLES } from "@/constants/candidates";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<Login />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/reset-password" element={<ResetPassword />} />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Dashboard />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ProtectedRoute roles={USER_MANAGEMENT_VIEW_ROLES}>
            <AppLayout>
              <Users />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/candidates"
        element={
          <ProtectedRoute roles={CANDIDATE_VIEW_ROLES}>
            <AppLayout>
              <CandidateList />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/candidates/new"
        element={
          <ProtectedRoute roles={CANDIDATE_EDIT_ROLES}>
            <AppLayout>
              <CandidateForm />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/candidates/:id/edit"
        element={
          <ProtectedRoute roles={CANDIDATE_EDIT_ROLES}>
            <AppLayout>
              <CandidateForm />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/candidates/:id"
        element={
          <ProtectedRoute roles={CANDIDATE_VIEW_ROLES}>
            <AppLayout>
              <CandidateDetail />
            </AppLayout>
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
