import * as signalR from '@microsoft/signalr';
import axios, { type InternalAxiosRequestConfig } from 'axios';
import type { AgentStatus } from '../types/production';

const AGENT_CONTROL_URL =
  import.meta.env.VITE_AGENT_CONTROL_URL || 'http://localhost:5003';

// Instance Axios dédiée au service agent-control
const agentApi = axios.create({
  baseURL: AGENT_CONTROL_URL,
  headers: { 'Content-Type': 'application/json' },
});

agentApi.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

class AgentControlService {
  private connection: signalR.HubConnection | null = null;
  private readonly hubUrl: string;

  constructor() {
    this.hubUrl = `${AGENT_CONTROL_URL}/hubs/agents`;
  }

  async startConnection(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => localStorage.getItem('accessToken') ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    await this.connection.start();
    console.log('AgentControlService: SignalR connected to agents hub');
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  onAgentStatusUpdated(callback: (status: AgentStatus) => void): void {
    this.connection?.on('AgentStatusUpdated', callback);
  }

  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  async startLine(lineId: number): Promise<void> {
    await agentApi.post(`/api/agents/${lineId}/start`);
  }

  async stopLine(lineId: number): Promise<void> {
    await agentApi.post(`/api/agents/${lineId}/stop`);
  }
}

export default new AgentControlService();
