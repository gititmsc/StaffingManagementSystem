import { Navigate, Route, Routes } from "react-router-dom";
import Login from "@/pages/login/Login";
import ForgotPassword from "@/pages/forgot-password/ForgotPassword";
import ResetPassword from "@/pages/reset-password/ResetPassword";
import Dashboard from "@/pages/dashboard/Dashboard";
import { ProtectedRoute } from "@/routes/ProtectedRoute";

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
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
