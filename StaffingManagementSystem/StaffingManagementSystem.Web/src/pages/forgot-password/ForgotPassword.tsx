import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { ITMLogo } from "@/components/brand/ITMLogo";
import { authService } from "@/services/authService";
import { LoginPanelPattern } from "@/pages/login/LoginPanelPattern";
import "@/pages/login/Login.css";
import "./ForgotPassword.css";

interface ForgotPasswordFormValues {
  email: string;
}

/**
 * "Forgot password" entry point (linked from the Login page). Always shows the same
 * generic confirmation after submit — the API never reveals whether the email matched
 * an account, so the UI shouldn't either.
 */
export default function ForgotPassword() {
  const [submittedMessage, setSubmittedMessage] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormValues>({
    mode: "onBlur",
    defaultValues: { email: "" },
  });

  const onSubmit = async (values: ForgotPasswordFormValues) => {
    setServerError(null);

    const response = await authService.forgotPassword(values.email);

    if (!response.success) {
      setServerError(response.message || "Unable to send the reset link. Please try again.");
      return;
    }

    setSubmittedMessage(response.message || "If an account exists for that email address, we've sent a password reset link.");
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
          <p className="login-panel__description">
            Forgot your password? No problem — enter your work email and we'll send you a secure link to set a new
            one.
          </p>
        </div>

        <div className="login-panel__footer">
          <div>Version 1.0</div>
          <div>&copy; 2026 ITMusketeers Consultancy Services</div>
        </div>
      </aside>

      <main className="login-content">
        <div className="login-card">
          <ITMLogo height={32} className="mb-1" />

          {submittedMessage ? (
            <div className="fp-success">
              <div className="fp-success__icon">
                <i className="bi bi-envelope-check-fill" aria-hidden="true" />
              </div>
              <h2 className="login-card__heading">Check your email</h2>
              <p className="login-card__subtitle">{submittedMessage}</p>
              <Link to="/login" className="login-submit fp-back-link">
                Back to Sign In
              </Link>
            </div>
          ) : (
            <>
              <h2 className="login-card__heading">Forgot Password?</h2>
              <p className="login-card__subtitle">
                Enter the email address associated with your account and we'll send you a link to reset your
                password.
              </p>

              {serverError && (
                <div className="login-alert" role="alert">
                  <i className="bi bi-exclamation-triangle-fill" aria-hidden="true" />
                  <span>{serverError}</span>
                </div>
              )}

              <form onSubmit={handleSubmit(onSubmit)} noValidate>
                <div className="login-field">
                  <label htmlFor="email">Email Address</label>
                  <div className={`login-input-group ${errors.email ? "is-invalid" : ""}`}>
                    <span className="login-input-group__icon">
                      <i className="bi bi-envelope-fill" aria-hidden="true" />
                    </span>
                    <input
                      id="email"
                      type="email"
                      autoFocus
                      autoComplete="email"
                      placeholder="Enter your email address"
                      aria-label="Email address"
                      aria-invalid={errors.email ? "true" : "false"}
                      aria-describedby={errors.email ? "email-error" : undefined}
                      {...register("email", {
                        required: "Email address is required.",
                        pattern: {
                          value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                          message: "Enter a valid email address.",
                        },
                      })}
                    />
                  </div>
                  {errors.email && (
                    <div className="login-field__error" id="email-error">
                      {errors.email.message}
                    </div>
                  )}
                </div>

                <button type="submit" className="login-submit" disabled={isSubmitting} aria-busy={isSubmitting}>
                  {isSubmitting && <span className="login-spinner" aria-hidden="true" />}
                  {isSubmitting ? "Sending..." : "Send Reset Link"}
                </button>
              </form>

              <div className="fp-footer-row">
                <Link to="/login" className="login-forgot">
                  <i className="bi bi-arrow-left" aria-hidden="true" /> Back to Sign In
                </Link>
              </div>
            </>
          )}
        </div>
      </main>
    </div>
  );
}
