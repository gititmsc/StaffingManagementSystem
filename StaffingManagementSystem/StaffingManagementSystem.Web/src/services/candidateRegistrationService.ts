/**
 * Public, no-login candidate self-registration service.
 * Calls StaffingManagementSystem.Api -> CandidateRegistrationController -> ICandidateRegistrationService.
 */
import { AxiosError } from "axios";
import { apiClient } from "@/services/apiClient";
import type { ApiResponse } from "@/services/authService";
import type { EducationInput, ExperienceInput, ProjectInput } from "@/services/candidatesService";

export interface CandidateSelfRegistrationRequest {
  fullName: string;
  email: string;
  phone: string;
  currentLocation?: string;
  linkedInUrl?: string;
  skills: string[];
  experience: ExperienceInput[];
  education: EducationInput[];
  projects: ProjectInput[];
  recaptchaToken: string;
  resume: File;
}

function toFailure(error: unknown): ApiResponse<null> {
  const axiosError = error as AxiosError<ApiResponse<null>>;
  const apiMessage = axiosError.response?.data?.message;

  return {
    success: false,
    message: apiMessage ?? "Unable to reach the server. Please try again.",
    errors: axiosError.response?.data?.errors,
  };
}

/** Appends `value` under `Key` only when it's defined — ASP.NET Core form binding ignores absent keys. */
function appendIfDefined(formData: FormData, key: string, value: string | number | boolean | undefined) {
  if (value === undefined || value === "") return;
  formData.append(key, String(value));
}

async function register(request: CandidateSelfRegistrationRequest): Promise<ApiResponse<null>> {
  try {
    const formData = new FormData();
    formData.append("FullName", request.fullName);
    formData.append("Email", request.email);
    formData.append("Phone", request.phone);
    appendIfDefined(formData, "CurrentLocation", request.currentLocation);
    appendIfDefined(formData, "LinkedInUrl", request.linkedInUrl);
    request.skills.forEach((skill) => formData.append("Skills", skill));

    request.experience.forEach((exp, i) => {
      formData.append(`Experience[${i}].CompanyName`, exp.companyName);
      formData.append(`Experience[${i}].JobTitle`, exp.jobTitle);
      appendIfDefined(formData, `Experience[${i}].EmploymentType`, exp.employmentType);
      formData.append(`Experience[${i}].StartDate`, exp.startDate);
      appendIfDefined(formData, `Experience[${i}].EndDate`, exp.endDate);
      formData.append(`Experience[${i}].IsCurrent`, String(exp.isCurrent));
      appendIfDefined(formData, `Experience[${i}].Location`, exp.location);
      appendIfDefined(formData, `Experience[${i}].Description`, exp.description);
    });

    request.education.forEach((edu, i) => {
      formData.append(`Education[${i}].Degree`, edu.degree);
      formData.append(`Education[${i}].Institution`, edu.institution);
      appendIfDefined(formData, `Education[${i}].FieldOfStudy`, edu.fieldOfStudy);
      appendIfDefined(formData, `Education[${i}].StartYear`, edu.startYear);
      appendIfDefined(formData, `Education[${i}].EndYear`, edu.endYear);
      formData.append(`Education[${i}].IsExpected`, String(edu.isExpected));
      appendIfDefined(formData, `Education[${i}].Grade`, edu.grade);
    });

    request.projects.forEach((proj, i) => {
      formData.append(`Projects[${i}].ProjectName`, proj.projectName);
      appendIfDefined(formData, `Projects[${i}].Role`, proj.role);
      appendIfDefined(formData, `Projects[${i}].DurationText`, proj.durationText);
      appendIfDefined(formData, `Projects[${i}].TechnologiesUsed`, proj.technologiesUsed);
      appendIfDefined(formData, `Projects[${i}].Description`, proj.description);
    });

    formData.append("RecaptchaToken", request.recaptchaToken);
    formData.append("resume", request.resume);

    const response = await apiClient.post<ApiResponse<null>>("/api/candidate-registration", formData);
    return response.data;
  } catch (error) {
    return toFailure(error);
  }
}

export const candidateRegistrationService = { register };
