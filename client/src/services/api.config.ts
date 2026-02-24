const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const API_VERSION = 'v1';

export const API_ENDPOINTS = {
  auth: {
    login: `${API_BASE_URL}/api/${API_VERSION}/auth/login`,
  },
  users: {
    list: `${API_BASE_URL}/api/${API_VERSION}/users`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/users/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/users`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/users/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/users/${id}`,
    changePassword: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/users/${id}/change-password`,
  },
  roles: {
    list: `${API_BASE_URL}/api/${API_VERSION}/roles`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/roles/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/roles`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/roles/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/roles/${id}`,
  },
  equipments: {
    list: `${API_BASE_URL}/api/${API_VERSION}/equipments`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/equipments/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/equipments`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/equipments/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/equipments/${id}`,
  },
  tags: {
    list: `${API_BASE_URL}/api/${API_VERSION}/tags`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/tags/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/tags`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/tags/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/tags/${id}`,
  },
  lines: {
    list: `${API_BASE_URL}/api/${API_VERSION}/lines`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/lines/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/lines`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/lines/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/lines/${id}`,
  },
  ofs: {
    list: `${API_BASE_URL}/api/${API_VERSION}/ofs`,
    getById: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/ofs/${id}`,
    create: `${API_BASE_URL}/api/${API_VERSION}/ofs`,
    update: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/ofs/${id}`,
    delete: (id: number) => `${API_BASE_URL}/api/${API_VERSION}/ofs/${id}`,
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
