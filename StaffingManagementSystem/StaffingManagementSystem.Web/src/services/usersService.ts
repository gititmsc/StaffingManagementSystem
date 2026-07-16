/**
 * User & role administration service.
 * Calls StaffingManagementSystem.Api -> UsersController -> IUserManagementService.
 */
import { AxiosError } from "axios";
import { apiClient } from "@/services/apiClient";
import type { ApiResponse } from "@/services/authService";

export interface ManagedUser {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  department?: string | null;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
  lastLoginAtUtc?: string | null;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  department?: string;
  role: string;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  department?: string;
  role: string;
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

async function getAll(): Promise<ApiResponse<ManagedUser[]>> {
  try {
    const response = await apiClient.get<ApiResponse<ManagedUser[]>>("/api/users");
    return response.data;
  } catch (error) {
    return toFailure<ManagedUser[]>(error);
  }
}

async function create(request: CreateUserRequest): Promise<ApiResponse<ManagedUser>> {
  try {
    const response = await apiClient.post<ApiResponse<ManagedUser>>("/api/users", request);
    return response.data;
  } catch (error) {
    return toFailure<ManagedUser>(error);
  }
}

async function update(id: string, request: UpdateUserRequest): Promise<ApiResponse<ManagedUser>> {
  try {
    const response = await apiClient.put<ApiResponse<ManagedUser>>(`/api/users/${id}`, request);
    return response.data;
  } catch (error) {
    return toFailure<ManagedUser>(error);
  }
}

async function setStatus(id: string, isActive: boolean): Promise<ApiResponse<null>> {
  try {
    const response = await apiClient.patch<ApiResponse<null>>(`/api/users/${id}/status`, { isActive });
    return response.data;
  } catch (error) {
    return toFailure<null>(error);
  }
}

async function remove(id: string): Promise<ApiResponse<null>> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(`/api/users/${id}`);
    return response.data;
  } catch (error) {
    return toFailure<null>(error);
  }
}

export const usersService = {
  getAll,
  create,
  update,
  setStatus,
  remove,
};
