import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { Modal } from "@/components/ui/Modal";
import { CANDIDATE_EDIT_ROLES, CANDIDATE_STATUS_LABELS, CANDIDATE_STATUS_OPTIONS } from "@/constants/candidates";
import { candidatesService, type CandidateListItem } from "@/services/candidatesService";
import "./CandidateList.css";

function formatDate(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString();
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case "Placed":
      return "candidates-badge--placed";
    case "InProcess":
      return "candidates-badge--in-process";
    case "Available":
      return "candidates-badge--available";
    case "OnHold":
      return "candidates-badge--on-hold";
    case "Blacklisted":
      return "candidates-badge--blacklisted";
    default:
      return "candidates-badge--new";
  }
}

export default function CandidateList() {
  const { user: currentUser } = useAuth();
  const navigate = useNavigate();
  const canEdit = !!currentUser && CANDIDATE_EDIT_ROLES.includes(currentUser.role);

  const [candidates, setCandidates] = useState<CandidateListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  const [pendingDelete, setPendingDelete] = useState<CandidateListItem | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [pageMessage, setPageMessage] = useState<string | null>(null);

  const loadCandidates = async () => {
    setIsLoading(true);
    setLoadError(null);

    const response = await candidatesService.getAll();

    if (!response.success || !response.data) {
      setLoadError(response.message || "Unable to load candidates.");
      setIsLoading(false);
      return;
    }

    setCandidates(response.data);
    setIsLoading(false);
  };

  useEffect(() => {
    loadCandidates();
  }, []);

  useEffect(() => {
    if (!pageMessage) return;
    const timer = window.setTimeout(() => setPageMessage(null), 4000);
    return () => window.clearTimeout(timer);
  }, [pageMessage]);

  const filteredCandidates = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();

    return candidates.filter((c) => {
      const matchesTerm =
        !term ||
        c.fullName.toLowerCase().includes(term) ||
        c.email.toLowerCase().includes(term) ||
        (c.currentCompany ?? "").toLowerCase().includes(term) ||
        c.skills.some((skill) => skill.toLowerCase().includes(term));

      const matchesStatus = !statusFilter || c.status === statusFilter;

      return matchesTerm && matchesStatus;
    });
  }, [candidates, searchTerm, statusFilter]);

  const confirmDelete = async () => {
    if (!pendingDelete) return;
    setActionError(null);

    const response = await candidatesService.remove(pendingDelete.id);

    if (!response.success) {
      setActionError(response.message || "Unable to delete this candidate.");
      setPendingDelete(null);
      return;
    }

    setPageMessage(response.message || "Candidate deleted.");
    setPendingDelete(null);
    await loadCandidates();
  };

  return (
    <div className="container py-4">
      <div className="candidates-header">
        <div>
          <h1 className="h4 mb-1" style={{ color: "var(--itm-primary)" }}>
            Candidate Master
          </h1>
          <p className="text-muted mb-0">Search, review and manage every candidate profile in the system.</p>
        </div>
        {canEdit && (
          <button type="button" className="candidates-add-btn" onClick={() => navigate("/candidates/new")}>
            <i className="bi bi-plus-lg" aria-hidden="true" />
            Add Candidate
          </button>
        )}
      </div>

      {pageMessage && (
        <div className="candidates-alert candidates-alert--success" role="status">
          <i className="bi bi-check-circle-fill" aria-hidden="true" />
          <span>{pageMessage}</span>
        </div>
      )}

      {actionError && (
        <div className="candidates-alert candidates-alert--error" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{actionError}</span>
        </div>
      )}

      <div className="candidates-toolbar">
        <div className="candidates-search">
          <i className="bi bi-search" aria-hidden="true" />
          <input
            type="text"
            placeholder="Search by name, email, company or skill..."
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            aria-label="Search candidates"
          />
        </div>

        <select
          className="form-select candidates-status-filter"
          value={statusFilter}
          onChange={(event) => setStatusFilter(event.target.value)}
          aria-label="Filter by status"
        >
          <option value="">All statuses</option>
          {CANDIDATE_STATUS_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div className="candidates-table-card">
        {isLoading ? (
          <div className="candidates-empty">
            <span
              className="login-spinner"
              style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
            />
            <span>Loading candidates...</span>
          </div>
        ) : loadError ? (
          <div className="candidates-empty candidates-empty--error">
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
            <span>{loadError}</span>
          </div>
        ) : filteredCandidates.length === 0 ? (
          <div className="candidates-empty">
            <i className="bi bi-person-lines-fill" aria-hidden="true" />
            <span>No candidates found.</span>
          </div>
        ) : (
          <table className="table candidates-table align-middle mb-0">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Location</th>
                <th>Experience</th>
                <th>Skills</th>
                <th>Status</th>
                <th>Owner</th>
                <th>Added</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredCandidates.map((row) => (
                <tr key={row.id}>
                  <td>
                    <button type="button" className="candidates-name-link" onClick={() => navigate(`/candidates/${row.id}`)}>
                      {row.fullName}
                    </button>
                    {row.title && <div className="candidates-name-subtitle">{row.title}</div>}
                  </td>
                  <td>{row.email}</td>
                  <td>{row.currentLocation || "—"}</td>
                  <td>{row.totalExperienceYears} yrs</td>
                  <td>
                    <div className="candidates-skill-chips">
                      {row.skills.slice(0, 3).map((skill) => (
                        <span key={skill} className="candidates-skill-chip">
                          {skill}
                        </span>
                      ))}
                      {row.skills.length > 3 && <span className="candidates-skill-chip">+{row.skills.length - 3}</span>}
                    </div>
                  </td>
                  <td>
                    <span className={`candidates-badge ${statusBadgeClass(row.status)}`}>
                      {CANDIDATE_STATUS_LABELS[row.status] ?? row.status}
                    </span>
                  </td>
                  <td>{row.ownerRecruiterName || "—"}</td>
                  <td>{formatDate(row.createdAtUtc)}</td>
                  <td>
                    <div className="candidates-row-actions">
                      <button
                        type="button"
                        className="candidates-icon-btn"
                        onClick={() => navigate(`/candidates/${row.id}`)}
                        aria-label={`View ${row.fullName}`}
                        title="View"
                      >
                        <i className="bi bi-eye-fill" aria-hidden="true" />
                      </button>
                      {canEdit && (
                        <>
                          <button
                            type="button"
                            className="candidates-icon-btn"
                            onClick={() => navigate(`/candidates/${row.id}/edit`)}
                            aria-label={`Edit ${row.fullName}`}
                            title="Edit"
                          >
                            <i className="bi bi-pencil-fill" aria-hidden="true" />
                          </button>
                          <button
                            type="button"
                            className="candidates-icon-btn candidates-icon-btn--danger"
                            onClick={() => setPendingDelete(row)}
                            aria-label={`Delete ${row.fullName}`}
                            title="Delete"
                          >
                            <i className="bi bi-trash-fill" aria-hidden="true" />
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {pendingDelete && (
        <Modal title="Delete Candidate" onClose={() => setPendingDelete(null)} size="sm">
          <p className="mb-0">
            Are you sure you want to delete <strong>{pendingDelete.fullName}</strong>? This can't be undone from this
            screen.
          </p>
          <div className="candidates-confirm-actions">
            <button type="button" className="candidates-btn candidates-btn--ghost" onClick={() => setPendingDelete(null)}>
              Cancel
            </button>
            <button type="button" className="candidates-btn candidates-btn--danger" onClick={confirmDelete}>
              Delete Candidate
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
}
