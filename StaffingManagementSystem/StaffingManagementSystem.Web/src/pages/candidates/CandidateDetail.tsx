import { useEffect, useState, type ChangeEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { Modal } from "@/components/ui/Modal";
import { CANDIDATE_EDIT_ROLES, CANDIDATE_STATUS_LABELS, GENDER_LABELS } from "@/constants/candidates";
import { candidatesService, type CandidateAttachment, type CandidateDetail as CandidateDetailData } from "@/services/candidatesService";
import "./CandidateDetail.css";

function formatDate(value?: string | null, options?: Intl.DateTimeFormatOptions): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString(undefined, options);
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function CandidateDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user: currentUser } = useAuth();
  const canEdit = !!currentUser && CANDIDATE_EDIT_ROLES.includes(currentUser.role);

  const [candidate, setCandidate] = useState<CandidateDetailData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [noteText, setNoteText] = useState("");
  const [noteError, setNoteError] = useState<string | null>(null);
  const [isSavingNote, setIsSavingNote] = useState(false);

  const [attachments, setAttachments] = useState<CandidateAttachment[]>([]);
  const [attachmentsError, setAttachmentsError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isUploadingResume, setIsUploadingResume] = useState(false);
  const [pendingDeleteAttachment, setPendingDeleteAttachment] = useState<CandidateAttachment | null>(null);

  const resume = attachments.find((a) => a.type === "Resume") ?? null;
  const otherAttachments = attachments.filter((a) => a.type !== "Resume");

  const loadCandidate = async () => {
    if (!id) return;
    setIsLoading(true);
    setLoadError(null);

    const response = await candidatesService.getById(id);

    if (!response.success || !response.data) {
      setLoadError(response.message || "Unable to load this candidate.");
      setIsLoading(false);
      return;
    }

    setCandidate(response.data);
    setIsLoading(false);
  };

  const loadAttachments = async () => {
    if (!id) return;
    const response = await candidatesService.getAttachments(id);

    if (!response.success || !response.data) {
      setAttachmentsError(response.message || "Unable to load attachments.");
      return;
    }

    setAttachments(response.data);
  };

  useEffect(() => {
    loadCandidate();
    loadAttachments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleFileSelected = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file || !id) return;

    setIsUploading(true);
    setAttachmentsError(null);

    const response = await candidatesService.uploadAttachment(id, file);

    if (!response.success) {
      setAttachmentsError(response.message || "Unable to upload this file.");
      setIsUploading(false);
      return;
    }

    setIsUploading(false);
    await loadAttachments();
  };

  const handleResumeFileSelected = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file || !id) return;

    setIsUploadingResume(true);
    setAttachmentsError(null);

    const response = await candidatesService.uploadResume(id, file);

    if (!response.success) {
      setAttachmentsError(response.message || "Unable to upload the resume.");
      setIsUploadingResume(false);
      return;
    }

    setIsUploadingResume(false);
    await loadAttachments();
  };

  const handleDownload = async (attachment: CandidateAttachment) => {
    if (!id) return;
    setAttachmentsError(null);

    try {
      await candidatesService.downloadAttachment(id, attachment.id, attachment.fileName);
    } catch {
      setAttachmentsError("Unable to download this file.");
    }
  };

  const confirmDeleteAttachment = async () => {
    if (!id || !pendingDeleteAttachment) return;
    setAttachmentsError(null);

    const response = await candidatesService.removeAttachment(id, pendingDeleteAttachment.id);

    if (!response.success) {
      setAttachmentsError(response.message || "Unable to delete this attachment.");
      setPendingDeleteAttachment(null);
      return;
    }

    setPendingDeleteAttachment(null);
    await loadAttachments();
  };

  const submitNote = async () => {
    if (!id || !noteText.trim()) return;
    setIsSavingNote(true);
    setNoteError(null);

    const response = await candidatesService.addNote(id, noteText.trim());

    if (!response.success) {
      setNoteError(response.message || "Unable to add this note.");
      setIsSavingNote(false);
      return;
    }

    setNoteText("");
    setIsSavingNote(false);
    await loadCandidate();
  };

  if (isLoading) {
    return (
      <div className="container py-4">
        <div className="candidate-detail-loading">
          <span
            className="login-spinner"
            style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
          />
          <span>Loading candidate...</span>
        </div>
      </div>
    );
  }

  if (loadError || !candidate) {
    return (
      <div className="container py-4">
        <div className="candidate-detail-alert" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{loadError || "Candidate not found."}</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container py-4">
      <div className="candidate-detail-header">
        <div>
          <button type="button" className="candidate-detail-back" onClick={() => navigate("/candidates")}>
            <i className="bi bi-arrow-left" aria-hidden="true" />
            Back to Candidates
          </button>
          <h1 className="h4 mb-1 mt-2" style={{ color: "var(--itm-primary)" }}>
            {candidate.fullName}
          </h1>
          {candidate.title && <p className="candidate-detail-title-line mb-1">{candidate.title}</p>}
          <p className="text-muted mb-0">{candidate.email}</p>
        </div>
        {canEdit && (
          <button type="button" className="candidate-detail-edit-btn" onClick={() => navigate(`/candidates/${candidate.id}/edit`)}>
            <i className="bi bi-pencil-fill" aria-hidden="true" />
            Edit Candidate
          </button>
        )}
      </div>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Personal Details</h2>
        <dl className="candidate-detail-grid">
          <div>
            <dt>Status</dt>
            <dd>{CANDIDATE_STATUS_LABELS[candidate.status] ?? candidate.status}</dd>
          </div>
          <div>
            <dt>Phone</dt>
            <dd>{candidate.phone || "—"}</dd>
          </div>
          <div>
            <dt>Current Location</dt>
            <dd>{candidate.currentLocation || "—"}</dd>
          </div>
          <div>
            <dt>Address</dt>
            <dd>{candidate.address || "—"}</dd>
          </div>
          <div>
            <dt>Date of Birth</dt>
            <dd>{formatDate(candidate.dateOfBirth)}</dd>
          </div>
          <div>
            <dt>Gender</dt>
            <dd>{candidate.gender ? (GENDER_LABELS[candidate.gender] ?? candidate.gender) : "—"}</dd>
          </div>
          <div>
            <dt>LinkedIn</dt>
            <dd>
              {candidate.linkedInUrl ? (
                <a href={candidate.linkedInUrl} target="_blank" rel="noreferrer">
                  View Profile <i className="bi bi-box-arrow-up-right" aria-hidden="true" />
                </a>
              ) : (
                "—"
              )}
            </dd>
          </div>
          <div>
            <dt>Source</dt>
            <dd>
              {candidate.source === "Other"
                ? `Other: ${candidate.otherSourceText || "—"}`
                : candidate.source || "—"}
            </dd>
          </div>
          <div>
            <dt>Owner Recruiter</dt>
            <dd>{candidate.ownerRecruiterName || "—"}</dd>
          </div>
          <div>
            <dt>Total Experience</dt>
            <dd>{candidate.totalExperienceYears} yrs</dd>
          </div>
          <div>
            <dt>Added</dt>
            <dd>{formatDate(candidate.createdAtUtc)}</dd>
          </div>
        </dl>
      </section>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Skills</h2>
        {candidate.skills.length === 0 ? (
          <p className="candidate-detail-empty">No skills recorded.</p>
        ) : (
          <div className="candidate-detail-skill-chips">
            {candidate.skills.map((skill) => (
              <span key={skill.id} className="candidate-detail-skill-chip">
                {skill.skillName}
                {skill.proficiency ? ` · ${skill.proficiency}` : ""}
                {skill.yearsOfExperience != null ? ` · ${skill.yearsOfExperience} yrs` : ""}
              </span>
            ))}
          </div>
        )}
      </section>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Work Experience</h2>
        {candidate.experience.length === 0 ? (
          <p className="candidate-detail-empty">No work experience recorded.</p>
        ) : (
          <div className="candidate-detail-list">
            {candidate.experience.map((exp) => (
              <div className="candidate-detail-item" key={exp.id}>
                <div className="candidate-detail-item__title">
                  {exp.jobTitle} · {exp.companyName}
                </div>
                <div className="candidate-detail-item__meta">
                  {formatDate(exp.startDate, { year: "numeric", month: "short" })} —{" "}
                  {exp.isCurrent ? "Present" : formatDate(exp.endDate, { year: "numeric", month: "short" })}
                  {exp.employmentType ? ` · ${exp.employmentType}` : ""}
                  {exp.location ? ` · ${exp.location}` : ""}
                </div>
                {exp.description && <p className="candidate-detail-item__desc">{exp.description}</p>}
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Education</h2>
        {candidate.education.length === 0 ? (
          <p className="candidate-detail-empty">No education recorded.</p>
        ) : (
          <div className="candidate-detail-list">
            {candidate.education.map((edu) => (
              <div className="candidate-detail-item" key={edu.id}>
                <div className="candidate-detail-item__title">
                  {edu.degree} · {edu.institution}
                </div>
                <div className="candidate-detail-item__meta">
                  {edu.startYear ?? "—"} — {edu.isExpected ? "Expected" : (edu.endYear ?? "—")}
                  {edu.fieldOfStudy ? ` · ${edu.fieldOfStudy}` : ""}
                  {edu.grade ? ` · ${edu.grade}` : ""}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Projects</h2>
        {candidate.projects.length === 0 ? (
          <p className="candidate-detail-empty">No projects recorded.</p>
        ) : (
          <div className="candidate-detail-list">
            {candidate.projects.map((proj) => (
              <div className="candidate-detail-item" key={proj.id}>
                <div className="candidate-detail-item__title">
                  {proj.projectName}
                  {proj.role ? ` · ${proj.role}` : ""}
                </div>
                <div className="candidate-detail-item__meta">
                  {proj.durationText || ""}
                  {proj.technologiesUsed ? ` · ${proj.technologiesUsed}` : ""}
                </div>
                {proj.description && <p className="candidate-detail-item__desc">{proj.description}</p>}
              </div>
            ))}
          </div>
        )}
      </section>

      {attachmentsError && (
        <div className="candidate-detail-alert candidate-detail-alert--inline" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{attachmentsError}</span>
        </div>
      )}

      <section className="candidate-detail-section">
        <div className="candidate-detail-section__header">
          <h2 className="candidate-detail-section__title mb-0">Resume</h2>
          {canEdit && (
            <label className="candidate-detail-upload-btn">
              {isUploadingResume && <span className="login-spinner" aria-hidden="true" />}
              <i className="bi bi-upload" aria-hidden="true" />
              {resume ? "Replace Resume" : "Upload Resume"}
              <input type="file" hidden onChange={handleResumeFileSelected} disabled={isUploadingResume} />
            </label>
          )}
        </div>

        {!resume ? (
          <p className="candidate-detail-empty">No resume uploaded yet.</p>
        ) : (
          <div className="candidate-detail-attachment">
            <div className="candidate-detail-attachment__info">
              <i className="bi bi-file-earmark-person" aria-hidden="true" />
              <div>
                <div className="candidate-detail-item__title">{resume.fileName}</div>
                <div className="candidate-detail-item__meta">
                  {formatFileSize(resume.fileSizeBytes)} · Uploaded by {resume.uploadedByName || "Unknown"} ·{" "}
                  {formatDate(resume.uploadedAtUtc, { year: "numeric", month: "short", day: "numeric" })}
                </div>
              </div>
            </div>
            <div className="candidate-detail-attachment__actions">
              <button
                type="button"
                className="candidate-detail-icon-btn"
                onClick={() => handleDownload(resume)}
                aria-label={`Download ${resume.fileName}`}
                title="Download"
              >
                <i className="bi bi-download" aria-hidden="true" />
              </button>
              {canEdit && (
                <button
                  type="button"
                  className="candidate-detail-icon-btn candidate-detail-icon-btn--danger"
                  onClick={() => setPendingDeleteAttachment(resume)}
                  aria-label={`Delete ${resume.fileName}`}
                  title="Delete"
                >
                  <i className="bi bi-trash-fill" aria-hidden="true" />
                </button>
              )}
            </div>
          </div>
        )}
      </section>

      <section className="candidate-detail-section">
        <div className="candidate-detail-section__header">
          <h2 className="candidate-detail-section__title mb-0">Other Attachments</h2>
          {canEdit && (
            <label className="candidate-detail-upload-btn">
              {isUploading && <span className="login-spinner" aria-hidden="true" />}
              <i className="bi bi-upload" aria-hidden="true" />
              Upload File
              <input type="file" hidden onChange={handleFileSelected} disabled={isUploading} />
            </label>
          )}
        </div>

        {otherAttachments.length === 0 ? (
          <p className="candidate-detail-empty">No other files attached yet.</p>
        ) : (
          <div className="candidate-detail-list">
            {otherAttachments.map((attachment) => (
              <div className="candidate-detail-attachment" key={attachment.id}>
                <div className="candidate-detail-attachment__info">
                  <i className="bi bi-file-earmark-text" aria-hidden="true" />
                  <div>
                    <div className="candidate-detail-item__title">{attachment.fileName}</div>
                    <div className="candidate-detail-item__meta">
                      {formatFileSize(attachment.fileSizeBytes)} · Uploaded by {attachment.uploadedByName || "Unknown"} ·{" "}
                      {formatDate(attachment.uploadedAtUtc, { year: "numeric", month: "short", day: "numeric" })}
                    </div>
                  </div>
                </div>
                <div className="candidate-detail-attachment__actions">
                  <button
                    type="button"
                    className="candidate-detail-icon-btn"
                    onClick={() => handleDownload(attachment)}
                    aria-label={`Download ${attachment.fileName}`}
                    title="Download"
                  >
                    <i className="bi bi-download" aria-hidden="true" />
                  </button>
                  {canEdit && (
                    <button
                      type="button"
                      className="candidate-detail-icon-btn candidate-detail-icon-btn--danger"
                      onClick={() => setPendingDeleteAttachment(attachment)}
                      aria-label={`Delete ${attachment.fileName}`}
                      title="Delete"
                    >
                      <i className="bi bi-trash-fill" aria-hidden="true" />
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="candidate-detail-section">
        <h2 className="candidate-detail-section__title">Notes</h2>

        {canEdit && (
          <div className="candidate-detail-note-form">
            {noteError && (
              <div className="candidate-detail-alert candidate-detail-alert--inline" role="alert">
                <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
                <span>{noteError}</span>
              </div>
            )}
            <textarea
              className="form-control"
              rows={2}
              placeholder="Add a note about this candidate..."
              value={noteText}
              onChange={(event) => setNoteText(event.target.value)}
            />
            <button
              type="button"
              className="candidate-detail-note-btn"
              onClick={submitNote}
              disabled={isSavingNote || !noteText.trim()}
            >
              {isSavingNote && <span className="login-spinner" aria-hidden="true" />}
              Add Note
            </button>
          </div>
        )}

        {candidate.notes.length === 0 ? (
          <p className="candidate-detail-empty">No notes yet.</p>
        ) : (
          <div className="candidate-detail-list">
            {candidate.notes.map((note) => (
              <div className="candidate-detail-item" key={note.id}>
                <p className="candidate-detail-item__desc mb-1">{note.note}</p>
                <div className="candidate-detail-item__meta">
                  {note.createdByName || "Unknown"} · {formatDate(note.createdAtUtc, { year: "numeric", month: "short", day: "numeric" })}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {pendingDeleteAttachment && (
        <Modal title="Delete Attachment" onClose={() => setPendingDeleteAttachment(null)} size="sm">
          <p className="mb-0">
            Are you sure you want to delete <strong>{pendingDeleteAttachment.fileName}</strong>? This can't be undone.
          </p>
          <div className="candidate-detail-confirm-actions">
            <button
              type="button"
              className="candidate-detail-btn candidate-detail-btn--ghost"
              onClick={() => setPendingDeleteAttachment(null)}
            >
              Cancel
            </button>
            <button type="button" className="candidate-detail-btn candidate-detail-btn--danger" onClick={confirmDeleteAttachment}>
              Delete Attachment
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
}
