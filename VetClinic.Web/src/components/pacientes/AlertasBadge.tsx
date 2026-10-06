import React from 'react';
import type { Especie, EstadoReproductivo, Sexo } from '../../types';
import { Dog, Cat, Sparkles, AlertTriangle, CheckCircle2, ShieldAlert } from 'lucide-react';

interface EspecieBadgeProps {
  especie: Especie;
  className?: string;
}

export const EspecieBadge: React.FC<EspecieBadgeProps> = ({ especie, className = '' }) => {
  const isCanino = especie === 'Canino';
  const isFelino = especie === 'Felino';

  return (
    <span
      className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold ${
        isCanino
          ? 'bg-emerald-50 text-emerald-800 border border-emerald-200'
          : isFelino
          ? 'bg-amber-50 text-amber-800 border border-amber-200'
          : 'bg-stone-100 text-stone-700 border border-stone-200'
      } ${className}`}
    >
      {isCanino && <Dog className="w-3.5 h-3.5 text-botanical-emerald" />}
      {isFelino && <Cat className="w-3.5 h-3.5 text-botanical-amberDark" />}
      {!isCanino && !isFelino && <Sparkles className="w-3.5 h-3.5 text-stone-500" />}
      <span>{especie}</span>
    </span>
  );
};

interface SexoReproductivoBadgeProps {
  sexo: Sexo;
  estadoReproductivo: EstadoReproductivo;
  className?: string;
}

export const SexoReproductivoBadge: React.FC<SexoReproductivoBadgeProps> = ({
  sexo,
  estadoReproductivo,
  className = '',
}) => {
  const esCastrado = estadoReproductivo === 'CastradoEsterilizado';

  return (
    <span
      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-medium bg-botanical-linen border border-botanical-stone text-botanical-graphite ${className}`}
    >
      <span>{sexo}</span>
      <span className="text-botanical-subtle">•</span>
      <span className={esCastrado ? 'text-botanical-emerald font-semibold' : 'text-botanical-muted'}>
        {esCastrado ? 'Esterilizado/a' : 'Entero/a'}
      </span>
    </span>
  );
};

interface AlertaClinicaBadgeProps {
  tipo: 'alerta' | 'advertencia' | 'exito' | 'info';
  texto: string;
  className?: string;
}

export const AlertaClinicaBadge: React.FC<AlertaClinicaBadgeProps> = ({
  tipo,
  texto,
  className = '',
}) => {
  switch (tipo) {
    case 'alerta':
      return (
        <span
          className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-red-50 text-red-800 border border-red-200 ${className}`}
        >
          <ShieldAlert className="w-3.5 h-3.5 text-red-600" />
          <span>{texto}</span>
        </span>
      );
    case 'advertencia':
      return (
        <span
          className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-amber-50 text-amber-800 border border-amber-200 ${className}`}
        >
          <AlertTriangle className="w-3.5 h-3.5 text-amber-600" />
          <span>{texto}</span>
        </span>
      );
    case 'exito':
      return (
        <span
          className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-semibold bg-emerald-50 text-emerald-800 border border-emerald-200 ${className}`}
        >
          <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />
          <span>{texto}</span>
        </span>
      );
    default:
      return (
        <span
          className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-medium bg-stone-100 text-stone-700 border border-stone-200 ${className}`}
        >
          <span>{texto}</span>
        </span>
      );
  }
};
