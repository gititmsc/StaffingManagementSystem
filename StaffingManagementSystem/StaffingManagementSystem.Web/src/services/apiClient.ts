import axios, { type InternalAxiosRequestConfig } from "axios";

/** Base URL of StaffingManagementSystem.Api, e.g. https://localhost:7001 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7056";

export const TOKEN_KEY = "sms_auth_token";
export const REFRESH_TOKEN_KEY = "sms_auth_refresh_token";
export const EXPIRES_AT_KEY = "sms_auth_expires_at";
export const USER_KEY = "sms_auth_user";

/** Silently refresh this long before the access token's actual expiry. */
const REFRESH_MARGIN_MS = 2 * 60 * 1000;

/** These must never trigger the silent-refresh-and-retry flow below. */
const AUTH_ENDPOINTS = ["/api/auth/login", "/api/auth/refresh", "/api/auth/logout"];

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retried?: boolean;
}

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

function getActiveStorage(): Storage | null {
  if (window.localStorage.getItem(TOKEN_KEY)) return window.localStorage;
  if (window.sessionStorage.getItem(TOKEN_KEY)) return window.sessionStorage;
  return null;
}

function clearStoredSession(): void {
  for (const storage of [window.localStorage, window.sessionStorage]) {
    storage.removeItem(TOKEN_KEY);
    storage.removeItem(REFRESH_TOKEN_KEY);
    storage.removeItem(EXPIRES_AT_KEY);
    storage.removeItem(USER_KEY);
  }
}

function isAuthEndpoint(url?: string): boolean {
  return !!url && AUTH_ENDPOINTS.some((path) => url.includes(path));
}

apiClient.interceptors.request.use((config) => {
  const token = window.localStorage.getItem(TOKEN_KEY) ?? window.sessionStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// --- Silent session auto-extend -------------------------------------------
//
// Two triggers share the same in-flight refresh promise so a token is never refreshed twice at
// once (which would race against the backend's refresh-token rotation and could otherwise log
// an active user out):
//   1. Reactive: a request comes back 401 (the access token already expired).
//   2. Proactive: a timer fires ~2 minutes before the token's known expiry, so an active user's
//      session is silently extended before it ever actually expires.

let refreshPromise: Promise<string | null> | null = null;
let proactiveTimer: number | null = null;

async function performRefresh(): Promise<string | null> {
  const storage = getActiveStorage();
  const refreshToken = storage?.getItem(REFRESH_TOKEN_KEY);
  if (!storage || !refreshToken) {
    return null;
  }

  try {
    // Plain axios, not `apiClient` — this call must not go through the interceptors below, or
    // a failed refresh would recurse into itself.
    const response = await axios.post(`${API_BASE_URL}/api/auth/refresh`, { refreshToken });
    const body = response.data;

    if (!body?.success || !body?.data) {
      return null;
    }

    storage.setItem(TOKEN_KEY, body.data.token);
    storage.setItem(REFRESH_TOKEN_KEY, body.data.refreshToken);
    storage.setItem(EXPIRES_AT_KEY, body.data.expiresAtUtc);

    return body.data.token as string;
  } catch {
    return null;
  }
}

function refreshAccessToken(): Promise<string | null> {
  refreshPromise ??= performRefresh().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

function clearProactiveTimer(): void {
  if (proactiveTimer !== null) {
    window.clearTimeout(proactiveTimer);
    proactiveTimer = null;
  }
}

function scheduleProactiveRefresh(): void {
  clearProactiveTimer();

  const storage = getActiveStorage();
  const expiresAtRaw = storage?.getItem(EXPIRES_AT_KEY);
  if (!storage || !expiresAtRaw) {
    return;
  }

  const expiresAtMs = new Date(expiresAtRaw).getTime();
  if (Number.isNaN(expiresAtMs)) {
    return;
  }

  const delay = Math.max(expiresAtMs - Date.now() - REFRESH_MARGIN_MS, 5000);

  proactiveTimer = window.setTimeout(() => {
    void refreshAccessToken().then((newToken) => {
      if (newToken) {
        scheduleProactiveRefresh();
      }
      // A failed refresh isn't retried here — the next authenticated request will hit the
      // reactive 401 path below and, if that also fails, the user is logged out there.
    });
  }, delay);
}

/** Re-arms the proactive refresh timer. Call after login and after every successful token refresh. */
export function restartTokenLifecycle(): void {
  scheduleProactiveRefresh();
}

/** Cancels the proactive refresh timer. Call on logout. */
export function stopTokenLifecycle(): void {
  clearProactiveTimer();
}

// Arm on module load too, so reloading the page with an already-valid session keeps extending it.
scheduleProactiveRefresh();

/**
 * On a 401, try one silent refresh-and-retry before giving up. If the refresh token is also
 * gone/expired, clear the stale session and send the user back to login instead of leaving the
 * app silently failing every subsequent request.
 */
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error.response?.status;
    const originalRequest = error.config as RetryableRequestConfig | undefined;
    const onLoginPage = window.location.pathname.startsWith("/login");

    if (
      status === 401 &&
      originalRequest &&
      !isAuthEndpoint(originalRequest.url) &&
      !originalRequest._retried &&
      !onLoginPage
    ) {
      originalRequest._retried = true;

      const newToken = await refreshAccessToken();

      if (newToken) {
        originalRequest.headers = originalRequest.headers ?? ({} as InternalAxiosRequestConfig["headers"]);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      }
    }

    if (status === 401 && !onLoginPage) {
      clearStoredSession();
      stopTokenLifecycle();
      window.location.assign("/login");
    }

    return Promise.reject(error);
  }
);
