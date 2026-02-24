import { API_ENDPOINTS, getAuthHeaders, handleApiError } from './api.config';
import type {
  EquipmentDto,
  CreateEquipmentRequestDto,
  UpdateEquipmentRequestDto,
} from '../types/api';

export const equipmentsService = {
  async getAll(): Promise<EquipmentDto[]> {
    const response = await fetch(API_ENDPOINTS.equipments.list, {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async getById(id: number): Promise<EquipmentDto> {
    const response = await fetch(API_ENDPOINTS.equipments.getById(id), {
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
    return response.json();
  },

  async create(data: CreateEquipmentRequestDto): Promise<EquipmentDto> {
    const response = await fetch(API_ENDPOINTS.equipments.create, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async update(id: number, data: UpdateEquipmentRequestDto): Promise<EquipmentDto> {
    const response = await fetch(API_ENDPOINTS.equipments.update(id), {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });
    await handleApiError(response);
    return response.json();
  },

  async delete(id: number): Promise<void> {
    const response = await fetch(API_ENDPOINTS.equipments.delete(id), {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    await handleApiError(response);
  },
};
