import * as signalR from '@microsoft/signalr';
import type { Line, ProductionOrder } from '../types/production';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private readonly hubUrl: string;

  constructor() {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5050';
    this.hubUrl = `${baseUrl}/productionHub`;
  }

  // Initialize SignalR connection
  async startConnection(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => {
            const token = localStorage.getItem('accessToken');
            return token || '';
          },
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Reconnect with exponential backoff: 0s, 2s, 10s, 30s, then 30s
            if (retryContext.previousRetryCount === 0) return 0;
            if (retryContext.previousRetryCount === 1) return 2000;
            if (retryContext.previousRetryCount === 2) return 10000;
            return 30000;
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Connection lifecycle events
      this.connection.onreconnecting((error) => {
        console.warn('SignalR reconnecting...', error);
      });

      this.connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
      });

      this.connection.onclose((error) => {
        console.error('SignalR connection closed:', error);
      });

      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      throw error;
    }
  }

  // Stop SignalR connection
  async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR disconnected');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  // Subscribe to production line updates
  onLineUpdated(callback: (line: Line) => void): void {
    if (!this.connection) {
      console.error('SignalR connection not initialized');
      return;
    }

    this.connection.on('LineUpdated', callback);
  }

  // Subscribe to production order updates
  onOfUpdated(callback: (of: ProductionOrder) => void): void {
    if (!this.connection) {
      console.error('SignalR connection not initialized');
      return;
    }

    this.connection.on('OfUpdated', callback);
  }

  // Subscribe to production progress updates (when quantity changes)
  onProductionProgress(callback: (data: { ofId: number; qteProduite: number; qteTotale: number }) => void): void {
    if (!this.connection) {
      console.error('SignalR connection not initialized');
      return;
    }

    this.connection.on('ProductionProgress', callback);
  }

  // Unsubscribe from events
  off(eventName: string): void {
    if (this.connection) {
      this.connection.off(eventName);
    }
  }

  // Get connection state
  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null;
  }

  // Check if connected
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export default new SignalRService();
