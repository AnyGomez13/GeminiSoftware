import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import {
  Stethoscope,
  Lock,
  User,
  Eye,
  EyeOff,
  ShieldCheck,
  AlertCircle,
  Loader2,
  HardDrive
} from 'lucide-react';
import { toast } from 'sonner';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username.trim() || !password) {
      setErrorMessage('Por favor ingrese su usuario y contraseña.');
      return;
    }

    setIsLoading(true);
    setErrorMessage(null);

    try {
      const result = await login(username.trim(), password);
      if (result.success) {
        toast.success('Sesión iniciada correctamente', {
          description: `Bienvenido a VetClinic Pro, ${username}.`
        });
      } else {
        setErrorMessage(result.mensaje || 'Credenciales incorrectas.');
        toast.error('Error de autenticación', {
          description: result.mensaje
        });
      }
    } catch {
      setErrorMessage('No se pudo conectar con el servidor local en localhost:5000.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen w-full bg-botanical-linen flex flex-col justify-center items-center p-4 sm:p-6 select-none relative overflow-hidden">
      {/* Botanical Background Soft Orbs */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-botanical-emerald/5 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-botanical-forest/5 rounded-full blur-3xl pointer-events-none" />

      {/* Main Login Card */}
      <div className="w-full max-w-md bg-white rounded-2xl shadow-xl border border-botanical-stone p-8 sm:p-10 z-10">
        {/* Clinic Identity */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-botanical-forest text-white shadow-md mb-4">
            <Stethoscope className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-botanical-graphite tracking-tight">
            VetClinic Pro
          </h1>
          <p className="text-sm text-botanical-muted mt-1 font-medium">
            Clínica Veterinaria • Dres. Fabio y William
          </p>
          <div className="mt-3 inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-50 text-emerald-800 text-xs font-semibold border border-emerald-200">
            <HardDrive className="w-3.5 h-3.5 text-botanical-emerald" />
            <span>Estación Monopuesto Local</span>
          </div>
        </div>

        {/* Error Feedback Banner */}
        {errorMessage && (
          <div className="mb-6 p-3.5 rounded-xl bg-red-50 border border-red-200 text-red-800 flex items-start gap-2.5 text-xs animate-fadeIn">
            <AlertCircle className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" />
            <div className="leading-relaxed font-medium">{errorMessage}</div>
          </div>
        )}

        {/* Form (CU-01, RF-01) */}
        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Campo Usuario */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase tracking-wider mb-1.5">
              Nombre de Usuario
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-botanical-subtle">
                <User className="w-4 h-4" />
              </div>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                autoFocus
                placeholder="Ej. admin"
                className="w-full pl-10 pr-3.5 py-2.5 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite placeholder-botanical-subtle focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald transition-all"
              />
            </div>
          </div>

          {/* Campo Contraseña */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase tracking-wider mb-1.5">
              Contraseña
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-botanical-subtle">
                <Lock className="w-4 h-4" />
              </div>
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder="••••••••••••"
                className="w-full pl-10 pr-10 py-2.5 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite placeholder-botanical-subtle focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald transition-all"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                tabIndex={-1}
                className="absolute inset-y-0 right-0 pr-3.5 flex items-center text-botanical-subtle hover:text-botanical-graphite transition-colors"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
          </div>

          {/* Botón de Envío */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full mt-2 py-3 px-4 rounded-xl bg-botanical-forest hover:bg-botanical-forestDark text-white text-sm font-semibold shadow-md hover:shadow-lg transition-all duration-150 flex items-center justify-center space-x-2 disabled:opacity-60 disabled:cursor-not-allowed transform active:scale-[0.99]"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Verificando credenciales...</span>
              </>
            ) : (
              <>
                <ShieldCheck className="w-4 h-4 text-emerald-300" />
                <span>Iniciar Sesión Clínica</span>
              </>
            )}
          </button>
        </form>

        {/* Security & Regulatory Footer Note */}
        <div className="mt-8 pt-6 border-t border-botanical-stone/80 text-center space-y-2">
          <p className="text-[11px] text-botanical-muted">
            Credenciales protegidas con <span className="font-semibold text-botanical-graphite">PBKDF2 HMAC-SHA256</span> (100.000 iteraciones).
          </p>
          <div className="text-[10px] text-botanical-subtle">
            Ley 576 de 2000 • Inmutabilidad y Trazabilidad Nominal
          </div>
        </div>
      </div>
    </div>
  );
};
