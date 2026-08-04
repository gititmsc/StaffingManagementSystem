import { useState } from "react";
import { useForm } from "react-hook-form";
import { Modal } from "@/components/ui/Modal";
import { authService } from "@/services/authService";
import "./ChangePasswordModal.css";

interface ChangePasswordFormValues {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

interface ChangePasswordModalProps {
  onClose: () => void;
}

/** Lets a signed-in user change their own password from anywhere in the app. */
export function ChangePasswordModal({ onClose }: ChangePasswordModalProps) {
  const [serverError, setServerError] = useState<string | null>(null);
  const [succeeded, setSucceeded] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordFormValues>({
    mode: "onBlur",
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
  });

  const onSubmit = async (values: ChangePasswordFormValues) => {
    setServerError(null);

    const response = await authService.changePassword({
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
      confirmPassword: values.confirmPassword,
    });

    if (!response.success) {
      setServerError(response.message || "Unable to change your password. Please try again.");
      return;
    }

    setSucceeded(true);
  };

  return (
    <Modal title="Change Password" onClose={onClose} size="sm">
      {succeeded ? (
        <div className="change-password-success">
          <i className="bi bi-check-circle-fill" aria-hidden="true" />
          <p className="mb-0">Your password has been changed.</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          {serverError && (
            <div className="change-password-alert" role="alert">
              <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
              <span>{serverError}</span>
            </div>
          )}

          <div className="change-password-field">
            <label htmlFor="currentPassword">Current Password</label>
            <input
              id="currentPassword"
              type="password"
              autoFocus
              autoComplete="current-password"
              className={`form-control ${errors.currentPassword ? "is-invalid" : ""}`}
              {...register("currentPassword", { required: "Current password is required." })}
            />
            {errors.currentPassword && <div className="invalid-feedback">{errors.currentPassword.message}</div>}
          </div>

          <div className="change-password-field">
            <label htmlFor="newPassword">New Password</label>
            <input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              className={`form-control ${errors.newPassword ? "is-invalid" : ""}`}
              {...register("newPassword", {
                required: "New password is required.",
                minLength: { value: 8, message: "Password must be at least 8 characters." },
              })}
            />
            {errors.newPassword && <div className="invalid-feedback">{errors.newPassword.message}</div>}
          </div>

          <div className="change-password-field">
            <label htmlFor="confirmPassword">Confirm New Password</label>
            <input
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              className={`form-control ${errors.confirmPassword ? "is-invalid" : ""}`}
              {...register("confirmPassword", {
                required: "Please confirm your new password.",
                validate: (value) => value === watch("newPassword") || "Passwords do not match.",
              })}
            />
            {errors.confirmPassword && <div className="invalid-feedback">{errors.confirmPassword.message}</div>}
          </div>

          <div className="change-password-actions">
            <button type="button" className="change-password-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="change-password-submit" disabled={isSubmitting}>
              {isSubmitting ? "Changing..." : "Change Password"}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
