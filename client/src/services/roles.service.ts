import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type { RoleDto, CreateRoleRequestDto, UpdateRoleRequestDto } from '../types/api';

export const rolesService = {
  async getAll(): Promise<RoleDto[]> {
    const response = await fetch(API_ENDPOINTS.roles.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<RoleDto> {
    const response = await fetch(API_ENDPOINTS.roles.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateRoleRequestDto): Promise<RoleDto> {
    const response = await fetch(API_ENDPOINTS.roles.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateRoleRequestDto): Promise<RoleDto> {
    const response = await fetch(API_ENDPOINTS.roles.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.roles.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },
};
