import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type {
  TagDto,
  CreateTagRequestDto,
  UpdateTagRequestDto,
} from '../types/api';

export const tagsService = {
  async getAll(): Promise<TagDto[]> {
    const response = await fetch(API_ENDPOINTS.tags.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<TagDto> {
    const response = await fetch(API_ENDPOINTS.tags.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateTagRequestDto): Promise<TagDto> {
    const response = await fetch(API_ENDPOINTS.tags.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateTagRequestDto): Promise<TagDto> {
    const response = await fetch(API_ENDPOINTS.tags.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.tags.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },
};
