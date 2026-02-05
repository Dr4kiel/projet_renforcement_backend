import api from './api';
import {
  type LoginCredentials,
  type ForgotPasswordRequest,
  type ResetPasswordRequest,
  type AuthResponse,
  type UserInfo,
  type ChangePasswordRequest,
} from '../types/auth';

class AuthService {
  // POST /auth/login
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/v1/auth/login', credentials);
    return response.data;
  }

  // POST /users/{id}/change-password
  async changePassword(userId: number, request: ChangePasswordRequest): Promise<void> {
    await api.post(`/api/v1/users/${userId}/change-password`, request);
  }

  async forgotPassword(request: ForgotPasswordRequest): Promise<void> {
    console.log('Forgot password - pas encore implémenté', request);
    // Une fois implémenté, décommenter :
    // await api.post('/api/auth/forgot-password', request);
  }

  async resetPassword(request: ResetPasswordRequest): Promise<void> {
    console.log('Reset password - pas encore implémenté', request);
    // Une fois implémenté, décommenter :
    // await api.post('/api/auth/reset-password', request);
  }

  // Gestion du localStorage
  saveToken(token: string): void {
    localStorage.setItem('accessToken', token);
  }

  saveUser(user: UserInfo): void {
    localStorage.setItem('user', JSON.stringify(user));
  }

  getStoredUser(): UserInfo | null {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;

    try {
      return JSON.parse(userStr) as UserInfo;
    } catch {
      return null;
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  clearAuth(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken() && !!this.getStoredUser();
  }
}

export default new AuthService();
