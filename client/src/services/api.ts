import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';

// Créer une instance Axios avec configuration de base
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5050',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Ajouter le token JWT à chaque requête
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Gérer les erreurs 401
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    // Si erreur 401, déconnecter l'utilisateur
    if (error.response?.status === 401) {
      // Vérifier si on n'est pas déjà sur la page de login
      if (!window.location.pathname.includes('/login')) {
        localStorage.clear();
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

export default api;
