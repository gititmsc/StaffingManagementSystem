import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { Modal } from "@/components/ui/Modal";
import { CANDIDATE_EDIT_ROLES, CANDIDATE_STATUS_LABELS, CANDIDATE_STATUS_OPTIONS } from "@/constants/candidates";
import { candidatesService, type CandidateListItem } from "@/services/candidatesService";
import "./CandidateList.css";

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];
const DEFAULT_PAGE_SIZE = 25;

/** Columns the backend can sort by, and which direction each starts in on first click. */
const SORTABLE_COLUMNS: Record<string, { label: string; defaultDescending: boolean }> = {
  name: { label: "Name", defaultDescending: false },
  email: { label: "Email", defaultDescending: false },
  experience: { label: "Experience", defaultDescending: true },
  status: { label: "Status", defaultDescending: false },
  added: { label: "Added", defaultDescending: true },
};
const DEFAULT_SORT_BY = "added";
const DEFAULT_SORT_DESCENDING = true;

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
    case "PendingApproval":
      return "candidates-badge--pending";
    case "Rejected":
      return "candidates-badge--rejected";
    default:
      // Includes "Approved" — the Candidate Master list shows a just-approved candidate as
      // "New" (see statusDisplayLabel), so it gets the same badge styling as New too.
      return "candidates-badge--new";
  }
}

/**
 * Candidate Master shows a freshly-approved self-registered candidate as "New" rather than
 * "Approved" — the underlying status stays Approved (the Candidate Approvals screen's
 * "Approved" tab and audit fields still rely on that real value), this is a display-only
 * relabel scoped to this screen.
 */
function statusDisplayLabel(status: string): string {
  if (status === "Approved") return "New";
  return CANDIDATE_STATUS_LABELS[status] ?? status;
}

export default function CandidateList() {
  const { user: currentUser } = useAuth();
  const navigate = useNavigate();
  const canEdit = !!currentUser && CANDIDATE_EDIT_ROLES.includes(currentUser.role);

  const [candidates, setCandidates] = useState<CandidateListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [searchInput, setSearchInput] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [sortBy, setSortBy] = useState(DEFAULT_SORT_BY);
  const [sortDescending, setSortDescending] = useState(DEFAULT_SORT_DESCENDING);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  const [pendingDelete, setPendingDelete] = useState<CandidateListItem | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [pageMessage, setPageMessage] = useState<string | null>(null);

  // Debounce free-text search so we don't hit the server on every keystroke.
  useEffect(() => {
    const timer = window.setTimeout(() => {
      setSearchTerm(searchInput.trim());
      setPage(1);
    }, 350);
    return () => window.clearTimeout(timer);
  }, [searchInput]);

  const loadCandidates = async (targetPage: number = page) => {
    setIsLoading(true);
    setLoadError(null);

    const response = await candidatesService.getAll({
      search: searchTerm || undefined,
      status: statusFilter || undefined,
      sortBy,
      sortDescending,
      page: targetPage,
      pageSize,
    });

    if (!response.success || !response.data) {
      setLoadError(response.message || "Unable to load candidates.");
      setIsLoading(false);
      return;
    }

    setCandidates(response.data.items);
    setTotalCount(response.data.totalCount);
    setTotalPages(response.data.totalPages);
    setPage(response.data.page);
    setIsLoading(false);
  };

  useEffect(() => {
    loadCandidates(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm, statusFilter, sortBy, sortDescending, pageSize, page]);

  useEffect(() => {
    if (!pageMessage) return;
    const timer = window.setTimeout(() => setPageMessage(null), 4000);
    return () => window.clearTimeout(timer);
  }, [pageMessage]);

  const handleStatusFilterChange = (value: string) => {
    setStatusFilter(value);
    setPage(1);
  };

  const handleSortColumnClick = (column: string) => {
    if (sortBy === column) {
      setSortDescending((prev) => !prev);
    } else {
      setSortBy(column);
      setSortDescending(SORTABLE_COLUMNS[column].defaultDescending);
    }
    setPage(1);
  };

  const handlePageSizeChange = (value: number) => {
    setPageSize(value);
    setPage(1);
  };

  const goToPage = (target: number) => {
    if (target < 1 || target > Math.max(totalPages, 1)) return;
    setPage(target);
  };

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

    // If we just deleted the last item on a page beyond the first, step back a page.
    // Changing `page` re-triggers the load effect; if it's unchanged, reload explicitly here.
    const nextPage = candidates.length === 1 && page > 1 ? page - 1 : page;
    if (nextPage === page) {
      await loadCandidates(nextPage);
    } else {
      setPage(nextPage);
    }
  };

  const rangeStart = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = totalCount === 0 ? 0 : Math.min(page * pageSize, totalCount);

  const renderSortableHeader = (column: string) => {
    const isActive = sortBy === column;
    const icon = isActive ? (sortDescending ? "bi-arrow-down" : "bi-arrow-up") : "bi-arrow-down-up";

    return (
      <th
        key={column}
        className="candidates-sortable-th"
        onClick={() => handleSortColumnClick(column)}
        aria-sort={isActive ? (sortDescending ? "descending" : "ascending") : "none"}
      >
        {SORTABLE_COLUMNS[column].label}
        <i className={`bi ${icon} candidates-sort-icon`} aria-hidden="true" />
      </th>
    );
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
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            aria-label="Search candidates"
          />
        </div>

        <select
          className="form-select candidates-status-filter"
          value={statusFilter}
          onChange={(event) => handleStatusFilterChange(event.target.value)}
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
        ) : candidates.length === 0 ? (
          <div className="candidates-empty">
            <i className="bi bi-person-lines-fill" aria-hidden="true" />
            <span>No candidates found.</span>
          </div>
        ) : (
          <>
            <table className="table candidates-table align-middle mb-0">
              <thead>
                <tr>
                  {renderSortableHeader("name")}
                  {renderSortableHeader("email")}
                  <th>Location</th>
                  {renderSortableHeader("experience")}
                  <th>Skills</th>
                  {renderSortableHeader("status")}
                  <th>Owner</th>
                  {renderSortableHeader("added")}
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {candidates.map((row) => (
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
                        {statusDisplayLabel(row.status)}
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

            <div className="candidates-pagination">
              <span className="candidates-pagination__summary">
                Showing {rangeStart}–{rangeEnd} of {totalCount}
              </span>

              <div className="candidates-pagination__right">
                <div className="candidates-page-size">
                  <label htmlFor="candidatesPageSize">Show</label>
                  <select
                    id="candidatesPageSize"
                    className="form-select"
                    value={pageSize}
                    onChange={(event) => handlePageSizeChange(Number(event.target.value))}
                    aria-label="Records per page"
                  >
                    {PAGE_SIZE_OPTIONS.map((size) => (
                      <option key={size} value={size}>
                        {size}
                      </option>
                    ))}
                  </select>
                  <span>per page</span>
                </div>

                {totalPages > 1 && (
                  <div className="candidates-pagination__controls">
                    <button
                      type="button"
                      className="candidates-page-btn"
                      disabled={page <= 1}
                      onClick={() => goToPage(page - 1)}
                    >
                      <i className="bi bi-chevron-left" aria-hidden="true" />
                      Previous
                    </button>
                    <span className="candidates-page-indicator">
                      Page {page} of {totalPages}
                    </span>
                    <button
                      type="button"
                      className="candidates-page-btn"
                      disabled={page >= totalPages}
                      onClick={() => goToPage(page + 1)}
                    >
                      Next
                      <i className="bi bi-chevron-right" aria-hidden="true" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          </>
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
