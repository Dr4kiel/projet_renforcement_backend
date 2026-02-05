import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type {
  LineDto,
  CreateLineRequestDto,
  UpdateLineRequestDto,
} from '../types/api';

export const linesService = {
  async getAll(): Promise<LineDto[]> {
    const response = await fetch(API_ENDPOINTS.lines.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<LineDto> {
    const response = await fetch(API_ENDPOINTS.lines.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateLineRequestDto): Promise<LineDto> {
    const response = await fetch(API_ENDPOINTS.lines.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateLineRequestDto): Promise<LineDto> {
    const response = await fetch(API_ENDPOINTS.lines.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.lines.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },
};
