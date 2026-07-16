import { Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { ROLE_LABELS, USER_MANAGEMENT_VIEW_ROLES } from "@/constants/roles";

/** Landing page reached after a successful login. */
export default function Dashboard() {
  const { user } = useAuth();
  const canViewUsers = !!user && USER_MANAGEMENT_VIEW_ROLES.includes(user.role);

  return (
    <div className="container py-5">
      <h1 className="h3 mb-1" style={{ color: "var(--itm-primary)" }}>
        Welcome{user ? `, ${user.fullName}` : ""}
      </h1>
      <p className="text-muted mb-4">
        Signed in as {user ? (ROLE_LABELS[user.role] ?? user.role) : ""}.
      </p>

      <div className="row g-3">
        {canViewUsers && (
          <div className="col-12 col-sm-6 col-lg-4">
            <Link to="/users" className="text-decoration-none">
              <div className="card h-100 shadow-sm border-0" style={{ borderRadius: "var(--itm-radius-card)" }}>
                <div className="card-body">
                  <i className="bi bi-people-fill fs-3" style={{ color: "var(--itm-primary)" }} aria-hidden="true" />
                  <h5 className="card-title mt-3 mb-1">User & Role Management</h5>
                  <p className="card-text text-muted small mb-0">
                    Create, edit, activate/deactivate, and manage roles for user accounts.
                  </p>
                </div>
              </div>
            </Link>
          </div>
        )}

        <div className="col-12 col-sm-6 col-lg-4">
          <div className="card h-100 shadow-sm border-0 opacity-75" style={{ borderRadius: "var(--itm-radius-card)" }}>
            <div className="card-body">
              <i className="bi bi-person-vcard-fill fs-3" style={{ color: "var(--itm-muted)" }} aria-hidden="true" />
              <h5 className="card-title mt-3 mb-1">Candidate Master</h5>
              <p className="card-text text-muted small mb-0">Coming soon.</p>
            </div>
          </div>
        </div>

        <div className="col-12 col-sm-6 col-lg-4">
          <div className="card h-100 shadow-sm border-0 opacity-75" style={{ borderRadius: "var(--itm-radius-card)" }}>
            <div className="card-body">
              <i className="bi bi-bar-chart-fill fs-3" style={{ color: "var(--itm-muted)" }} aria-hidden="true" />
              <h5 className="card-title mt-3 mb-1">Search & Reports</h5>
              <p className="card-text text-muted small mb-0">Coming soon.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
