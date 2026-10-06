import React from 'react';
import type { Recordatorio } from '../../types';
import {
  Calendar,
  User,
  Phone,
  Mail,
  MessageCircle,
  AlertTriangle,
  Clock,
  Syringe,
  ExternalLink
} from 'lucide-react';

interface RecordatorioCardProps {
  recordatorio: Recordatorio;
  onGoToVacunacion?: (pacienteId: number) => void;
}

export const RecordatorioCard: React.FC<RecordatorioCardProps> = ({
  recordatorio,
  onGoToVacunacion,
}) => {
  const isVencido = recordatorio.estado === 'Vencido';
  const diasTexto = isVencido
    ? `Venció hace ${Math.abs(recordatorio.diasDiferencia)} ${
        Math.abs(recordatorio.diasDiferencia) === 1 ? 'día' : 'días'
      }`
    : recordatorio.diasDiferencia === 0
    ? 'Vence hoy'
    : `Vence en ${recordatorio.diasDiferencia} ${
        recordatorio.diasDiferencia === 1 ? 'día' : 'días'
      }`;

  const fechaRefuerzoFormateada = new Date(recordatorio.fechaRefuerzo).toLocaleDateString(
    'es-CO',
    {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
    }
  );

  return (
    <div
      className={`bg-white rounded-2xl p-5 border shadow-card hover:shadow-card-hover transition-all flex flex-col justify-between ${
        isVencido ? 'border-red-200/90' : 'border-botanical-stone'
      }`}
    >
      <div>
        {/* Header Status & Countdown */}
        <div className="flex items-start justify-between gap-2">
          <span
            className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl text-xs font-bold ${
              isVencido
                ? 'bg-red-50 text-red-800 border border-red-200'
                : 'bg-amber-50 text-amber-900 border border-amber-200'
            }`}
          >
            {isVencido ? (
              <AlertTriangle className="w-3.5 h-3.5 text-red-600" />
            ) : (
              <Clock className="w-3.5 h-3.5 text-amber-600" />
            )}
            <span>{diasTexto}</span>
          </span>

          <span className="text-[11px] font-mono font-medium text-botanical-muted">
            {recordatorio.especiePaciente}
          </span>
        </div>

        {/* Patient & Biologic Product */}
        <div className="mt-3">
          <div className="flex items-baseline justify-between">
            <h3 className="text-base font-bold text-botanical-graphite leading-tight">
              {recordatorio.nombrePaciente}
            </h3>
            {onGoToVacunacion && (
              <button
                onClick={() => onGoToVacunacion(recordatorio.pacienteId)}
                title="Ver carnet"
                className="text-[11px] text-botanical-forest hover:underline font-semibold flex items-center"
              >
                <span>Ver carnet</span>
                <ExternalLink className="w-3 h-3 ml-0.5" />
              </button>
            )}
          </div>

          <div className="mt-1 flex items-center gap-1.5 text-xs text-botanical-forest font-semibold">
            <Syringe className="w-3.5 h-3.5 text-botanical-emerald" />
            <span>
              {recordatorio.nombreProducto} ({recordatorio.tipoBiologico})
            </span>
          </div>

          <div className="mt-1 text-xs text-botanical-muted flex items-center gap-1.5">
            <Calendar className="w-3.5 h-3.5 text-stone-400" />
            <span>Fecha Programada: {fechaRefuerzoFormateada}</span>
          </div>
        </div>

        {/* Tutor & Contact Details */}
        <div className="mt-4 p-3 rounded-xl bg-botanical-linen/60 border border-botanical-stone text-xs space-y-1.5">
          <div className="flex items-center gap-1.5 font-bold text-botanical-graphite">
            <User className="w-3.5 h-3.5 text-botanical-emerald flex-shrink-0" />
            <span className="truncate">{recordatorio.nombrePropietario}</span>
          </div>

          <div className="flex items-center gap-1.5 text-botanical-muted font-mono text-[11px]">
            <Phone className="w-3.5 h-3.5 text-stone-400 flex-shrink-0" />
            <span>{recordatorio.telefonoPropietario}</span>
          </div>

          {recordatorio.emailPropietario && (
            <div className="flex items-center gap-1.5 text-botanical-muted text-[11px] truncate">
              <Mail className="w-3.5 h-3.5 text-stone-400 flex-shrink-0" />
              <span className="truncate">{recordatorio.emailPropietario}</span>
            </div>
          )}
        </div>
      </div>

      {/* 1-Clic Native Messaging Actions (RN-09, RNF-09) */}
      <div className="mt-4 pt-3 border-t border-botanical-stone flex items-center gap-2">
        {/* WhatsApp 1-Clic */}
        <a
          href={recordatorio.whatsAppUrl}
          target="_blank"
          rel="noopener noreferrer"
          title="Abrir chat en WhatsApp Web con mensaje prellenado (Costo $0)"
          className="flex-1 py-2 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-750 hover:bg-emerald-700 text-white text-xs font-semibold flex items-center justify-center gap-1.5 shadow-sm transition-all transform active:scale-95"
        >
          <MessageCircle className="w-3.5 h-3.5" />
          <span>WhatsApp 1-Clic</span>
        </a>

        {/* Mail 1-Clic (si tiene email registrado) */}
        {recordatorio.mailtoUrl && (
          <a
            href={recordatorio.mailtoUrl}
            title="Enviar recordatorio por correo electrónico (Costo $0)"
            className="p-2 rounded-xl bg-stone-100 hover:bg-stone-200 text-botanical-graphite transition-colors flex items-center justify-center"
          >
            <Mail className="w-4 h-4 text-stone-600" />
          </a>
        )}
      </div>
    </div>
  );
};
