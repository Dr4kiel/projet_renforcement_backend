import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import {
  type AuthContextType,
  type LoginCredentials,
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

  const isAuthenticated = !!user;

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

  const login = async (credentials: LoginCredentials): Promise<void> => {
    setIsLoading(true);
    try {
      const response = await authService.login(credentials);

      authService.saveToken(response.token);
      authService.saveUser(response.user);
      setUser(response.user);

      navigate('/dashboard');
    } catch (error) {
      console.error('Erreur lors de la connexion:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

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
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth doit être utilisé dans un AuthProvider');
  }

  return context;
};
