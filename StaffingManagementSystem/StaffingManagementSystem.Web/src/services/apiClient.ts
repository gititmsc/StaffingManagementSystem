import axios from "axios";

/** Base URL of StaffingManagementSystem.Api, e.g. https://localhost:7001 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7056";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

apiClient.interceptors.request.use((config) => {
  const token = window.localStorage.getItem("sms_auth_token") ?? window.sessionStorage.getItem("sms_auth_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * A 401 means the access token is missing/expired — clear the stale session and send the
 * user back to login instead of leaving the app silently failing every subsequent request.
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !window.location.pathname.startsWith("/login")) {
      window.localStorage.removeItem("sms_auth_token");
      window.localStorage.removeItem("sms_auth_user");
      window.sessionStorage.removeItem("sms_auth_token");
      window.sessionStorage.removeItem("sms_auth_user");
      window.location.assign("/login");
    }
    return Promise.reject(error);
  }
);
