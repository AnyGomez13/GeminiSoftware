import React from 'react';
import type { AtencionClinica } from '../../types';
import {
  Stethoscope,
  ShieldCheck,
  Scale,
  Calendar,
  AlertCircle,
  Pill,
  ClipboardList,
  Clock,
  UserCheck
} from 'lucide-react';

interface TimelineAtencionesProps {
  atenciones: AtencionClinica[];
  className?: string;
}

export const TimelineAtenciones: React.FC<TimelineAtencionesProps> = ({
  atenciones,
  className = '',
}) => {
  if (!atenciones || atenciones.length === 0) {
    return (
      <div className={`bg-white rounded-2xl p-12 border border-botanical-stone text-center ${className}`}>
        <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-botanical-emerald flex items-center justify-center mx-auto mb-3">
          <ClipboardList className="w-6 h-6" />
        </div>
        <h4 className="text-sm font-bold text-botanical-graphite">Sin registros clínicos previos</h4>
        <p className="text-xs text-botanical-muted mt-1 max-w-sm mx-auto">
          Este paciente aún no registra consultas médicas ni procedimientos clínicos en su expediente inmutable.
        </p>
      </div>
    );
  }

  // Ordenar cronológicamente descendente (RF-07, STF-05)
  const sorted = [...atenciones].sort(
    (a, b) => new Date(b.fechaHoraAtencion).getTime() - new Date(a.fechaHoraAtencion).getTime()
  );

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Immutability Banner */}
      <div className="bg-emerald-50/70 border border-emerald-200/80 rounded-2xl p-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-botanical-forest text-white flex items-center justify-center flex-shrink-0">
            <ShieldCheck className="w-5 h-5 text-emerald-300" />
          </div>
          <div>
            <h5 className="text-xs font-bold text-emerald-950">
              Expediente Clínico Inmutable (Ley 576 de 2000)
            </h5>
            <p className="text-[11px] text-emerald-800 leading-tight mt-0.5">
              Cada acto clínico cuenta con firma y trazabilidad nominal médica. Bloqueo de alteración y borrado según Código de Ética Veterinario.
            </p>
          </div>
        </div>
        <span className="hidden sm:inline-block px-2.5 py-1 bg-white text-emerald-800 border border-emerald-200 rounded-lg text-[10px] font-bold font-mono">
          {sorted.length} {sorted.length === 1 ? 'CONSULTA' : 'CONSULTAS'}
        </span>
      </div>

      {/* Timeline Elements */}
      <div className="relative pl-6 sm:pl-8 border-l-2 border-botanical-emerald/30 space-y-8 ml-3 sm:ml-4">
        {sorted.map((atencion) => {
          const fecha = new Date(atencion.fechaHoraAtencion);
          const fechaFormateada = fecha.toLocaleDateString('es-CO', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
          });
          const horaFormateada = fecha.toLocaleTimeString('es-CO', {
            hour: '2-digit',
            minute: '2-digit',
          });

          return (
            <div key={atencion.id} className="relative group">
              {/* Timeline Bullet Node */}
              <div className="absolute -left-[31px] sm:-left-[39px] top-1.5 w-7 h-7 rounded-full bg-botanical-forest text-white flex items-center justify-center border-4 border-botanical-linen shadow-sm">
                <Stethoscope className="w-3.5 h-3.5 text-emerald-300" />
              </div>

              {/* Consultation Card */}
              <div className="bg-white rounded-2xl p-6 border border-botanical-stone shadow-card hover:shadow-card-hover transition-all">
                {/* Header: Date, Doctor tratante & Weight */}
                <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-botanical-stone/80 gap-3">
                  <div>
                    <div className="flex items-center gap-2 text-xs font-semibold text-botanical-muted capitalize">
                      <Calendar className="w-3.5 h-3.5 text-botanical-emerald inline" />
                      <span>{fechaFormateada}</span>
                      <span className="text-botanical-stone">•</span>
                      <Clock className="w-3.5 h-3.5 text-stone-400 inline" />
                      <span>{horaFormateada}</span>
                    </div>

                    <div className="mt-1.5 flex items-center gap-2">
                      <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-bold bg-emerald-50 text-emerald-900 border border-emerald-200">
                        <UserCheck className="w-3.5 h-3.5 text-botanical-emerald" />
                        <span>{atencion.nombreVeterinario}</span>
                      </span>
                    </div>
                  </div>

                  {/* Weight at Consultation */}
                  <div className="flex items-center gap-2 self-start sm:self-center px-3 py-1.5 rounded-xl bg-botanical-linen border border-botanical-stone">
                    <Scale className="w-4 h-4 text-botanical-emerald" />
                    <div>
                      <span className="text-[10px] text-botanical-muted font-semibold uppercase block leading-none">
                        Peso Consulta
                      </span>
                      <span className="text-xs font-bold text-botanical-graphite font-mono">
                        {atencion.pesoConsultaKg.toFixed(2)} Kg
                      </span>
                    </div>
                  </div>
                </div>

                {/* Clinical Content Sections */}
                <div className="mt-4 space-y-4 text-xs">
                  {/* Motivo de Consulta / Anamnesis */}
                  <div>
                    <h6 className="font-bold text-botanical-muted uppercase text-[10px] tracking-wider mb-1">
                      Motivo de Consulta y Anamnesis
                    </h6>
                    <p className="text-botanical-graphite leading-relaxed whitespace-pre-wrap bg-stone-50/50 p-3 rounded-xl border border-stone-200/60">
                      {atencion.motivoConsulta}
                    </p>
                  </div>

                  {/* Examen Clínico / Constantes Vitales (si existe) */}
                  {atencion.examenClinico && (
                    <div>
                      <h6 className="font-bold text-botanical-muted uppercase text-[10px] tracking-wider mb-1">
                        Examen Físico y Constantes Vitales
                      </h6>
                      <p className="text-botanical-graphite leading-relaxed whitespace-pre-wrap bg-stone-50/50 p-3 rounded-xl border border-stone-200/60 font-mono text-[11px]">
                        {atencion.examenClinico}
                      </p>
                    </div>
                  )}

                  {/* Diagnóstico */}
                  <div>
                    <h6 className="font-bold text-botanical-forest uppercase text-[10px] tracking-wider mb-1 flex items-center gap-1">
                      <AlertCircle className="w-3 h-3 text-botanical-emerald" />
                      <span>Diagnóstico o Presunción Diagnóstica</span>
                    </h6>
                    <p className="text-botanical-graphite font-semibold leading-relaxed whitespace-pre-wrap bg-emerald-50/40 p-3 rounded-xl border border-emerald-200/60">
                      {atencion.diagnostico}
                    </p>
                  </div>

                  {/* Plan y Tratamiento Prescrito */}
                  <div>
                    <h6 className="font-bold text-botanical-forest uppercase text-[10px] tracking-wider mb-1 flex items-center gap-1">
                      <Pill className="w-3 h-3 text-botanical-emerald" />
                      <span>Plan Terapéutico y Formulación Farmacológica</span>
                    </h6>
                    <p className="text-botanical-graphite leading-relaxed whitespace-pre-wrap bg-stone-50/50 p-3 rounded-xl border border-stone-200/60">
                      {atencion.tratamiento}
                    </p>
                  </div>

                  {/* Indicaciones para el Tutor */}
                  {atencion.indicaciones && (
                    <div>
                      <h6 className="font-bold text-botanical-muted uppercase text-[10px] tracking-wider mb-1">
                        Indicaciones para el Tutor / Acudiente
                      </h6>
                      <p className="text-botanical-muted leading-relaxed whitespace-pre-wrap bg-stone-50/50 p-3 rounded-xl border border-stone-200/60 italic">
                        {atencion.indicaciones}
                      </p>
                    </div>
                  )}

                  {/* Próximo Control Sugerido */}
                  {atencion.fechaControl && (
                    <div className="pt-2">
                      <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-amber-50 text-amber-900 border border-amber-200 font-semibold text-xs">
                        <Calendar className="w-3.5 h-3.5 text-botanical-amberDark" />
                        <span>
                          Próximo Control Sugerido:{' '}
                          {new Date(atencion.fechaControl).toLocaleDateString('es-CO', {
                            day: '2-digit',
                            month: 'long',
                            year: 'numeric',
                          })}
                        </span>
                      </span>
                    </div>
                  )}
                </div>

                {/* Footer Legal Badge */}
                <div className="mt-4 pt-3 border-t border-botanical-stone/60 flex items-center justify-between text-[10px] text-botanical-subtle">
                  <span>Acto Clínico Registrado por {atencion.nombreVeterinario}</span>
                  <span className="font-mono">Registro Oficial #{atencion.id}</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
