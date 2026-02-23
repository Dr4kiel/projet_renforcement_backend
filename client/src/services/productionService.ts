import * as signalR from '@microsoft/signalr';
import api from './api';
import type { SensorData, ProductionOrder, Line } from '../types/production';

const SIGNALR_HUB_URL =
  import.meta.env.VITE_API_URL?.replace('/api/v1', '') || 'http://localhost:5000';

export class ProductionService {
  private hubConnection: signalR.HubConnection | null = null;

  async connectToHub(token: string): Promise<signalR.HubConnection> {
    if (this.hubConnection) {
      return this.hubConnection;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${SIGNALR_HUB_URL}/hubs/production`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start();
    return this.hubConnection;
  }

  async disconnect() {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  getConnection(): signalR.HubConnection | null {
    return this.hubConnection;
  }

  async getLatestSensorData(): Promise<SensorData> {
    const response = await api.get<SensorData>('/production/sensors/latest');
    return response.data;
  }

  async getAllLines(): Promise<Line[]> {
    const response = await api.get<Line[]>('/production/lines');
    return response.data;
  }

  async getAllOfs(): Promise<ProductionOrder[]> {
    const response = await api.get<ProductionOrder[]>('/production/ofs');
    return response.data;
  }
}

export const productionService = new ProductionService();
