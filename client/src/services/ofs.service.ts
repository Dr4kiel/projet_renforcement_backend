import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type {
  OfDto,
  CreateOfRequestDto,
  UpdateOfRequestDto,
} from '../types/api';

export const ofsService = {
  async getAll(): Promise<OfDto[]> {
    const response = await fetch(API_ENDPOINTS.ofs.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<OfDto> {
    const response = await fetch(API_ENDPOINTS.ofs.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateOfRequestDto): Promise<OfDto> {
    const response = await fetch(API_ENDPOINTS.ofs.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateOfRequestDto): Promise<OfDto> {
    const response = await fetch(API_ENDPOINTS.ofs.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.ofs.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },
};
