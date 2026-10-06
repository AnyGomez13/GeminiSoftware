import React from 'react';
import { useAuth } from '../../context/AuthContext';
import { LoginPage } from '../../pages/LoginPage';
import { Loader2, Stethoscope } from 'lucide-react';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen w-full bg-botanical-linen flex flex-col items-center justify-center p-6">
        <div className="w-16 h-16 rounded-2xl bg-botanical-forest flex items-center justify-center shadow-lg mb-4 animate-pulse">
          <Stethoscope className="w-8 h-8 text-white" />
        </div>
        <div className="flex items-center space-x-2 text-botanical-graphite font-semibold text-sm">
          <Loader2 className="w-4 h-4 animate-spin text-botanical-emerald" />
          <span>Iniciando estación clínica...</span>
        </div>
        <p className="text-xs text-botanical-muted mt-2">
          Verificando sesión local y credenciales
        </p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return <>{children}</>;
};
