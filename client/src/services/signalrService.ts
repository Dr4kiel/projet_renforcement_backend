import * as signalR from '@microsoft/signalr';
import type { ProductionMetricsDto } from '../types/production';

type ConnectionCallback = (error?: Error) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private readonly hubUrl: string;
  private onReconnectingCallback: ConnectionCallback | null = null;
  private onReconnectedCallback: ((connectionId?: string) => void) | null = null;
  private onCloseCallback: ConnectionCallback | null = null;

  constructor() {
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5050';
    this.hubUrl = `${baseUrl}/hubs/metrics`;
  }

  async startConnection(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => {
          const token = localStorage.getItem('accessToken');
          return token || '';
        },
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext: signalR.RetryContext) => {
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onreconnecting((error?: Error) => {
      console.warn('SignalR reconnecting...', error);
      this.onReconnectingCallback?.(error ?? undefined);
    });

    this.connection.onreconnected((connectionId?: string) => {
      console.log('SignalR reconnected:', connectionId);
      this.onReconnectedCallback?.(connectionId ?? undefined);
    });

    this.connection.onclose((error?: Error) => {
      console.error('SignalR connection closed:', error);
      this.onCloseCallback?.(error ?? undefined);
    });

    await this.connection.start();
    console.log('SignalR connected to metrics hub');
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async subscribeToLine(lineId: number): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      console.error('SignalR not connected, cannot subscribe to line', lineId);
      return;
    }
    await this.connection.invoke('SubscribeToLine', lineId);
  }

  async unsubscribeFromLine(lineId: number): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      return;
    }
    await this.connection.invoke('UnsubscribeFromLine', lineId);
  }

  onReceiveMetrics(callback: (data: ProductionMetricsDto) => void): void {
    if (!this.connection) return;
    this.connection.on('ReceiveMetrics', callback);
  }

  onReceiveLineMetrics(callback: (data: ProductionMetricsDto) => void): void {
    if (!this.connection) return;
    this.connection.on('ReceiveLineMetrics', callback);
  }

  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  setOnReconnecting(callback: ConnectionCallback): void {
    this.onReconnectingCallback = callback;
  }

  setOnReconnected(callback: (connectionId?: string) => void): void {
    this.onReconnectedCallback = callback;
  }

  setOnClose(callback: ConnectionCallback): void {
    this.onCloseCallback = callback;
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export default new SignalRService();
