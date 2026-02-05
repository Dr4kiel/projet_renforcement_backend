const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5050';

export const API_ENDPOINTS = {
  auth: {
    login: `${API_BASE_URL}/api/v1/auth/login`,
  },
  users: {
    list: `${API_BASE_URL}/api/v1/users`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/users/${id}`,
    create: `${API_BASE_URL}/api/v1/users`,
    update: (id: number) => `${API_BASE_URL}/api/v1/users/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/users/${id}`,
    changePassword: (id: number) => `${API_BASE_URL}/api/v1/users/${id}/change-password`,
  },
  roles: {
    list: `${API_BASE_URL}/api/v1/roles`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/roles/${id}`,
    create: `${API_BASE_URL}/api/v1/roles`,
    update: (id: number) => `${API_BASE_URL}/api/v1/roles/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/roles/${id}`,
  },
  equipments: {
    list: `${API_BASE_URL}/api/v1/equipments`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/equipments/${id}`,
    create: `${API_BASE_URL}/api/v1/equipments`,
    update: (id: number) => `${API_BASE_URL}/api/v1/equipments/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/equipments/${id}`,
  },
  tags: {
    list: `${API_BASE_URL}/api/v1/tags`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/tags/${id}`,
    create: `${API_BASE_URL}/api/v1/tags`,
    update: (id: number) => `${API_BASE_URL}/api/v1/tags/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/tags/${id}`,
  },
  lines: {
    list: `${API_BASE_URL}/api/v1/lines`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/lines/${id}`,
    create: `${API_BASE_URL}/api/v1/lines`,
    update: (id: number) => `${API_BASE_URL}/api/v1/lines/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/lines/${id}`,
  },
  ofs: {
    list: `${API_BASE_URL}/api/v1/ofs`,
    getById: (id: number) => `${API_BASE_URL}/api/v1/ofs/${id}`,
    create: `${API_BASE_URL}/api/v1/ofs`,
    update: (id: number) => `${API_BASE_URL}/api/v1/ofs/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/v1/ofs/${id}`,
  },
};

export const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('accessToken');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };
};

export const handleApiError = async (response: Response) => {
  if (!response.ok) {
    const error = await response.json().catch(() => ({ detail: 'Une erreur est survenue' }));
    throw new Error(error.detail || error.title || 'Une erreur est survenue');
  }
  return response;
};
