/**
 * Role-aware Dashboard summary. Calls StaffingManagementSystem.Api -> DashboardController -> IDashboardService.
 * Only the fields relevant to the signed-in user's role come back populated — the rest are
 * undefined, so callers should check for presence before rendering a widget.
 */
import { AxiosError } from "axios";
import { apiClient } from "@/services/apiClient";
import type { ApiResponse } from "@/services/authService";

export interface NameCount {
  name: string;
  count: number;
}

export interface DashboardCandidate {
  id: string;
  fullName: string;
  status: string;
  eventAtUtc: string;
  ownerRecruiterName?: string | null;
}

export interface DashboardSummary {
  // Admin
  statusCounts?: NameCount[];
  pendingApprovalsCount?: number;
  recentlyApproved?: DashboardCandidate[];
  recentlyRejected?: DashboardCandidate[];
  recruiterWorkload?: NameCount[];
  topSkills?: NameCount[];
  recentRegistrations?: DashboardCandidate[];

  // Recruiter
  myCandidatesCount?: number;
  myCandidatesByStatus?: NameCount[];
  myInProcessCount?: number;
  recentlyAddedSystemWide?: DashboardCandidate[];

  // Viewer (also uses statusCounts above)
  totalVisibleCandidates?: number;
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

async function getSummary(): Promise<ApiResponse<DashboardSummary>> {
  try {
    const response = await apiClient.get<ApiResponse<DashboardSummary>>("/api/dashboard/summary");
    return response.data;
  } catch (error) {
    return toFailure<DashboardSummary>(error);
  }
}

export const dashboardService = { getSummary };
