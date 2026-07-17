/**
 * Candidate master service.
 * Calls StaffingManagementSystem.Api -> CandidatesController -> ICandidateService.
 */
import { AxiosError } from "axios";
import { apiClient } from "@/services/apiClient";
import type { ApiResponse } from "@/services/authService";

export interface CandidateListItem {
  id: string;
  fullName: string;
  email: string;
  phone?: string | null;
  currentLocation?: string | null;
  status: string;
  totalExperienceYears: number;
  currentCompany?: string | null;
  skills: string[];
  ownerRecruiterId: string;
  ownerRecruiterName?: string | null;
  createdAtUtc: string;
}

export interface CandidateSkill {
  id: string;
  skillName: string;
  proficiency?: string | null;
  yearsOfExperience?: number | null;
}

export interface CandidateExperience {
  id: string;
  companyName: string;
  jobTitle: string;
  employmentType?: string | null;
  startDate: string;
  endDate?: string | null;
  isCurrent: boolean;
  location?: string | null;
  description?: string | null;
}

export interface CandidateEducation {
  id: string;
  degree: string;
  institution: string;
  fieldOfStudy?: string | null;
  startYear?: number | null;
  endYear?: number | null;
  isExpected: boolean;
  grade?: string | null;
}

export interface CandidateProject {
  id: string;
  projectName: string;
  role?: string | null;
  durationText?: string | null;
  technologiesUsed?: string | null;
  description?: string | null;
}

export interface CandidateNote {
  id: string;
  note: string;
  createdByName?: string | null;
  createdAtUtc: string;
}

export interface CandidateAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedByName?: string | null;
  uploadedAtUtc: string;
}

export interface CandidateDetail {
  id: string;
  fullName: string;
  email: string;
  phone?: string | null;
  address?: string | null;
  currentLocation?: string | null;
  dateOfBirth?: string | null;
  gender?: string | null;
  status: string;
  source?: string | null;
  ownerRecruiterId: string;
  ownerRecruiterName?: string | null;
  totalExperienceYears: number;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  skills: CandidateSkill[];
  experience: CandidateExperience[];
  education: CandidateEducation[];
  projects: CandidateProject[];
  notes: CandidateNote[];
}

export interface SkillInput {
  skillName: string;
  proficiency?: string;
  yearsOfExperience?: number;
}

export interface ExperienceInput {
  companyName: string;
  jobTitle: string;
  employmentType?: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  location?: string;
  description?: string;
}

export interface EducationInput {
  degree: string;
  institution: string;
  fieldOfStudy?: string;
  startYear?: number;
  endYear?: number;
  isExpected: boolean;
  grade?: string;
}

export interface ProjectInput {
  projectName: string;
  role?: string;
  durationText?: string;
  technologiesUsed?: string;
  description?: string;
}

export interface SaveCandidateRequest {
  fullName: string;
  email: string;
  phone?: string;
  address?: string;
  currentLocation?: string;
  dateOfBirth?: string;
  gender?: string;
  status: string;
  source?: string;
  ownerRecruiterId?: string;
  skills: SkillInput[];
  experience: ExperienceInput[];
  education: EducationInput[];
  projects: ProjectInput[];
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

async function getAll(): Promise<ApiResponse<CandidateListItem[]>> {
  try {
    const response = await apiClient.get<ApiResponse<CandidateListItem[]>>("/api/candidates");
    return response.data;
  } catch (error) {
    return toFailure<CandidateListItem[]>(error);
  }
}

async function getById(id: string): Promise<ApiResponse<CandidateDetail>> {
  try {
    const response = await apiClient.get<ApiResponse<CandidateDetail>>(`/api/candidates/${id}`);
    return response.data;
  } catch (error) {
    return toFailure<CandidateDetail>(error);
  }
}

async function create(request: SaveCandidateRequest): Promise<ApiResponse<CandidateDetail>> {
  try {
    const response = await apiClient.post<ApiResponse<CandidateDetail>>("/api/candidates", request);
    return response.data;
  } catch (error) {
    return toFailure<CandidateDetail>(error);
  }
}

async function update(id: string, request: SaveCandidateRequest): Promise<ApiResponse<CandidateDetail>> {
  try {
    const response = await apiClient.put<ApiResponse<CandidateDetail>>(`/api/candidates/${id}`, request);
    return response.data;
  } catch (error) {
    return toFailure<CandidateDetail>(error);
  }
}

async function remove(id: string): Promise<ApiResponse<null>> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(`/api/candidates/${id}`);
    return response.data;
  } catch (error) {
    return toFailure<null>(error);
  }
}

async function addNote(id: string, note: string): Promise<ApiResponse<CandidateNote>> {
  try {
    const response = await apiClient.post<ApiResponse<CandidateNote>>(`/api/candidates/${id}/notes`, { note });
    return response.data;
  } catch (error) {
    return toFailure<CandidateNote>(error);
  }
}

async function getAttachments(candidateId: string): Promise<ApiResponse<CandidateAttachment[]>> {
  try {
    const response = await apiClient.get<ApiResponse<CandidateAttachment[]>>(`/api/candidates/${candidateId}/attachments`);
    return response.data;
  } catch (error) {
    return toFailure<CandidateAttachment[]>(error);
  }
}

async function uploadAttachment(candidateId: string, file: File): Promise<ApiResponse<CandidateAttachment>> {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await apiClient.post<ApiResponse<CandidateAttachment>>(
      `/api/candidates/${candidateId}/attachments`,
      formData
    );
    return response.data;
  } catch (error) {
    return toFailure<CandidateAttachment>(error);
  }
}

async function removeAttachment(candidateId: string, attachmentId: string): Promise<ApiResponse<null>> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(`/api/candidates/${candidateId}/attachments/${attachmentId}`);
    return response.data;
  } catch (error) {
    return toFailure<null>(error);
  }
}

/** Downloads an attachment and triggers a browser save-as, using the server-provided file name. */
async function downloadAttachment(candidateId: string, attachmentId: string, fallbackFileName: string): Promise<void> {
  const response = await apiClient.get(`/api/candidates/${candidateId}/attachments/${attachmentId}/download`, {
    responseType: "blob",
  });

  const disposition = (response.headers as Record<string, string>)["content-disposition"];
  const match = disposition ? /filename="?([^";]+)"?/i.exec(disposition) : null;
  const fileName = match?.[1] ?? fallbackFileName;

  const blobUrl = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = blobUrl;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(blobUrl);
}

export const candidatesService = {
  getAll,
  getById,
  create,
  update,
  remove,
  addNote,
  getAttachments,
  uploadAttachment,
  removeAttachment,
  downloadAttachment,
};
