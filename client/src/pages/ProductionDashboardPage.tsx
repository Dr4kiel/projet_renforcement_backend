import { useEffect, useRef, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { productionApi } from '../services/productionApi';
import signalrService from '../services/signalrService';
import type { Line, ProductionMetricsDto, ProductionOrder } from '../types/production';

interface SensorData {
  pressure: number;
  speed: number;
  temperature: number;
  vibration: number;
}

function extractSensorData(metrics: Record<string, number>): SensorData {
  const data: SensorData = { pressure: 0, speed: 0, temperature: 0, vibration: 0 };
  for (const [key, value] of Object.entries(metrics)) {
    const upper = key.toUpperCase();
    if (upper.startsWith('PRESSURE_')) data.pressure = value;
    else if (upper.startsWith('SPEED_')) data.speed = value;
    else if (upper.startsWith('TEMP_')) data.temperature = value;
    else if (upper.startsWith('VIBRATION_')) data.vibration = value;
  }
  return data;
}

export const ProductionDashboardPage = () => {
  const { user, logout } = useAuth();
  const [sensorData, setSensorData] = useState<SensorData>({
    pressure: 0,
    speed: 0,
    temperature: 0,
    vibration: 0,
  });
  const [lines, setLines] = useState<Line[]>([]);
  const [allOfs, setAllOfs] = useState<ProductionOrder[]>([]);
  const [selectedLineId, setSelectedLineId] = useState<number | null>(null);
  const [productionOrders, setProductionOrders] = useState<ProductionOrder[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSignalRConnected, setIsSignalRConnected] = useState(false);
  const previousLineIdRef = useRef<number | null>(null);
  const selectedLineIdRef = useRef<number | null>(null);

  // Load lines and OFs from backend
  useEffect(() => {
    const loadData = async () => {
      try {
        setIsLoading(true);

        const [linesData, ofsData] = await Promise.all([
          productionApi.getAllLines(),
          productionApi.getAllOfs(),
        ]);

        setLines(linesData);
        setAllOfs(ofsData);

        if (linesData.length > 0) {
          setSelectedLineId(linesData[0].id);
        }

        setError(null);
      } catch (err) {
        console.error('Error loading data:', err);
        setError('Erreur lors du chargement des données de production');
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, []);

  // Update production orders when selected line changes
  useEffect(() => {
    if (selectedLineId === null) {
      setProductionOrders([]);
      return;
    }

    const selectedLine = lines.find((line) => line.id === selectedLineId);
    if (!selectedLine) {
      setProductionOrders([]);
      return;
    }

    const ofs: ProductionOrder[] = [];

    if (selectedLine.ofEnCoursId) {
      const ofEnCours = allOfs.find((of) => of.id === selectedLine.ofEnCoursId);
      if (ofEnCours) {
        ofs.push({ ...ofEnCours, lineName: selectedLine.name });
      }
    }

    if (selectedLine.ofSuivantId) {
      const ofSuivant = allOfs.find((of) => of.id === selectedLine.ofSuivantId);
      if (ofSuivant) {
        ofs.push({ ...ofSuivant, lineName: selectedLine.name });
      }
    }

    setProductionOrders(ofs);
  }, [selectedLineId, lines, allOfs]);

  // Keep ref in sync with selectedLineId for use in SignalR callbacks
  useEffect(() => {
    selectedLineIdRef.current = selectedLineId;
  }, [selectedLineId]);

  // SignalR connection
  useEffect(() => {
    const handleMetrics = (data: ProductionMetricsDto) => {
      // Filter: only process metrics for the currently selected line
      if (data.lineId !== selectedLineIdRef.current) return;

      setSensorData(extractSensorData(data.metrics));

      if (data.status) {
        setAllOfs((prevOfs) =>
          prevOfs.map((of) => {
            if (of.of === data.status.currentOf) {
              return {
                ...of,
                qteProduite: data.status.qteProduite,
                qteTotale: data.status.qteTotale,
              };
            }
            return of;
          })
        );
      }
    };

    const setupSignalR = async () => {
      try {
        signalrService.setOnReconnecting(() => setIsSignalRConnected(false));
        signalrService.setOnReconnected(() => setIsSignalRConnected(true));
        signalrService.setOnClose(() => setIsSignalRConnected(false));

        await signalrService.startConnection();
        setIsSignalRConnected(true);

        // Listen to both broadcast (all lines) and line-specific events
        signalrService.onReceiveMetrics(handleMetrics);
        signalrService.onReceiveLineMetrics(handleMetrics);
      } catch (err) {
        console.error('Failed to setup SignalR:', err);
        setIsSignalRConnected(false);
      }
    };

    setupSignalR();

    return () => {
      signalrService.off('ReceiveMetrics');
      signalrService.off('ReceiveLineMetrics');
      signalrService.stopConnection().then(() => setIsSignalRConnected(false));
    };
  }, []);

  // Subscribe/unsubscribe to line when selection changes
  useEffect(() => {
    if (!isSignalRConnected) return;

    const switchLine = async () => {
      // Unsubscribe from previous line
      if (previousLineIdRef.current !== null) {
        await signalrService.unsubscribeFromLine(previousLineIdRef.current);
      }

      // Subscribe to new line
      if (selectedLineId !== null) {
        await signalrService.subscribeToLine(selectedLineId);
      }

      previousLineIdRef.current = selectedLineId;
    };

    switchLine();
  }, [selectedLineId, isSignalRConnected]);

  const getProgressColor = (percentage: number) => {
    if (percentage >= 80) return 'bg-green-500';
    if (percentage >= 50) return 'bg-blue-500';
    if (percentage >= 25) return 'bg-yellow-500';
    return 'bg-red-500';
  };

  const getSensorStatus = (value: number, min: number, max: number) => {
    if (value < min || value > max)
      return { color: 'text-red-600', bg: 'bg-red-50', status: 'Alerte' };
    if (value < min + (max - min) * 0.1 || value > max - (max - min) * 0.1)
      return { color: 'text-yellow-600', bg: 'bg-yellow-50', status: 'Attention' };
    return { color: 'text-green-600', bg: 'bg-green-50', status: 'Normal' };
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Chargement des données de production...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md">
          <h2 className="text-red-800 font-bold text-lg mb-2">Erreur</h2>
          <p className="text-red-600">{error}</p>
          <button
            onClick={() => window.location.reload()}
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
          >
            Réessayer
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-3">
              <h1 className="text-xl font-bold text-gray-900">
                Dashboard de Production - Temps Réel
              </h1>
              <span
                className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                  isSignalRConnected
                    ? 'bg-green-100 text-green-800'
                    : 'bg-gray-100 text-gray-800'
                }`}
                title={isSignalRConnected ? 'Connecté en temps réel' : 'Déconnecté'}
              >
                <span
                  className={`w-2 h-2 mr-1.5 rounded-full ${
                    isSignalRConnected ? 'bg-green-500 animate-pulse' : 'bg-gray-400'
                  }`}
                ></span>
                {isSignalRConnected ? 'Live' : 'Hors ligne'}
              </span>
            </div>

            <div className="flex items-center space-x-4">
              <div className="text-sm">
                <p className="font-medium text-gray-900">{user?.identifiant}</p>
                <p className="text-gray-500">{user?.roleName}</p>
              </div>
              <button
                onClick={logout}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
              >
                Déconnexion
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main content */}
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Line Selector */}
        <div className="px-4 mb-6">
          <div className="bg-white shadow rounded-lg p-6">
            <label htmlFor="line-select" className="block text-sm font-medium text-gray-700 mb-2">
              Sélectionner une ligne de production
            </label>
            <select
              id="line-select"
              value={selectedLineId || ''}
              onChange={(e) => setSelectedLineId(Number(e.target.value))}
              className="block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            >
              {lines.length === 0 ? (
                <option value="">Aucune ligne disponible</option>
              ) : (
                lines.map((line) => (
                  <option key={line.id} value={line.id}>
                    {line.name} - {line.equipmentName}
                    {line.isChangement && ' (En changement)'}
                  </option>
                ))
              )}
            </select>
          </div>
        </div>

        {/* Métriques des capteurs */}
        <div className="px-4 mb-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Capteurs en Temps Réel</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {/* Pressure */}
            <div
              className={`${getSensorStatus(sensorData.pressure, 4.0, 8.0).bg} p-6 rounded-lg border-2 ${getSensorStatus(sensorData.pressure, 4.0, 8.0).color} border-current`}
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium">Pression</p>
                  <p className="text-3xl font-bold mt-2">{sensorData.pressure.toFixed(1)}</p>
                  <p className="text-xs mt-1">bar</p>
                </div>
                <div className="text-right">
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getSensorStatus(sensorData.pressure, 4.0, 8.0).bg}`}
                  >
                    {getSensorStatus(sensorData.pressure, 4.0, 8.0).status}
                  </span>
                  <p className="text-xs mt-2">4.0-8.0 bar</p>
                </div>
              </div>
            </div>

            {/* Speed */}
            <div
              className={`${getSensorStatus(sensorData.speed, 1200, 1800).bg} p-6 rounded-lg border-2 ${getSensorStatus(sensorData.speed, 1200, 1800).color} border-current`}
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium">Vitesse</p>
                  <p className="text-3xl font-bold mt-2">{sensorData.speed.toFixed(0)}</p>
                  <p className="text-xs mt-1">rpm</p>
                </div>
                <div className="text-right">
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getSensorStatus(sensorData.speed, 1200, 1800).bg}`}
                  >
                    {getSensorStatus(sensorData.speed, 1200, 1800).status}
                  </span>
                  <p className="text-xs mt-2">1200-1800 rpm</p>
                </div>
              </div>
            </div>

            {/* Temperature */}
            <div
              className={`${getSensorStatus(sensorData.temperature, 60, 90).bg} p-6 rounded-lg border-2 ${getSensorStatus(sensorData.temperature, 60, 90).color} border-current`}
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium">Température</p>
                  <p className="text-3xl font-bold mt-2">{sensorData.temperature.toFixed(1)}</p>
                  <p className="text-xs mt-1">°C</p>
                </div>
                <div className="text-right">
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getSensorStatus(sensorData.temperature, 60, 90).bg}`}
                  >
                    {getSensorStatus(sensorData.temperature, 60, 90).status}
                  </span>
                  <p className="text-xs mt-2">60-90 °C</p>
                </div>
              </div>
            </div>

            {/* Vibration */}
            <div
              className={`${getSensorStatus(sensorData.vibration, 1.0, 4.5).bg} p-6 rounded-lg border-2 ${getSensorStatus(sensorData.vibration, 1.0, 4.5).color} border-current`}
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium">Vibration</p>
                  <p className="text-3xl font-bold mt-2">{sensorData.vibration.toFixed(2)}</p>
                  <p className="text-xs mt-1">mm/s</p>
                </div>
                <div className="text-right">
                  <span
                    className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getSensorStatus(sensorData.vibration, 1.0, 4.5).bg}`}
                  >
                    {getSensorStatus(sensorData.vibration, 1.0, 4.5).status}
                  </span>
                  <p className="text-xs mt-2">1.0-4.5 mm/s</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Ordres de Fabrication */}
        <div className="px-4">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">
            Ordres de Fabrication en Cours
          </h2>
          {productionOrders.length === 0 ? (
            <div className="bg-gray-50 border border-gray-200 rounded-lg p-6 text-center">
              <p className="text-gray-600">
                {selectedLineId
                  ? 'Aucun ordre de fabrication pour cette ligne'
                  : 'Veuillez sélectionner une ligne'}
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {productionOrders.map((order) => {
                const progress = (order.qteProduite / order.qteTotale) * 100;
                return (
                  <div key={order.id} className="bg-white shadow rounded-lg p-6">
                    <div className="flex justify-between items-start mb-4">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{order.of}</h3>
                        <p className="text-sm text-gray-600">{order.produit}</p>
                        <p className="text-xs text-gray-500 mt-1">{order.lineName}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-2xl font-bold text-gray-900">{progress.toFixed(0)}%</p>
                        <p className="text-xs text-gray-500">
                          {order.qteProduite} / {order.qteTotale}
                        </p>
                      </div>
                    </div>

                    {/* Barre de progression */}
                    <div className="relative">
                      <div className="overflow-hidden h-4 text-xs flex rounded-full bg-gray-200">
                        <div
                          style={{ width: `${progress}%` }}
                          className={`shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center ${getProgressColor(progress)} transition-all duration-500`}
                        ></div>
                      </div>
                    </div>

                    {/* Statistiques */}
                    <div className="mt-4 grid grid-cols-3 gap-4 text-center">
                      <div>
                        <p className="text-xs text-gray-500">Produit</p>
                        <p className="text-lg font-semibold text-green-600">{order.qteProduite}</p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Restant</p>
                        <p className="text-lg font-semibold text-blue-600">
                          {order.qteTotale - order.qteProduite}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-500">Total</p>
                        <p className="text-lg font-semibold text-gray-900">{order.qteTotale}</p>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </main>
    </div>
  );
};
