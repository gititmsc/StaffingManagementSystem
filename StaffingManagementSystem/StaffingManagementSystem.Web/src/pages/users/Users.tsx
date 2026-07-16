import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useAuth } from "@/context/AuthContext";
import { Modal } from "@/components/ui/Modal";
import { ROLE_LABELS, ROLE_OPTIONS, USER_MANAGEMENT_EDIT_ROLES } from "@/constants/roles";
import { usersService, type ManagedUser } from "@/services/usersService";
import "./Users.css";

interface UserFormValues {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  department: string;
  role: string;
}

const EMPTY_FORM: UserFormValues = {
  firstName: "",
  lastName: "",
  email: "",
  phoneNumber: "",
  department: "",
  role: ROLE_OPTIONS[0].value,
};

function formatDate(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleString();
}

export default function Users() {
  const { user: currentUser } = useAuth();
  const canEdit = !!currentUser && USER_MANAGEMENT_EDIT_ROLES.includes(currentUser.role);

  const [users, setUsers] = useState<ManagedUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");

  const [modalMode, setModalMode] = useState<"create" | "edit" | null>(null);
  const [editingUser, setEditingUser] = useState<ManagedUser | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  const [pendingDelete, setPendingDelete] = useState<ManagedUser | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [pageMessage, setPageMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<UserFormValues>({ mode: "onBlur", defaultValues: EMPTY_FORM });

  const loadUsers = async () => {
    setIsLoading(true);
    setLoadError(null);

    const response = await usersService.getAll();

    if (!response.success || !response.data) {
      setLoadError(response.message || "Unable to load users.");
      setIsLoading(false);
      return;
    }

    setUsers(response.data);
    setIsLoading(false);
  };

  useEffect(() => {
    loadUsers();
  }, []);

  useEffect(() => {
    if (!pageMessage) return;
    const timer = window.setTimeout(() => setPageMessage(null), 4000);
    return () => window.clearTimeout(timer);
  }, [pageMessage]);

  const filteredUsers = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return users;

    return users.filter(
      (u) =>
        u.fullName.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term) ||
        (u.department ?? "").toLowerCase().includes(term)
    );
  }, [users, searchTerm]);

  const openCreateModal = () => {
    setEditingUser(null);
    setFormError(null);
    reset(EMPTY_FORM);
    setModalMode("create");
  };

  const openEditModal = (target: ManagedUser) => {
    setEditingUser(target);
    setFormError(null);
    reset({
      firstName: target.firstName,
      lastName: target.lastName,
      email: target.email,
      phoneNumber: target.phoneNumber ?? "",
      department: target.department ?? "",
      role: target.role,
    });
    setModalMode("edit");
  };

  const closeModal = () => {
    setModalMode(null);
    setEditingUser(null);
    setFormError(null);
  };

  const onSubmit = async (values: UserFormValues) => {
    setFormError(null);

    const payload = {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      phoneNumber: values.phoneNumber.trim() || undefined,
      department: values.department.trim() || undefined,
      role: values.role,
    };

    const response =
      modalMode === "create"
        ? await usersService.create({ ...payload, email: values.email.trim() })
        : await usersService.update(editingUser!.id, payload);

    if (!response.success) {
      setFormError(response.message || "Unable to save this user. Please try again.");
      return;
    }

    setPageMessage(response.message || (modalMode === "create" ? "User created." : "User updated."));
    closeModal();
    await loadUsers();
  };

  const toggleStatus = async (target: ManagedUser) => {
    setActionError(null);
    const response = await usersService.setStatus(target.id, !target.isActive);

    if (!response.success) {
      setActionError(response.message || "Unable to update this user's status.");
      return;
    }

    setPageMessage(response.message || "User status updated.");
    await loadUsers();
  };

  const confirmDelete = async () => {
    if (!pendingDelete) return;
    setActionError(null);

    const response = await usersService.remove(pendingDelete.id);

    if (!response.success) {
      setActionError(response.message || "Unable to delete this user.");
      setPendingDelete(null);
      return;
    }

    setPageMessage(response.message || "User deleted.");
    setPendingDelete(null);
    await loadUsers();
  };

  return (
    <div className="container py-4">
      <div className="users-header">
        <div>
          <h1 className="h4 mb-1" style={{ color: "var(--itm-primary)" }}>
            User & Role Management
          </h1>
          <p className="text-muted mb-0">Manage who can access the Staffing Management System and what they can do.</p>
        </div>
        {canEdit && (
          <button type="button" className="users-add-btn" onClick={openCreateModal}>
            <i className="bi bi-plus-lg" aria-hidden="true" />
            Add User
          </button>
        )}
      </div>

      {pageMessage && (
        <div className="users-alert users-alert--success" role="status">
          <i className="bi bi-check-circle-fill" aria-hidden="true" />
          <span>{pageMessage}</span>
        </div>
      )}

      {actionError && (
        <div className="users-alert users-alert--error" role="alert">
          <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          <span>{actionError}</span>
        </div>
      )}

      <div className="users-toolbar">
        <div className="users-search">
          <i className="bi bi-search" aria-hidden="true" />
          <input
            type="text"
            placeholder="Search by name, email or department..."
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            aria-label="Search users"
          />
        </div>
      </div>

      <div className="users-table-card">
        {isLoading ? (
          <div className="users-empty">
            <span
              className="login-spinner"
              style={{ borderTopColor: "var(--itm-primary)", borderColor: "rgba(22,58,99,0.2)" }}
            />
            <span>Loading users...</span>
          </div>
        ) : loadError ? (
          <div className="users-empty users-empty--error">
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
            <span>{loadError}</span>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="users-empty">
            <i className="bi bi-people" aria-hidden="true" />
            <span>No users found.</span>
          </div>
        ) : (
          <table className="table users-table align-middle mb-0">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Department</th>
                <th>Role</th>
                <th>Status</th>
                <th>Last Login</th>
                {canEdit && <th className="text-end">Actions</th>}
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map((row) => {
                const isSelf = row.id === currentUser?.id;
                return (
                  <tr key={row.id}>
                    <td>{row.fullName}</td>
                    <td>{row.email}</td>
                    <td>{row.phoneNumber || "—"}</td>
                    <td>{row.department || "—"}</td>
                    <td>{ROLE_LABELS[row.role] ?? row.role}</td>
                    <td>
                      <span className={`users-badge ${row.isActive ? "users-badge--active" : "users-badge--inactive"}`}>
                        {row.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td>{formatDate(row.lastLoginAtUtc)}</td>
                    {canEdit && (
                      <td>
                        <div className="users-row-actions">
                          <button
                            type="button"
                            className="users-icon-btn"
                            onClick={() => openEditModal(row)}
                            aria-label={`Edit ${row.fullName}`}
                            title="Edit"
                          >
                            <i className="bi bi-pencil-fill" aria-hidden="true" />
                          </button>
                          <button
                            type="button"
                            className="users-icon-btn"
                            onClick={() => toggleStatus(row)}
                            disabled={isSelf && row.isActive}
                            aria-label={row.isActive ? `Deactivate ${row.fullName}` : `Activate ${row.fullName}`}
                            title={
                              isSelf && row.isActive
                                ? "You cannot deactivate your own account"
                                : row.isActive
                                  ? "Deactivate"
                                  : "Activate"
                            }
                          >
                            <i className={`bi ${row.isActive ? "bi-slash-circle" : "bi-check-circle"}`} aria-hidden="true" />
                          </button>
                          <button
                            type="button"
                            className="users-icon-btn users-icon-btn--danger"
                            onClick={() => setPendingDelete(row)}
                            disabled={isSelf}
                            aria-label={`Delete ${row.fullName}`}
                            title={isSelf ? "You cannot delete your own account" : "Delete"}
                          >
                            <i className="bi bi-trash-fill" aria-hidden="true" />
                          </button>
                        </div>
                      </td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {modalMode && (
        <Modal
          title={modalMode === "create" ? "Add User" : "Edit User"}
          onClose={closeModal}
          footer={
            <>
              <button type="button" className="users-btn users-btn--ghost" onClick={closeModal}>
                Cancel
              </button>
              <button
                type="submit"
                form="user-form"
                className="users-btn users-btn--primary"
                disabled={isSubmitting}
              >
                {isSubmitting && <span className="login-spinner" aria-hidden="true" />}
                {modalMode === "create" ? "Create User" : "Save Changes"}
              </button>
            </>
          }
        >
          {formError && (
            <div className="users-alert users-alert--error" role="alert">
              <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
              <span>{formError}</span>
            </div>
          )}

          {modalMode === "create" && (
            <p className="users-form-hint">The new user will receive an email with a link to set their own password.</p>
          )}

          <form onSubmit={handleSubmit(onSubmit)} noValidate id="user-form">
            <div className="row g-3">
              <div className="col-12 col-sm-6">
                <label className="users-form-label" htmlFor="firstName">
                  First Name
                </label>
                <input
                  id="firstName"
                  className={`form-control ${errors.firstName ? "is-invalid" : ""}`}
                  {...register("firstName", { required: "First name is required." })}
                />
                {errors.firstName && <div className="invalid-feedback">{errors.firstName.message}</div>}
              </div>

              <div className="col-12 col-sm-6">
                <label className="users-form-label" htmlFor="lastName">
                  Last Name
                </label>
                <input
                  id="lastName"
                  className={`form-control ${errors.lastName ? "is-invalid" : ""}`}
                  {...register("lastName", { required: "Last name is required." })}
                />
                {errors.lastName && <div className="invalid-feedback">{errors.lastName.message}</div>}
              </div>

              <div className="col-12">
                <label className="users-form-label" htmlFor="email">
                  Email Address
                </label>
                <input
                  id="email"
                  type="email"
                  disabled={modalMode === "edit"}
                  className={`form-control ${errors.email ? "is-invalid" : ""}`}
                  {...register("email", {
                    required: "Email address is required.",
                    pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address." },
                  })}
                />
                {errors.email && <div className="invalid-feedback">{errors.email.message}</div>}
                {modalMode === "edit" && <div className="users-form-note">Email cannot be changed after creation.</div>}
              </div>

              <div className="col-12 col-sm-6">
                <label className="users-form-label" htmlFor="phoneNumber">
                  Phone (optional)
                </label>
                <input id="phoneNumber" className="form-control" {...register("phoneNumber")} />
              </div>

              <div className="col-12 col-sm-6">
                <label className="users-form-label" htmlFor="department">
                  Department (optional)
                </label>
                <input id="department" className="form-control" {...register("department")} />
              </div>

              <div className="col-12">
                <label className="users-form-label" htmlFor="role">
                  Role
                </label>
                <select id="role" className="form-select" {...register("role", { required: true })}>
                  {ROLE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </form>
        </Modal>
      )}

      {pendingDelete && (
        <Modal title="Delete User" onClose={() => setPendingDelete(null)} size="sm">
          <p className="mb-0">
            Are you sure you want to delete <strong>{pendingDelete.fullName}</strong>? They will lose access
            immediately. This can't be undone from this screen.
          </p>
          <div className="users-confirm-actions">
            <button type="button" className="users-btn users-btn--ghost" onClick={() => setPendingDelete(null)}>
              Cancel
            </button>
            <button type="button" className="users-btn users-btn--danger" onClick={confirmDelete}>
              Delete User
            </button>
          </div>
        </Modal>
      )}
    </div>
  );
}
