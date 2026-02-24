import api from './api';
import type { Line, ProductionOrder } from '../types/production';

export const productionApi = {
  // Get all production lines with their current and next production orders
  getAllLines: async (): Promise<Line[]> => {
    const response = await api.get<Line[]>('/api/v1/lines');
    return response.data;
  },

  // Get all production orders (OFs)
  getAllOfs: async (): Promise<ProductionOrder[]> => {
    const response = await api.get<ProductionOrder[]>('/api/v1/ofs');
    return response.data;
  },
};
