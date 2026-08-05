import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { ROLE_LABELS, USER_MANAGEMENT_VIEW_ROLES } from "@/constants/roles";
import { CANDIDATE_STATUS_LABELS } from "@/constants/candidates";
import { dashboardService, type DashboardCandidate, type DashboardSummary, type NameCount } from "@/services/dashboardService";
import "@/pages/candidates/CandidateList.css";
import "./Dashboard.css";

function formatDate(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString();
}

/** Small labeled number, optionally a link, used for the single-metric highlight cards. */
function CountTile({ label, value, to, icon }: { label: string; value: number; to?: string; icon: string }) {
  const content = (
    <div className="dashboard-tile">
      <i className={`bi ${icon} dashboard-tile__icon`} aria-hidden="true" />
      <div className="dashboard-tile__value">{value}</div>
      <div className="dashboard-tile__label">{label}</div>
    </div>
  );

  return to ? (
    <Link to={to} className="dashboard-card dashboard-card--link">
      {content}
    </Link>
  ) : (
    <div className="dashboard-card">{content}</div>
  );
}

/** Horizontal bar-list of name/count pairs (status breakdown, recruiter workload, top skills). */
function CountListCard({ title, items, emptyText }: { title: string; items?: NameCount[]; emptyText: string }) {
  const max = Math.max(1, ...(items ?? []).map((i) => i.count));

  return (
    <div className="dashboard-card">
      <h3 className="dashboard-card__title">{title}</h3>
      {!items || items.length === 0 ? (
        <p className="dashboard-empty">{emptyText}</p>
      ) : (
        <ul className="dashboard-bar-list">
          {items.map((item) => (
            <li key={item.name} className="dashboard-bar-list__row">
              <span className="dashboard-bar-list__name">{CANDIDATE_STATUS_LABELS[item.name] ?? item.name}</span>
              <div className="dashboard-bar-list__track">
                <div className="dashboard-bar-list__fill" style={{ width: `${(item.count / max) * 100}%` }} />
              </div>
              <span className="dashboard-bar-list__count">{item.count}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

/** Skill chip cloud, reusing the Candidate Master list's existing chip styling. */
function SkillChipsCard({ items }: { items?: NameCount[] }) {
  return (
    <div className="dashboard-card">
      <h3 className="dashboard-card__title">Top Skills</h3>
      {!items || items.length === 0 ? (
        <p className="dashboard-empty">No skills recorded yet.</p>
      ) : (
        <div className="candidates-skill-chips">
          {items.map((item) => (
            <span key={item.name} className="candidates-skill-chip">
              {item.name} · {item.count}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}

/** Mini list of candidates for "recently X" widgets. */
function CandidateMiniListCard({
  title,
  items,
  emptyText,
}: {
  title: string;
  items?: DashboardCandidate[];
  emptyText: string;
}) {
  return (
    <div className="dashboard-card">
      <h3 className="dashboard-card__title">{title}</h3>
      {!items || items.length === 0 ? (
        <p className="dashboard-empty">{emptyText}</p>
      ) : (
        <ul className="dashboard-mini-list">
          {items.map((item) => (
            <li key={item.id} className="dashboard-mini-list__row">
              <Link to={`/candidates/${item.id}`} className="dashboard-mini-list__name">
                {item.fullName}
              </Link>
              <span className="dashboard-mini-list__meta">
                {item.ownerRecruiterName ? `${item.ownerRecruiterName} · ` : ""}
                {formatDate(item.eventAtUtc)}
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

/** Landing page reached after a successful login — widgets vary by role. */
export default function Dashboard() {
  const { user } = useAuth();
  const canViewUsers = !!user && USER_MANAGEMENT_VIEW_ROLES.includes(user.role);

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  useEffect(() => {
    dashboardService.getSummary().then((response) => {
      if (!response.success || !response.data) {
        setLoadError(response.message || "Unable to load dashboard data.");
        setIsLoading(false);
        return;
      }
      setSummary(response.data);
      setIsLoading(false);
    });
  }, []);

  return (
    <div className="container py-5">
      <h1 className="h3 mb-1" style={{ color: "var(--itm-primary)" }}>
        Welcome{user ? `, ${user.fullName}` : ""}
      </h1>
      <p className="text-muted mb-4">Signed in as {user ? (ROLE_LABELS[user.role] ?? user.role) : ""}.</p>

      {isLoading ? (
        <div className="dashboard-empty">
          <span
            className="login-spinner"
            style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
          />
          <span>Loading dashboard...</span>
        </div>
      ) : loadError ? (
        <div className="candidates-alert candidates-alert--error" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{loadError}</span>
        </div>
      ) : (
        <>
          {user?.role === "Admin" && summary && (
            <div className="dashboard-grid">
              <CountTile
                label="Pending Approvals"
                value={summary.pendingApprovalsCount ?? 0}
                to="/candidate-approvals"
                icon="bi-person-check"
              />
              <CountListCard title="Candidate Pipeline" items={summary.statusCounts} emptyText="No candidates yet." />
              <CountListCard
                title="Recruiter Workload"
                items={summary.recruiterWorkload}
                emptyText="No candidates assigned to a recruiter yet."
              />
              <SkillChipsCard items={summary.topSkills} />
              <CandidateMiniListCard
                title="Recently Approved"
                items={summary.recentlyApproved}
                emptyText="No approvals yet."
              />
              <CandidateMiniListCard
                title="Recently Rejected"
                items={summary.recentlyRejected}
                emptyText="No rejections yet."
              />
              <CandidateMiniListCard
                title="Recent Registrations (7 days)"
                items={summary.recentRegistrations}
                emptyText="No new self-registrations in the last 7 days."
              />
            </div>
          )}

          {user?.role === "Recruiter" && summary && (
            <div className="dashboard-grid">
              <CountTile
                label="My Candidates"
                value={summary.myCandidatesCount ?? 0}
                to={user ? `/candidates?owner=${user.id}` : "/candidates"}
                icon="bi-people-fill"
              />
              <CountTile
                label="My In Process"
                value={summary.myInProcessCount ?? 0}
                to={user ? `/candidates?owner=${user.id}` : "/candidates"}
                icon="bi-arrow-repeat"
              />
              <CountListCard
                title="My Candidates by Status"
                items={summary.myCandidatesByStatus}
                emptyText="You don't own any candidates yet."
              />
              <CandidateMiniListCard
                title="Recently Added (7 days)"
                items={summary.recentlyAddedSystemWide}
                emptyText="No new candidates in the last 7 days."
              />
              <Link to={user ? `/candidates?owner=${user.id}` : "/candidates"} className="dashboard-card dashboard-card--link">
                <div className="dashboard-tile">
                  <i className="bi bi-person-lines-fill dashboard-tile__icon" aria-hidden="true" />
                  <div className="dashboard-tile__label">Candidate Master (My Candidates)</div>
                </div>
              </Link>
              <Link to="/reports" className="dashboard-card dashboard-card--link">
                <div className="dashboard-tile">
                  <i className="bi bi-bar-chart-line dashboard-tile__icon" aria-hidden="true" />
                  <div className="dashboard-tile__label">Reports</div>
                </div>
              </Link>
            </div>
          )}

          {user?.role === "Viewer" && summary && (
            <div className="dashboard-grid">
              <CountTile
                label="Total Candidates"
                value={summary.totalVisibleCandidates ?? 0}
                to="/candidates"
                icon="bi-person-lines-fill"
              />
              <CountListCard title="Candidate Pipeline" items={summary.statusCounts} emptyText="No candidates yet." />
              <Link to="/candidates" className="dashboard-card dashboard-card--link">
                <div className="dashboard-tile">
                  <i className="bi bi-search dashboard-tile__icon" aria-hidden="true" />
                  <div className="dashboard-tile__label">Candidate Master</div>
                </div>
              </Link>
            </div>
          )}

          {canViewUsers && (
            <div className="dashboard-grid dashboard-grid--secondary">
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
        </>
      )}
    </div>
  );
}
