import { useAuth } from "@/context/AuthContext";

/** Placeholder landing page reached after a successful login. */
export default function Dashboard() {
  const { user, logout } = useAuth();

  return (
    <div className="container py-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0" style={{ color: "var(--itm-primary)" }}>
          Welcome{user ? `, ${user.fullName}` : ""}
        </h1>
        <button type="button" className="btn btn-outline-secondary" onClick={logout}>
          Sign Out
        </button>
      </div>
      <p className="text-muted">This is a placeholder dashboard for the Staffing Management System.</p>
    </div>
  );
}
