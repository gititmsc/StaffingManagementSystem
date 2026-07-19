/**
 * Advanced candidate search and Phase 1 standard reports (skill-wise, experience-wise,
 * company-wise). Calls StaffingManagementSystem.Api -> ReportsController -> ICandidateSearchService.
 */
import { AxiosError } from "axios";
import { apiClient } from "@/services/apiClient";
import type { ApiResponse } from "@/services/authService";
import type { CandidateListItem } from "@/services/candidatesService";

export interface CandidateSearchParams {
  skills?: string[];
  skillMatchMode?: "AND" | "OR";
  skillProficiency?: string;
  minYearsInSkill?: number;
  minExperience?: number;
  maxExperience?: number;
  company?: string;
  designation?: string;
  location?: string;
  status?: string;
  sortBy?: "Experience" | "Name" | "Created";
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CandidateSearchResult {
  items: CandidateListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface NameCount {
  name: string;
  count: number;
}

export interface CandidateReportSummary {
  skillCounts: NameCount[];
  experienceBandCounts: NameCount[];
  companyCounts: NameCount[];
}

function toFailure<T>(error: unknown): ApiResponse<T> {
  const axiosError = error as AxiosError<ApiResponse<T>>;
  const apiMessage = axiosError.response?.data?.message;

  return {
    success: false,
    message: apiMessage ?? "Unable to reach the server. Please try again.",
    errors: axiosError.response?.data?.errors,
  };
}

function buildSearchParams(params: CandidateSearchParams): URLSearchParams {
  const query = new URLSearchParams();

  (params.skills ?? []).forEach((skill) => {
    if (skill.trim()) query.append("skills", skill.trim());
  });

  if (params.skillMatchMode) query.set("skillMatchMode", params.skillMatchMode);
  if (params.skillProficiency) query.set("skillProficiency", params.skillProficiency);
  if (params.minYearsInSkill != null) query.set("minYearsInSkill", String(params.minYearsInSkill));
  if (params.minExperience != null) query.set("minExperience", String(params.minExperience));
  if (params.maxExperience != null) query.set("maxExperience", String(params.maxExperience));
  if (params.company) query.set("company", params.company);
  if (params.designation) query.set("designation", params.designation);
  if (params.location) query.set("location", params.location);
  if (params.status) query.set("status", params.status);
  if (params.sortBy) query.set("sortBy", params.sortBy);
  if (params.sortDescending != null) query.set("sortDescending", String(params.sortDescending));
  if (params.page != null) query.set("page", String(params.page));
  if (params.pageSize != null) query.set("pageSize", String(params.pageSize));

  return query;
}

async function search(params: CandidateSearchParams): Promise<ApiResponse<CandidateSearchResult>> {
  try {
    const response = await apiClient.get<ApiResponse<CandidateSearchResult>>("/api/reports/search", {
      params: buildSearchParams(params),
    });
    return response.data;
  } catch (error) {
    return toFailure<CandidateSearchResult>(error);
  }
}

async function getSummary(): Promise<ApiResponse<CandidateReportSummary>> {
  try {
    const response = await apiClient.get<ApiResponse<CandidateReportSummary>>("/api/reports/summary");
    return response.data;
  } catch (error) {
    return toFailure<CandidateReportSummary>(error);
  }
}

/** Downloads the CSV export for the given filters and triggers a browser save-as. */
async function exportCsv(params: CandidateSearchParams): Promise<void> {
  const response = await apiClient.get("/api/reports/search/export", {
    params: buildSearchParams(params),
    responseType: "blob",
  });

  const disposition = (response.headers as Record<string, string>)["content-disposition"];
  const match = disposition ? /filename="?([^";]+)"?/i.exec(disposition) : null;
  const fileName = match?.[1] ?? "candidates-export.csv";

  const blobUrl = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = blobUrl;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(blobUrl);
}

/** Downloads the PDF export for the given filters and triggers a browser save-as. */
async function exportPdf(params: CandidateSearchParams, reportTitle?: string): Promise<void> {
  const query = buildSearchParams(params);
  if (reportTitle) query.set("reportTitle", reportTitle);

  const response = await apiClient.get("/api/reports/search/export/pdf", {
    params: query,
    responseType: "blob",
  });

  const disposition = (response.headers as Record<string, string>)["content-disposition"];
  const match = disposition ? /filename="?([^";]+)"?/i.exec(disposition) : null;
  const fileName = match?.[1] ?? "candidates-report.pdf";

  const blobUrl = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = blobUrl;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(blobUrl);
}

export const reportsService = {
  search,
  getSummary,
  exportCsv,
  exportPdf,
};
