import axios from "axios";

/** Base URL of StaffingManagementSystem.Api, e.g. https://localhost:7001 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7056";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const token = window.localStorage.getItem("sms_auth_token") ?? window.sessionStorage.getItem("sms_auth_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
