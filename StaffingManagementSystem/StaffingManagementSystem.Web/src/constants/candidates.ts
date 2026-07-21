/** Mirrors StaffingManagementSystem.Core.Enums.CandidateStatus. */
export const CANDIDATE_STATUS_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "New", label: "New" },
  { value: "Available", label: "Available" },
  { value: "InProcess", label: "In Process" },
  { value: "Placed", label: "Placed" },
  { value: "OnHold", label: "On Hold" },
  { value: "Blacklisted", label: "Blacklisted" },
];

export const CANDIDATE_STATUS_LABELS: Record<string, string> = Object.fromEntries(
  CANDIDATE_STATUS_OPTIONS.map((s) => [s.value, s.label])
);

/** Mirrors StaffingManagementSystem.Core.Enums.CandidateSource. */
export const CANDIDATE_SOURCE_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "Referral", label: "Referral" },
  { value: "JobPortal", label: "Job Portal" },
  { value: "LinkedIn", label: "LinkedIn" },
  { value: "WalkIn", label: "Walk-in" },
  { value: "Agency", label: "Agency" },
  { value: "Website", label: "Website" },
  { value: "Other", label: "Other" },
];

/** Mirrors StaffingManagementSystem.Core.Enums.ProficiencyLevel. */
export const PROFICIENCY_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "Beginner", label: "Beginner" },
  { value: "Intermediate", label: "Intermediate" },
  { value: "Expert", label: "Expert" },
];

/** Mirrors StaffingManagementSystem.Core.Enums.EmploymentType. */
export const EMPLOYMENT_TYPE_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "FullTime", label: "Full-time" },
  { value: "Contract", label: "Contract" },
  { value: "Internship", label: "Internship" },
];

/** Candidate personal-details gender options. */
export const GENDER_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "Male", label: "Male" },
  { value: "Female", label: "Female" },
  { value: "Other", label: "Other" },
  { value: "PreferNotToSay", label: "Prefer not to say" },
];

export const GENDER_LABELS: Record<string, string> = Object.fromEntries(
  GENDER_OPTIONS.map((g) => [g.value, g.label])
);

/** Matches the CreateCandidateRequestDto/UpdateCandidateRequestDto RegularExpression rule. */
export const PHONE_PATTERN = /^\+?[0-9\s\-()]{7,20}$/;

/**
 * Roles allowed to create/edit/delete candidates and their attachments/resume — also the
 * roles allowed to download attachments/resume (Viewer can view candidates but not download).
 */
export const CANDIDATE_EDIT_ROLES = ["Admin", "Recruiter"];

/** Every authenticated role can view the candidate master (read-only for Viewer). */
export const CANDIDATE_VIEW_ROLES = ["Admin", "Recruiter", "Viewer"];

/** Reports are available to Admin and Recruiter only — Viewer has no reporting access. */
export const REPORT_VIEW_ROLES = ["Admin", "Recruiter"];
