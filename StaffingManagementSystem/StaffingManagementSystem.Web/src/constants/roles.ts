/**
 * User roles supported by the Staffing Management System.
 * Mirrors StaffingManagementSystem.Core.Enums.UserRole — keep in sync with the backend.
 */
export const ROLE_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "SuperAdmin", label: "Super Admin" },
  { value: "HRAdmin", label: "HR Admin" },
  { value: "Recruiter", label: "Recruiter" },
  { value: "HiringManager", label: "Hiring Manager" },
  { value: "Viewer", label: "Viewer" },
];

export const ROLE_LABELS: Record<string, string> = Object.fromEntries(
  ROLE_OPTIONS.map((role) => [role.value, role.label])
);

/** Roles allowed to view (but not necessarily edit) the Users administration screen. */
export const USER_MANAGEMENT_VIEW_ROLES = ["SuperAdmin", "HRAdmin"];

/** Only SuperAdmin has create/edit/deactivate/delete rights over user accounts. */
export const USER_MANAGEMENT_EDIT_ROLES = ["SuperAdmin"];
