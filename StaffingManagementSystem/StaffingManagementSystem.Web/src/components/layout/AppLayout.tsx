import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { ITMLogo } from "@/components/brand/ITMLogo";
import { useAuth } from "@/context/AuthContext";
import { ROLE_LABELS, USER_MANAGEMENT_VIEW_ROLES } from "@/constants/roles";
import { CANDIDATE_VIEW_ROLES } from "@/constants/candidates";
import "./AppLayout.css";

/** Shared top navigation shell wrapping every authenticated page. */
export function AppLayout({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const canViewUsers = !!user && USER_MANAGEMENT_VIEW_ROLES.includes(user.role);
  const canViewCandidates = !!user && CANDIDATE_VIEW_ROLES.includes(user.role);

  return (
    <div className="app-shell">
      <header className="app-topbar">
        <div className="app-topbar__brand">
          <ITMLogo height={28} />
          <span className="app-topbar__title">Staffing Management System</span>
        </div>

        <nav className="app-topbar__nav">
          <NavLink to="/dashboard" className={({ isActive }) => `app-topbar__link${isActive ? " is-active" : ""}`}>
            <i className="bi bi-speedometer2" aria-hidden="true" />
            Dashboard
          </NavLink>
          {canViewCandidates && (
            <NavLink to="/candidates" className={({ isActive }) => `app-topbar__link${isActive ? " is-active" : ""}`}>
              <i className="bi bi-person-lines-fill" aria-hidden="true" />
              Candidates
            </NavLink>
          )}
          {canViewCandidates && (
            <NavLink to="/reports" className={({ isActive }) => `app-topbar__link${isActive ? " is-active" : ""}`}>
              <i className="bi bi-bar-chart-line" aria-hidden="true" />
              Reports
            </NavLink>
          )}
          {canViewUsers && (
            <NavLink to="/users" className={({ isActive }) => `app-topbar__link${isActive ? " is-active" : ""}`}>
              <i className="bi bi-people" aria-hidden="true" />
              Users
            </NavLink>
          )}
        </nav>

        <div className="app-topbar__user">
          <div className="app-topbar__user-info">
            <span className="app-topbar__user-name">{user?.fullName}</span>
            <span className="app-topbar__user-role">{user ? (ROLE_LABELS[user.role] ?? user.role) : ""}</span>
          </div>
          <button type="button" className="app-topbar__signout" onClick={logout}>
            <i className="bi bi-box-arrow-right" aria-hidden="true" />
            Sign Out
          </button>
        </div>
      </header>

      <main className="app-content">{children}</main>
    </div>
  );
}
