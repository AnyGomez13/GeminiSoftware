import React, { useState } from 'react';
import type { PuntoCurvaPeso } from '../../types';
import { Scale, TrendingUp, TrendingDown, Minus } from 'lucide-react';

interface CurvaPesoChartProps {
  puntos: PuntoCurvaPeso[];
  className?: string;
}

export const CurvaPesoChart: React.FC<CurvaPesoChartProps> = ({ puntos, className = '' }) => {
  const [hoveredPoint, setHoveredPoint] = useState<PuntoCurvaPeso | null>(null);

  if (!puntos || puntos.length === 0) {
    return (
      <div className={`bg-white rounded-2xl p-6 border border-botanical-stone text-center ${className}`}>
        <Scale className="w-8 h-8 text-botanical-subtle mx-auto mb-2" />
        <p className="text-xs text-botanical-muted">No se registran datos de peso para este paciente.</p>
      </div>
    );
  }

  // Ordenar cronológicamente ascendente para la curva
  const sorted = [...puntos].sort(
    (a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime()
  );

  const initialPeso = sorted[0].pesoKg;
  const currentPeso = sorted[sorted.length - 1].pesoKg;
  const diff = currentPeso - initialPeso;

  const minPeso = Math.max(0, Math.min(...sorted.map((p) => p.pesoKg)) * 0.9);
  const maxPeso = Math.max(...sorted.map((p) => p.pesoKg)) * 1.1 || 10;

  // Dimensiones SVG
  const width = 600;
  const height = 220;
  const paddingX = 50;
  const paddingY = 30;

  const getX = (index: number) => {
    if (sorted.length === 1) return width / 2;
    return paddingX + (index / (sorted.length - 1)) * (width - 2 * paddingX);
  };

  const getY = (peso: number) => {
    if (maxPeso === minPeso) return height / 2;
    return height - paddingY - ((peso - minPeso) / (maxPeso - minPeso)) * (height - 2 * paddingY);
  };

  const pointsPath = sorted
    .map((p, i) => `${i === 0 ? 'M' : 'L'} ${getX(i)} ${getY(p.pesoKg)}`)
    .join(' ');

  const areaPath =
    sorted.length > 1
      ? `${pointsPath} L ${getX(sorted.length - 1)} ${height - paddingY} L ${getX(0)} ${
          height - paddingY
        } Z`
      : '';

  return (
    <div className={`bg-white rounded-2xl p-6 border border-botanical-stone shadow-card ${className}`}>
      {/* Chart Header & Trend Stats */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-botanical-stone/80 gap-3">
        <div>
          <div className="flex items-center gap-2">
            <Scale className="w-4 h-4 text-botanical-emerald" />
            <h4 className="text-sm font-bold text-botanical-graphite">
              Curva de Evolución de Peso (Kg)
            </h4>
          </div>
          <p className="text-xs text-botanical-muted mt-0.5">
            Registro secuencial sincronizado con cada consulta médica (RN-05, RN-06)
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="px-3 py-1.5 rounded-xl bg-botanical-linen border border-botanical-stone/80 text-right">
            <div className="text-[10px] text-botanical-muted font-semibold uppercase">Actual</div>
            <div className="text-sm font-bold text-botanical-graphite font-mono">
              {currentPeso.toFixed(2)} Kg
            </div>
          </div>

          <div
            className={`px-3 py-1.5 rounded-xl border flex items-center gap-1.5 ${
              Math.abs(diff) < 0.05
                ? 'bg-stone-50 border-stone-200 text-stone-700'
                : diff > 0
                ? 'bg-emerald-50 border-emerald-200 text-emerald-800'
                : 'bg-amber-50 border-amber-200 text-amber-800'
            }`}
          >
            {Math.abs(diff) < 0.05 ? (
              <Minus className="w-3.5 h-3.5" />
            ) : diff > 0 ? (
              <TrendingUp className="w-3.5 h-3.5 text-botanical-emerald" />
            ) : (
              <TrendingDown className="w-3.5 h-3.5 text-botanical-amberDark" />
            )}
            <div className="text-right">
              <div className="text-[10px] font-semibold uppercase opacity-75">Variación</div>
              <div className="text-xs font-bold font-mono">
                {diff > 0 ? `+${diff.toFixed(2)}` : diff.toFixed(2)} Kg
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* SVG Canvas */}
      <div className="relative mt-4">
        <svg
          viewBox={`0 0 ${width} ${height}`}
          className="w-full h-52 overflow-visible select-none"
        >
          <defs>
            <linearGradient id="botanicalGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#059669" stopOpacity="0.25" />
              <stop offset="100%" stopColor="#059669" stopOpacity="0.0" />
            </linearGradient>
          </defs>

          {/* Grid Horizontal Lines */}
          {[0, 0.33, 0.66, 1].map((pct, i) => {
            const yVal = paddingY + pct * (height - 2 * paddingY);
            const pesoLabel = (maxPeso - pct * (maxPeso - minPeso)).toFixed(1);
            return (
              <g key={i}>
                <line
                  x1={paddingX}
                  y1={yVal}
                  x2={width - paddingX}
                  y2={yVal}
                  stroke="#E7E5E4"
                  strokeDasharray="4 4"
                />
                <text
                  x={paddingX - 8}
                  y={yVal + 3}
                  textAnchor="end"
                  fontSize="10"
                  fill="#78716C"
                  fontFamily="monospace"
                >
                  {pesoLabel}
                </text>
              </g>
            );
          })}

          {/* Fill Area Gradient */}
          {areaPath && <path d={areaPath} fill="url(#botanicalGradient)" />}

          {/* Main Weight Curve Stroke */}
          {sorted.length > 1 && (
            <path
              d={pointsPath}
              fill="none"
              stroke="#059669"
              strokeWidth="2.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          )}

          {/* Data Points */}
          {sorted.map((p, i) => {
            const cx = getX(i);
            const cy = getY(p.pesoKg);
            const isHovered = hoveredPoint === p;
            const dateStr = new Date(p.fecha).toLocaleDateString('es-CO', {
              day: '2-digit',
              month: 'short',
            });

            return (
              <g
                key={i}
                className="cursor-pointer"
                onMouseEnter={() => setHoveredPoint(p)}
                onMouseLeave={() => setHoveredPoint(null)}
              >
                {/* Outer halo */}
                <circle
                  cx={cx}
                  cy={cy}
                  r={isHovered ? 8 : 5}
                  fill="#FFFFFF"
                  stroke="#059669"
                  strokeWidth={isHovered ? 3 : 2}
                  className="transition-all duration-150"
                />
                {/* Inner dot */}
                <circle cx={cx} cy={cy} r={isHovered ? 4 : 2.5} fill="#166534" />

                {/* Date Label on X Axis */}
                <text
                  x={cx}
                  y={height - 8}
                  textAnchor="middle"
                  fontSize="10"
                  fill="#78716C"
                  className="font-sans"
                >
                  {dateStr}
                </text>
              </g>
            );
          })}
        </svg>

        {/* Floating Tooltip */}
        {hoveredPoint && (
          <div className="absolute top-2 right-4 bg-botanical-forest text-white px-3 py-2 rounded-xl text-xs shadow-lg animate-fadeIn border border-white/10 pointer-events-none">
            <div className="font-bold flex items-center gap-1.5">
              <span>{hoveredPoint.pesoKg.toFixed(2)} Kg</span>
              <span className="text-[10px] text-emerald-300 font-normal">
                ({hoveredPoint.tipoEvento})
              </span>
            </div>
            <div className="text-[10px] text-white/80">
              {new Date(hoveredPoint.fecha).toLocaleDateString('es-CO', {
                day: '2-digit',
                month: 'long',
                year: 'numeric',
              })}
            </div>
            {hoveredPoint.detalle && (
              <div className="text-[10px] text-white/70 italic mt-0.5 truncate max-w-xs">
                {hoveredPoint.detalle}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
