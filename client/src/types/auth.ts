// Types et interfaces pour l'authentification (basé sur le Swagger backend)

export interface LoginCredentials {
  identifiant: string;
  password: string;
}

export interface RegisterCredentials {
  identifiant: string;
  email: string;
  password: string;
  roleId?: number;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface UserInfo {
  id: number;
  identifiant: string;
  email: string;
  roleName: string;
}

export interface AuthResponse {
  token: string;
  tokenType: string;
  expiresAt: string;
  user: UserInfo;
}

// Pour compatibilité (endpoints non implémentés dans le backend)
export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  password: string;
  confirmPassword: string;
}

export interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (credentials: RegisterCredentials) => Promise<void>;
  logout: () => void;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}
