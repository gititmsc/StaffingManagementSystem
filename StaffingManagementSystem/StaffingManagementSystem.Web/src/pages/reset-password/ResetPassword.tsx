import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router-dom";
import { ITMLogo } from "@/components/brand/ITMLogo";
import { authService } from "@/services/authService";
import { LoginPanelPattern } from "@/pages/login/LoginPanelPattern";
import "@/pages/login/Login.css";
import "@/pages/forgot-password/ForgotPassword.css";

interface ResetPasswordFormValues {
  newPassword: string;
  confirmPassword: string;
}

/** Reached from the link emailed by the "forgot password" flow: /reset-password?token=... */
export default function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);
  const [succeeded, setSucceeded] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormValues>({
    mode: "onBlur",
    defaultValues: { newPassword: "", confirmPassword: "" },
  });

  const onSubmit = async (values: ResetPasswordFormValues) => {
    setServerError(null);

    const response = await authService.resetPassword({
      token,
      newPassword: values.newPassword,
      confirmPassword: values.confirmPassword,
    });

    if (!response.success) {
      setServerError(response.message || "Unable to reset your password. Please request a new link.");
      return;
    }

    setSucceeded(true);
  };

  const renderBody = () => {
    if (!token) {
      return (
        <div className="fp-success">
          <div className="fp-success__icon" style={{ background: "rgba(229, 57, 53, 0.12)", color: "var(--itm-secondary)" }}>
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
          </div>
          <h2 className="login-card__heading">Invalid reset link</h2>
          <p className="login-card__subtitle">
            This link is missing its reset token. Please request a new password reset link.
          </p>
          <Link to="/forgot-password" className="login-submit fp-back-link">
            Request New Link
          </Link>
        </div>
      );
    }

    if (succeeded) {
      return (
        <div className="fp-success">
          <div className="fp-success__icon">
            <i className="bi bi-check-circle-fill" aria-hidden="true" />
          </div>
          <h2 className="login-card__heading">Password reset</h2>
          <p className="login-card__subtitle">
            Your password has been changed successfully. You can now sign in with your new password.
          </p>
          <Link to="/login" className="login-submit fp-back-link">
            Back to Sign In
          </Link>
        </div>
      );
    }

    return (
      <>
        <h2 className="login-card__heading">Reset Password</h2>
        <p className="login-card__subtitle">Choose a new password for your account.</p>

        {serverError && (
          <div className="login-alert" role="alert">
            <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
            <span>{serverError}</span>
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="login-field">
            <label htmlFor="newPassword">New Password</label>
            <div className={`login-input-group ${errors.newPassword ? "is-invalid" : ""}`}>
              <span className="login-input-group__icon">
                <i className="bi bi-lock-fill" aria-hidden="true" />
              </span>
              <input
                id="newPassword"
                type={showPassword ? "text" : "password"}
                autoFocus
                autoComplete="new-password"
                placeholder="Enter your new password"
                aria-label="New password"
                aria-invalid={errors.newPassword ? "true" : "false"}
                aria-describedby={errors.newPassword ? "new-password-error" : undefined}
                {...register("newPassword", {
                  required: "New password is required.",
                  minLength: { value: 8, message: "Password must be at least 8 characters." },
                })}
              />
              <button
                type="button"
                className="login-input-group__toggle"
                onClick={() => setShowPassword((prev) => !prev)}
                aria-label={showPassword ? "Hide password" : "Show password"}
                aria-pressed={showPassword}
                tabIndex={0}
              >
                <i className={`bi ${showPassword ? "bi-eye-slash-fill" : "bi-eye-fill"}`} aria-hidden="true" />
              </button>
            </div>
            {errors.newPassword && (
              <div className="login-field__error" id="new-password-error">
                {errors.newPassword.message}
              </div>
            )}
          </div>

          <div className="login-field">
            <label htmlFor="confirmPassword">Confirm New Password</label>
            <div className={`login-input-group ${errors.confirmPassword ? "is-invalid" : ""}`}>
              <span className="login-input-group__icon">
                <i className="bi bi-lock-fill" aria-hidden="true" />
              </span>
              <input
                id="confirmPassword"
                type={showConfirmPassword ? "text" : "password"}
                autoComplete="new-password"
                placeholder="Re-enter your new password"
                aria-label="Confirm new password"
                aria-invalid={errors.confirmPassword ? "true" : "false"}
                aria-describedby={errors.confirmPassword ? "confirm-password-error" : undefined}
                {...register("confirmPassword", {
                  required: "Please confirm your new password.",
                  validate: (value) => value === watch("newPassword") || "Passwords do not match.",
                })}
              />
              <button
                type="button"
                className="login-input-group__toggle"
                onClick={() => setShowConfirmPassword((prev) => !prev)}
                aria-label={showConfirmPassword ? "Hide password" : "Show password"}
                aria-pressed={showConfirmPassword}
                tabIndex={0}
              >
                <i className={`bi ${showConfirmPassword ? "bi-eye-slash-fill" : "bi-eye-fill"}`} aria-hidden="true" />
              </button>
            </div>
            {errors.confirmPassword && (
              <div className="login-field__error" id="confirm-password-error">
                {errors.confirmPassword.message}
              </div>
            )}
          </div>

          <button type="submit" className="login-submit" disabled={isSubmitting} aria-busy={isSubmitting}>
            {isSubmitting && <span className="login-spinner" aria-hidden="true" />}
            {isSubmitting ? "Resetting..." : "Reset Password"}
          </button>
        </form>

        <div className="fp-footer-row">
          <Link to="/login" className="login-forgot">
            <i className="bi bi-arrow-left" aria-hidden="true" /> Back to Sign In
          </Link>
        </div>
      </>
    );
  };

  return (
    <div className="login-page">
      <aside className="login-panel">
        <div className="login-panel__pattern">
          <LoginPanelPattern />
        </div>

        <div className="login-panel__content">
          <ITMLogo height={40} variant="light" />
          <h1 className="login-panel__heading">Staffing Management System</h1>
          <p className="login-panel__subtitle">{"Manage Talent.\nAccelerate Hiring.\nBuild High-Performing Teams."}</p>
          <p className="login-panel__description">Set a new password to get back into your account.</p>
        </div>

        <div className="login-panel__footer">
          <div>Version 1.0</div>
          <div>&copy; 2026 ITMusketeers Consultancy Services</div>
        </div>
      </aside>

      <main className="login-content">
        <div className="login-card">
          <ITMLogo height={32} className="mb-1" />
          {renderBody()}
        </div>
      </main>
    </div>
  );
}
