import React, { useState, useEffect } from 'react';
import type { Recordatorio } from '../types';
import { api, ApiError } from '../services/apiClient';
import { RecordatorioCard } from '../components/recordatorios/RecordatorioCard';
import {
  Bell,
  Clock,
  AlertTriangle,
  RefreshCw,
  Loader2,
  CheckCircle2,
  MessageCircle
} from 'lucide-react';
import { toast } from 'sonner';

interface RecordatoriosPageProps {
  onGoToVacunacion?: (pacienteId: number) => void;
}

export const RecordatoriosPage: React.FC<RecordatoriosPageProps> = ({ onGoToVacunacion }) => {
  const [recordatorios, setRecordatorios] = useState<Recordatorio[]>([]);
  const [dias, setDias] = useState<number>(30);
  const [estadoFilter, setEstadoFilter] = useState<'todos' | 'proximos' | 'vencidos'>('todos');
  const [isLoading, setIsLoading] = useState(true);

  const fetchRecordatorios = async () => {
    setIsLoading(true);
    try {
      const data = await api.recordatorios.get(dias, estadoFilter);
      setRecordatorios(data);
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al cargar recordatorios.';
      toast.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchRecordatorios();
  }, [dias, estadoFilter]);

  const proximosCount = recordatorios.filter((r) => r.estado === 'Proximo').length;
  const vencidosCount = recordatorios.filter((r) => r.estado === 'Vencido').length;

  return (
    <div className="space-y-6">
      {/* Zero-Cost Messaging Clinical Banner */}
      <div className="bg-white rounded-2xl p-5 border border-botanical-stone shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-emerald-50 text-botanical-emerald flex items-center justify-center flex-shrink-0">
            <MessageCircle className="w-5 h-5" />
          </div>
          <div>
            <h3 className="text-sm font-bold text-botanical-graphite flex items-center gap-1.5">
              <span>Tablero de Recordatorios y Refuerzos 1-Clic</span>
              <span className="text-[10px] font-bold bg-emerald-100 text-emerald-800 px-2 py-0.5 rounded-full">
                Costo $0
              </span>
            </h3>
            <p className="text-xs text-botanical-muted mt-0.5">
              Notificaciones directas vía WhatsApp Web y correo nativo sin plataformas de pago ni pasarelas externas (RN-09, RNF-09).
            </p>
          </div>
        </div>

        <button
          onClick={fetchRecordatorios}
          title="Actualizar recordatorios"
          className="self-end md:self-center flex items-center gap-1.5 px-3 py-2 rounded-xl bg-botanical-linen hover:bg-stone-200 text-botanical-graphite text-xs font-semibold border border-botanical-stone transition-all"
        >
          <RefreshCw className="w-3.5 h-3.5 text-botanical-forest" />
          <span>Actualizar</span>
        </button>
      </div>

      {/* KPI Stats Overview */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-4 rounded-2xl border border-botanical-stone shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-botanical-muted uppercase">
              Total Monitoreados
            </span>
            <div className="text-2xl font-bold text-botanical-graphite mt-1">
              {recordatorios.length}
            </div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-stone-100 text-stone-600 flex items-center justify-center">
            <Bell className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white p-4 rounded-2xl border border-botanical-stone shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-botanical-amberDark uppercase">
              Próximos a Vencer
            </span>
            <div className="text-2xl font-bold text-amber-700 mt-1">
              {proximosCount}
            </div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
            <Clock className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white p-4 rounded-2xl border border-botanical-stone shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-red-700 uppercase">
              Refuerzos Vencidos
            </span>
            <div className="text-2xl font-bold text-red-700 mt-1">
              {vencidosCount}
            </div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-red-50 text-red-600 flex items-center justify-center">
            <AlertTriangle className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Filter and Time Horizon Bar */}
      <div className="bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        {/* Status Segmented Buttons */}
        <div className="flex items-center space-x-1 bg-botanical-linen/80 p-1 rounded-xl border border-botanical-stone">
          {(
            [
              { id: 'todos', label: 'Todos' },
              { id: 'proximos', label: 'Próximos' },
              { id: 'vencidos', label: 'Vencidos' },
            ] as const
          ).map((item) => (
            <button
              key={item.id}
              onClick={() => setEstadoFilter(item.id)}
              className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all ${
                estadoFilter === item.id
                  ? 'bg-botanical-forest text-white shadow-sm'
                  : 'text-botanical-muted hover:text-botanical-graphite'
              }`}
            >
              {item.label}
            </button>
          ))}
        </div>

        {/* Days Horizon Select */}
        <div className="flex items-center gap-2">
          <span className="text-xs font-semibold text-botanical-muted whitespace-nowrap">
            Ventana de Tiempo:
          </span>
          <select
            value={dias}
            onChange={(e) => setDias(Number(e.target.value))}
            className="px-3 py-1.5 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite font-bold focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
          >
            <option value={7}>Próximos 7 días</option>
            <option value={15}>Próximos 15 días</option>
            <option value={30}>Próximos 30 días (1 mes)</option>
            <option value={60}>Próximos 60 días (2 meses)</option>
            <option value={90}>Próximos 90 días (3 meses)</option>
          </select>
        </div>
      </div>

      {/* Grid of Reminder Cards */}
      {isLoading ? (
        <div className="flex flex-col items-center justify-center p-16 bg-white rounded-2xl border border-botanical-stone">
          <Loader2 className="w-8 h-8 animate-spin text-botanical-emerald mb-2" />
          <span className="text-xs text-botanical-muted">Monitoreando refuerzos pendientes...</span>
        </div>
      ) : recordatorios.length === 0 ? (
        <div className="flex flex-col items-center justify-center p-16 bg-white rounded-2xl border border-botanical-stone text-center">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-botanical-emerald flex items-center justify-center mb-3">
            <CheckCircle2 className="w-6 h-6" />
          </div>
          <h4 className="text-sm font-bold text-botanical-graphite">
            ¡Todos los refuerzos están al día!
          </h4>
          <p className="text-xs text-botanical-muted mt-1 max-w-sm">
            No se registran biológicos próximos a vencer en los siguientes {dias} días ni refuerzos pendientes bajo el filtro seleccionado.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {recordatorios.map((rec) => (
            <RecordatorioCard
              key={rec.inmunizacionId}
              recordatorio={rec}
              onGoToVacunacion={onGoToVacunacion}
            />
          ))}
        </div>
      )}
    </div>
  );
};
