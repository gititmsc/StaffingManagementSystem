import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { CANDIDATE_EDIT_ROLES, CANDIDATE_STATUS_LABELS } from "@/constants/candidates";
import { candidatesService, type CandidateDetail as CandidateDetailData } from "@/services/candidatesService";
import "./CandidateDetail.css";

function formatDate(value?: string | null, options?: Intl.DateTimeFormatOptions): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString(undefined, options);
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

  useEffect(() => {
    loadCandidate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

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
            <dd>{candidate.gender || "—"}</dd>
          </div>
          <div>
            <dt>Source</dt>
            <dd>{candidate.source || "—"}</dd>
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
    </div>
  );
}
