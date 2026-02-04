import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import {
  type AuthContextType,
  type LoginCredentials,
  type RegisterCredentials,
  type UserInfo,
} from '../types/auth';

// Créer le Context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Props pour le Provider
interface AuthProviderProps {
  children: ReactNode;
}

// Provider component
export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  // Computed property
  const isAuthenticated = !!user;

  // Charger le user depuis localStorage au montage
  useEffect(() => {
    const loadUser = () => {
      try {
        const storedUser = authService.getStoredUser();
        const token = authService.getAccessToken();

        if (storedUser && token) {
          setUser(storedUser);
        }
      } catch (error) {
        console.error('Erreur lors du chargement de l\'utilisateur:', error);
        authService.clearAuth();
      } finally {
        setIsLoading(false);
      }
    };

    loadUser();
  }, []);

  // Login
  const login = async (credentials: LoginCredentials): Promise<void> => {
    setIsLoading(true);
    try {
      // POST /api/v1/Auth/login
      const response = await authService.login(credentials);

      // Sauvegarder token et user
      authService.saveToken(response.token);
      authService.saveUser(response.user);
      setUser(response.user);

      // Rediriger vers dashboard
      navigate('/dashboard');
    } catch (error) {
      console.error('Erreur lors de la connexion:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Register
  const register = async (credentials: RegisterCredentials): Promise<void> => {
    setIsLoading(true);
    try {
      // POST /api/v1/Users (création d'utilisateur)
      await authService.register(credentials);

      // Connecter automatiquement après inscription
      await login({
        identifiant: credentials.identifiant,
        password: credentials.password,
      });
    } catch (error) {
      console.error('Erreur lors de l\'inscription:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Logout
  const logout = (): void => {
    authService.clearAuth();
    setUser(null);
    navigate('/login');
  };

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

// Hook personnalisé pour utiliser le AuthContext
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth doit être utilisé dans un AuthProvider');
  }

  return context;
};
