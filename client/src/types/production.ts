export interface SensorData {
  pressure: number;
  speed: number;
  temperature: number;
  vibration: number;
  timestamp: string;
}

export interface LineStatusDto {
  currentOf: string;
  currentProduct: string;
  qteProduite: number;
  qteTotale: number;
  nextOf: string;
  isChangement: boolean;
}

export interface ProductionMetricsDto {
  lineId: number;
  lineName: string;
  timestamp: string;
  metrics: Record<string, number>;
  status: LineStatusDto;
}

export interface ProductionOrder {
  id: number;
  of: string;
  produit: string;
  qteProduite: number;
  qteTotale: number;
  lineName?: string | null;
}

export interface AgentStatus {
  lineId: string;
  status: 'running' | 'stopped';
  success: boolean;
  message?: string | null;
  timestamp: string;
}

// Response from /lines API
export interface Line {
  id: number;
  name: string;
  isChangement: boolean;
  tempsChangement: number;
  equipmentId: number;
  equipmentName: string;
  ofEnCoursId: number | null;
  ofEnCoursName: string | null;
  ofSuivantId: number | null;
  ofSuivantName: string | null;
}
