import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Modal } from "@/components/ui/Modal";
import { candidatesService, type CandidateListItem } from "@/services/candidatesService";
import "@/pages/candidates/CandidateList.css";
import "./CandidateApprovals.css";

const TABS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "PendingApproval", label: "Pending Approval" },
  { value: "Approved", label: "Approved" },
  { value: "Rejected", label: "Rejected" },
];

function formatDateTime(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}`;
}

export default function CandidateApprovals() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [statusTab, setStatusTab] = useState(() => searchParams.get("tab") ?? "PendingApproval");
  const [candidates, setCandidates] = useState<CandidateListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [pageMessage, setPageMessage] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [pendingReject, setPendingReject] = useState<CandidateListItem | null>(null);
  const [rejectComment, setRejectComment] = useState("");
  const [isRejecting, setIsRejecting] = useState(false);

  const loadCandidates = async () => {
    setIsLoading(true);
    setLoadError(null);

    const response = await candidatesService.getAll({ status: statusTab, page: 1, pageSize: 100 });

    if (!response.success || !response.data) {
      setLoadError(response.message || "Unable to load candidates.");
      setIsLoading(false);
      return;
    }

    setCandidates(response.data.items);
    setIsLoading(false);
  };

  useEffect(() => {
    loadCandidates();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusTab]);

  const handleTabChange = (value: string) => {
    setStatusTab(value);
    setSearchParams({ tab: value }, { replace: true });
  };

  /** Passed as navigation state so the candidate detail page's "Back" button returns here, on the same tab. */
  const returnToState = { returnTo: `/candidate-approvals?tab=${statusTab}`, returnLabel: "Back to Candidate Approvals" };

  useEffect(() => {
    if (!pageMessage) return;
    const timer = window.setTimeout(() => setPageMessage(null), 4000);
    return () => window.clearTimeout(timer);
  }, [pageMessage]);

  const handleApprove = async (candidate: CandidateListItem) => {
    setActionError(null);
    setBusyId(candidate.id);

    const response = await candidatesService.approveCandidate(candidate.id);

    setBusyId(null);
    if (!response.success) {
      setActionError(response.message || "Unable to approve this candidate.");
      return;
    }

    setPageMessage(`${candidate.fullName} approved.`);
    await loadCandidates();
  };

  const handleDownloadResume = async (candidate: CandidateListItem) => {
    setActionError(null);

    const response = await candidatesService.getAttachments(candidate.id);
    if (!response.success || !response.data) {
      setActionError(response.message || "Unable to load attachments.");
      return;
    }

    const resume = response.data.find((a) => a.type === "Resume");
    if (!resume) {
      setActionError("This candidate has no resume on file.");
      return;
    }

    await candidatesService.downloadAttachment(candidate.id, resume.id, resume.fileName);
  };

  const confirmReject = async () => {
    if (!pendingReject || !rejectComment.trim()) return;
    setIsRejecting(true);
    setActionError(null);

    const response = await candidatesService.rejectCandidate(pendingReject.id, rejectComment.trim());

    setIsRejecting(false);

    if (!response.success) {
      setActionError(response.message || "Unable to reject this candidate.");
      setPendingReject(null);
      setRejectComment("");
      return;
    }

    setPageMessage(`${pendingReject.fullName} rejected.`);
    setPendingReject(null);
    setRejectComment("");
    await loadCandidates();
  };

  return (
    <div className="container-fluid py-4 candidate-approvals-page">
      <div className="candidates-header">
        <div>
          <h1 className="h4 mb-1" style={{ color: "var(--itm-primary)" }}>
            Candidate Approvals
          </h1>
          <p className="text-muted mb-0">Review self-registered candidates awaiting approval.</p>
        </div>
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

      <div className="candidate-approvals-tabs">
        {TABS.map((tab) => (
          <button
            key={tab.value}
            type="button"
            className={`candidate-approvals-tab ${statusTab === tab.value ? "is-active" : ""}`}
            onClick={() => handleTabChange(tab.value)}
          >
            {tab.label}
          </button>
        ))}
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
            <span>No candidates in this category.</span>
          </div>
        ) : (
          <table className="table candidates-table align-middle mb-0">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>LinkedIn</th>
                <th>Current Location</th>
                <th>Experience</th>
                <th>Skills</th>
                <th>Registered</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {candidates.map((row) => (
                <tr key={row.id}>
                  <td>
                    <Link to={`/candidates/${row.id}`} state={returnToState} className="candidates-name-link">
                      {row.fullName}
                    </Link>
                  </td>
                  <td>{row.email}</td>
                  <td>{row.phone || "—"}</td>
                  <td>
                    {row.linkedInUrl ? (
                      <a href={row.linkedInUrl} target="_blank" rel="noreferrer">
                        View <i className="bi bi-box-arrow-up-right" aria-hidden="true" />
                      </a>
                    ) : (
                      "—"
                    )}
                  </td>
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
                  <td>{formatDateTime(row.createdAtUtc)}</td>
                  <td>
                    <div className="candidates-row-actions">
                      <Link
                        to={`/candidates/${row.id}`}
                        state={returnToState}
                        className="candidates-icon-btn"
                        aria-label={`View ${row.fullName}`}
                        title="View Profile"
                      >
                        <i className="bi bi-eye-fill" aria-hidden="true" />
                      </Link>
                      <button
                        type="button"
                        className="candidates-icon-btn"
                        onClick={() => handleDownloadResume(row)}
                        aria-label={`Download resume for ${row.fullName}`}
                        title="Download Resume"
                      >
                        <i className="bi bi-download" aria-hidden="true" />
                      </button>
                      {statusTab === "PendingApproval" && (
                        <>
                          <button
                            type="button"
                            className="candidate-approvals-btn candidate-approvals-btn--approve"
                            onClick={() => handleApprove(row)}
                            disabled={busyId === row.id}
                          >
                            Approve
                          </button>
                          <button
                            type="button"
                            className="candidate-approvals-btn candidate-approvals-btn--reject"
                            onClick={() => setPendingReject(row)}
                            disabled={busyId === row.id}
                          >
                            Reject
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

      {pendingReject && (
        <Modal title="Reject Candidate" onClose={() => setPendingReject(null)} size="sm">
          <p className="mb-2">
            Rejecting <strong>{pendingReject.fullName}</strong>. A comment is required — it will be included in the
            email sent to the candidate.
          </p>
          <textarea
            className="form-control"
            rows={4}
            placeholder="Reason for rejection..."
            value={rejectComment}
            onChange={(event) => setRejectComment(event.target.value)}
            autoFocus
          />
          <div className="candidates-confirm-actions">
            <button
              type="button"
              className="candidates-btn candidates-btn--ghost"
              onClick={() => {
                setPendingReject(null);
                setRejectComment("");
              }}
            >
              Cancel
            </button>
            <button
              type="button"
              className="candidates-btn candidates-btn--danger"
              onClick={confirmReject}
              disabled={!rejectComment.trim() || isRejecting}
            >
              {isRejecting ? "Rejecting..." : "Reject Candidate"}
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
}
