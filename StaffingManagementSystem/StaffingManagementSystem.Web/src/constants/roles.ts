/**
 * User roles supported by the Staffing Management System.
 * Mirrors StaffingManagementSystem.Core.Enums.UserRole — keep in sync with the backend.
 */
export const ROLE_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: "Admin", label: "Admin" },
  { value: "Recruiter", label: "Recruiter" },
  { value: "Viewer", label: "Viewer" },
];

export const ROLE_LABELS: Record<string, string> = Object.fromEntries(
  ROLE_OPTIONS.map((role) => [role.value, role.label])
);

/** Only Admin can view the Users administration screen. */
export const USER_MANAGEMENT_VIEW_ROLES = ["Admin"];

/** Only Admin has create/edit/deactivate/delete rights over user accounts. */
export const USER_MANAGEMENT_EDIT_ROLES = ["Admin"];
