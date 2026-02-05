import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type {
  UserDto,
  CreateUserRequestDto,
  UpdateUserRequestDto,
  ChangePasswordRequestDto,
} from '../types/api';

export const usersService = {
  async getAll(): Promise<UserDto[]> {
    const response = await fetch(API_ENDPOINTS.users.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<UserDto> {
    const response = await fetch(API_ENDPOINTS.users.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateUserRequestDto): Promise<UserDto> {
    const response = await fetch(API_ENDPOINTS.users.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateUserRequestDto): Promise<UserDto> {
    const response = await fetch(API_ENDPOINTS.users.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.users.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },

  async changePassword(id: number, data: ChangePasswordRequestDto): Promise<void> {
    const response = await fetch(API_ENDPOINTS.users.changePassword(id), {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
  },
};
