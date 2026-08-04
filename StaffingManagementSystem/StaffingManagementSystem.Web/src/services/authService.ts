/**
 * Authentication service for the Staffing Management System login, forgot-password and
 * reset-password flows. Calls StaffingManagementSystem.Api -> AuthController -> IAuthService.
 */
import { AxiosError } from "axios";
import {
  apiClient,
  EXPIRES_AT_KEY,
  REFRESH_TOKEN_KEY,
  restartTokenLifecycle,
  stopTokenLifecycle,
  TOKEN_KEY,
  USER_KEY,
} from "@/services/apiClient";

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

export interface AuthResult {
  token: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: AuthUser;
}

/** Standard API envelope returned by every endpoint. */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

interface LoginResponseData {
  token: string;
  expiresAtUtc: string;
  refreshToken: string;
  user: {
    id: string;
    fullName: string;
    email: string;
    role: string;
  };
}

/** Attempts to sign the user in via the Staffing Management System API. */
async function login(request: LoginRequest): Promise<ApiResponse<AuthResult>> {
  try {
    const response = await apiClient.post<ApiResponse<LoginResponseData>>("/api/auth/login", {
      email: request.email,
      password: request.password,
      rememberMe: request.rememberMe,
    });

    if (!response.data.success || !response.data.data) {
      return {
        success: false,
        message: response.data.message || "Invalid email or password.",
        errors: response.data.errors,
      };
    }

    return {
      success: true,
      message: response.data.message,
      data: {
        token: response.data.data.token,
        refreshToken: response.data.data.refreshToken,
        expiresAtUtc: response.data.data.expiresAtUtc,
        user: response.data.data.user,
      },
    };
  } catch (error) {
    const axiosError = error as AxiosError<ApiResponse<LoginResponseData>>;
    const apiMessage = axiosError.response?.data?.message;

    return {
      success: false,
      message: apiMessage ?? "Unable to reach the server. Please try again.",
      errors: axiosError.response?.data?.errors,
    };
  }
}

/** Extracts a consistent {success, message, errors} shape from any AuthController response. */
async function callAuthEndpoint(path: string, payload: unknown): Promise<ApiResponse<null>> {
  try {
    const response = await apiClient.post<ApiResponse<unknown>>(path, payload);

    return {
      success: response.data.success,
      message: response.data.message,
      errors: response.data.errors,
    };
  } catch (error) {
    const axiosError = error as AxiosError<ApiResponse<unknown>>;
    const apiMessage = axiosError.response?.data?.message;

    return {
      success: false,
      message: apiMessage ?? "Unable to reach the server. Please try again.",
      errors: axiosError.response?.data?.errors,
    };
  }
}

/**
 * Requests a password reset email for the given address. The API always returns a generic
 * success message regardless of whether the address matches an account, by design.
 */
function forgotPassword(email: string): Promise<ApiResponse<null>> {
  return callAuthEndpoint("/api/auth/forgot-password", { email });
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

/** Redeems a password reset token (from the emailed link) and sets a new password. */
function resetPassword(request: ResetPasswordRequest): Promise<ApiResponse<null>> {
  return callAuthEndpoint("/api/auth/reset-password", request);
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

/** Changes the signed-in user's own password after verifying their current password. */
function changePassword(request: ChangePasswordRequest): Promise<ApiResponse<null>> {
  return callAuthEndpoint("/api/auth/change-password", request);
}

/**
 * Persists a login result and arms the silent-refresh timer so the session stays alive without
 * the user having to sign in again while they're active.
 */
function persistSession(result: AuthResult, rememberMe: boolean): void {
  const storage = rememberMe ? window.localStorage : window.sessionStorage;
  storage.setItem(TOKEN_KEY, result.token);
  storage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
  storage.setItem(EXPIRES_AT_KEY, result.expiresAtUtc);
  storage.setItem(USER_KEY, JSON.stringify(result.user));
  restartTokenLifecycle();
}

function getToken(): string | null {
  return window.localStorage.getItem(TOKEN_KEY) ?? window.sessionStorage.getItem(TOKEN_KEY);
}

function getStoredUser(): AuthUser | null {
  const raw = window.localStorage.getItem(USER_KEY) ?? window.sessionStorage.getItem(USER_KEY);
  return raw ? (JSON.parse(raw) as AuthUser) : null;
}

/**
 * Signs the user out: best-effort revokes the refresh token server-side (so it can't silently
 * mint new access tokens later), stops the auto-extend timer, then clears the local session
 * regardless of whether the revoke call succeeded.
 */
async function logout(): Promise<void> {
  const refreshToken =
    window.localStorage.getItem(REFRESH_TOKEN_KEY) ?? window.sessionStorage.getItem(REFRESH_TOKEN_KEY);

  stopTokenLifecycle();

  if (refreshToken) {
    try {
      await apiClient.post("/api/auth/logout", { refreshToken });
    } catch {
      // Offline or the server is down — still clear the local session below either way.
    }
  }

  for (const storage of [window.localStorage, window.sessionStorage]) {
    storage.removeItem(TOKEN_KEY);
    storage.removeItem(REFRESH_TOKEN_KEY);
    storage.removeItem(EXPIRES_AT_KEY);
    storage.removeItem(USER_KEY);
  }
}

export const authService = {
  login,
  logout,
  getToken,
  getStoredUser,
  persistSession,
  forgotPassword,
  resetPassword,
  changePassword,
};
