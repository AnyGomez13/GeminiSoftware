import React, { createContext, useContext, useState, useEffect } from 'react';
import type { Usuario } from '../types';
import { api, ApiError } from '../services/apiClient';

interface AuthContextType {
  usuario: Usuario | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (username: string, password: string) => Promise<{ success: boolean; mensaje: string }>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const TOKEN_KEY = 'vetclinic_token';
const USER_KEY = 'vetclinic_user';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem(TOKEN_KEY);
      const storedUser = localStorage.getItem(USER_KEY);
      if (storedToken && storedUser) {
        setToken(storedToken);
        setUsuario(JSON.parse(storedUser));
      }
    } catch (e) {
      console.error('Error restaurando sesión local:', e);
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const login = async (username: string, password: string): Promise<{ success: boolean; mensaje: string }> => {
    try {
      const response = await api.auth.login(username, password);
      if (response.success && response.usuario) {
        const authToken = response.token || 'vetclinic_local_session';
        setToken(authToken);
        setUsuario(response.usuario);
        localStorage.setItem(TOKEN_KEY, authToken);
        localStorage.setItem(USER_KEY, JSON.stringify(response.usuario));
        return { success: true, mensaje: response.mensaje || 'Autenticación exitosa.' };
      } else {
        return { success: false, mensaje: response.mensaje || 'Credenciales incorrectas.' };
      }
    } catch (error: any) {
      const mensaje = error instanceof ApiError ? error.message : 'No se pudo conectar con el servidor local.';
      return { success: false, mensaje };
    }
  };

  const logout = () => {
    setUsuario(null);
    setToken(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  };

  return (
    <AuthContext.Provider
      value={{
        usuario,
        token,
        isAuthenticated: !!usuario,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe ser utilizado dentro de un AuthProvider');
  }
  return context;
};
