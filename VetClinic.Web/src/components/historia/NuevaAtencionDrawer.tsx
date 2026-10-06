import React, { useState, useEffect } from 'react';
import type { Paciente, Veterinario, CrearAtencionDto } from '../../types';
import { api, ApiError } from '../../services/apiClient';
import {
  X,
  Stethoscope,
  Scale,
  Calendar,
  AlertTriangle,
  UserCheck,
  Check,
  Loader2,
  FileText,
  Pill
} from 'lucide-react';
import { toast } from 'sonner';

interface NuevaAtencionDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  paciente: Paciente;
}

export const NuevaAtencionDrawer: React.FC<NuevaAtencionDrawerProps> = ({
  isOpen,
  onClose,
  onSuccess,
  paciente,
}) => {
  const [veterinarios, setVeterinarios] = useState<Veterinario[]>([]);
  const [veterinarioId, setVeterinarioId] = useState<number>(0);
  const [fechaAtencion, setFechaAtencion] = useState<string>('');
  const [pesoKg, setPesoKg] = useState<string>(paciente.pesoActualKg.toString());
  const [motivoConsulta, setMotivoConsulta] = useState('');
  const [examenClinico, setExamenClinico] = useState('');
  const [diagnostico, setDiagnostico] = useState('');
  const [tratamiento, setTratamiento] = useState('');
  const [indicaciones, setIndicaciones] = useState('');
  const [proximoControl, setProximoControl] = useState('');

  const [isLoadingVets, setIsLoadingVets] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  // Cargar veterinarios oficiales (RN-02, STF-01)
  useEffect(() => {
    if (isOpen) {
      const initDate = new Date();
      // Formato YYYY-MM-DDTHH:mm para input datetime-local
      const localIso = new Date(initDate.getTime() - initDate.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
      setFechaAtencion(localIso);
      setPesoKg(paciente.pesoActualKg.toString());
      setMotivoConsulta('');
      setExamenClinico('');
      setDiagnostico('');
      setTratamiento('');
      setIndicaciones('');
      setProximoControl('');
      setErrorMsg(null);

      const loadVets = async () => {
        setIsLoadingVets(true);
        try {
          const vets = await api.veterinarios.getAll();
          setVeterinarios(vets);
          if (vets.length > 0) {
            setVeterinarioId(vets[0].id);
          }
        } catch {
          // Ignorar
        } finally {
          setIsLoadingVets(false);
        }
      };

      loadVets();
    }
  }, [isOpen, paciente]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    const pesoNum = parseFloat(pesoKg);
    if (isNaN(pesoNum) || pesoNum < 0.01 || pesoNum > 150.0) {
      setErrorMsg('El peso del paciente debe estar entre 0.01 y 150.00 Kg (RN-05).');
      return;
    }

    if (!veterinarioId) {
      setErrorMsg('Debe asignar obligatoriamente al veterinario tratante (Dr. Fabio o Dr. William).');
      return;
    }

    if (!motivoConsulta.trim() || !diagnostico.trim() || !tratamiento.trim()) {
      setErrorMsg('Motivo de consulta, Diagnóstico y Tratamiento son campos obligatorios.');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: CrearAtencionDto = {
        pacienteId: paciente.id,
        veterinarioId,
        fechaHoraAtencion: new Date(fechaAtencion).toISOString(),
        pesoConsultaKg: pesoNum,
        motivoConsulta: motivoConsulta.trim(),
        examenClinico: examenClinico.trim() ? examenClinico.trim() : null,
        diagnostico: diagnostico.trim(),
        tratamiento: tratamiento.trim(),
        indicaciones: indicaciones.trim() ? indicaciones.trim() : null,
        fechaControl: proximoControl ? new Date(proximoControl).toISOString() : null,
      };

      await api.atenciones.crear(payload);
      toast.success('Atención médica registrada exitosamente.', {
        description: `Se actualizó el peso de ${paciente.nombre} a ${pesoNum.toFixed(2)} Kg.`
      });
      onSuccess();
      onClose();
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al guardar la atención médica.';
      setErrorMsg(msg);
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex justify-end bg-black/40 backdrop-blur-sm animate-fadeIn">
      <div className="bg-white w-full max-w-xl h-full shadow-2xl border-l border-botanical-stone flex flex-col overflow-hidden animate-slideLeft">
        {/* Drawer Header */}
        <div className="px-6 py-4 border-b border-botanical-stone flex items-center justify-between bg-botanical-linen/60">
          <div>
            <div className="flex items-center gap-2">
              <span className="p-1.5 rounded-lg bg-botanical-forest text-white">
                <Stethoscope className="w-4 h-4 text-emerald-300" />
              </span>
              <h3 className="text-base font-bold text-botanical-graphite">
                Nueva Consulta Médica
              </h3>
            </div>
            <p className="text-xs text-botanical-muted mt-1">
              Paciente: <strong className="text-botanical-graphite">{paciente.nombre}</strong> ({paciente.especie} • {paciente.edadFormateada})
            </p>
          </div>

          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-botanical-muted hover:text-botanical-graphite hover:bg-stone-200 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Legal Inmutability Warning */}
        <div className="px-6 py-3 bg-amber-50 border-b border-amber-200 flex items-start gap-2.5 text-xs text-amber-900">
          <AlertTriangle className="w-4 h-4 text-amber-600 flex-shrink-0 mt-0.5" />
          <div className="leading-tight">
            <strong>Inmutabilidad Legal (Ley 576 de 2000):</strong> Una vez confirmada, esta atención médica quedará fijada en la historia clínica del paciente y no podrá ser modificada ni eliminada.
          </div>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="flex-1 p-6 overflow-y-auto space-y-4">
          {errorMsg && (
            <div className="p-3 rounded-xl bg-red-50 border border-red-200 text-red-800 text-xs font-semibold">
              {errorMsg}
            </div>
          )}

          {/* Veterinario Tratante (RN-02, STF-01) */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
              Médico Veterinario Tratante *
            </label>
            <div className="relative">
              <UserCheck className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
              <select
                disabled={isLoadingVets}
                value={veterinarioId}
                onChange={(e) => setVeterinarioId(Number(e.target.value))}
                className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-medium"
              >
                {veterinarios.map((vet) => (
                  <option key={vet.id} value={vet.id}>
                    {vet.nombre} — TP: {vet.tarjetaProfesional}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Fecha / Hora y Peso en Consulta */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Fecha y Hora de Atención *
              </label>
              <div className="relative">
                <Calendar className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="datetime-local"
                  required
                  value={fechaAtencion}
                  onChange={(e) => setFechaAtencion(e.target.value)}
                  className="w-full pl-9 pr-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <label className="text-xs font-semibold text-botanical-graphite uppercase">
                  Peso Consulta (Kg) *
                </label>
                <span className="text-[10px] text-botanical-subtle font-mono">0.01 - 150 Kg</span>
              </div>
              <div className="relative">
                <Scale className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="number"
                  step="0.01"
                  min="0.01"
                  max="150.00"
                  required
                  value={pesoKg}
                  onChange={(e) => setPesoKg(e.target.value)}
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-mono font-semibold"
                />
              </div>
            </div>
          </div>

          {/* Anamnesis / Motivo de Consulta */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
              Motivo de Consulta y Anamnesis *
            </label>
            <textarea
              required
              rows={3}
              value={motivoConsulta}
              onChange={(e) => setMotivoConsulta(e.target.value)}
              placeholder="Describa el motivo de la consulta, síntomas observados por el tutor, duración y antecedentes relevantes..."
              className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
            />
          </div>

          {/* Examen Clínico / Constantes Vitales */}
          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="text-xs font-semibold text-botanical-graphite uppercase">
                Examen Físico y Constantes Vitales
              </label>
              <span className="text-[10px] text-botanical-subtle">Opcional</span>
            </div>
            <textarea
              rows={2}
              value={examenClinico}
              onChange={(e) => setExamenClinico(e.target.value)}
              placeholder="Temperatura, FC, FR, TLLC, mucosas, palpación abdominal, estado corporal..."
              className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-mono"
            />
          </div>

          {/* Diagnóstico */}
          <div>
            <label className="block text-xs font-semibold text-botanical-forest uppercase mb-1 flex items-center gap-1">
              <FileText className="w-3.5 h-3.5 text-botanical-emerald" />
              <span>Diagnóstico o Presunción Diagnóstica *</span>
            </label>
            <textarea
              required
              rows={2}
              value={diagnostico}
              onChange={(e) => setDiagnostico(e.target.value)}
              placeholder="Juicio clínico definitivo o presuntivo..."
              className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite font-medium focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
            />
          </div>

          {/* Tratamiento y Receta */}
          <div>
            <label className="block text-xs font-semibold text-botanical-forest uppercase mb-1 flex items-center gap-1">
              <Pill className="w-3.5 h-3.5 text-botanical-emerald" />
              <span>Tratamiento y Prescripción Médica *</span>
            </label>
            <textarea
              required
              rows={3}
              value={tratamiento}
              onChange={(e) => setTratamiento(e.target.value)}
              placeholder="Medicamentos administrados y recetados, concentración, posología, vía y duración del tratamiento..."
              className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
            />
          </div>

          {/* Indicaciones para el Tutor y Próximo Control */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Indicaciones al Tutor
              </label>
              <textarea
                rows={2}
                value={indicaciones}
                onChange={(e) => setIndicaciones(e.target.value)}
                placeholder="Cuidados en casa, dieta..."
                className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Próximo Control Sugerido
              </label>
              <input
                type="date"
                min={new Date().toISOString().substring(0, 10)}
                value={proximoControl}
                onChange={(e) => setProximoControl(e.target.value)}
                className="w-full px-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              />
            </div>
          </div>

          {/* Drawer Actions */}
          <div className="pt-4 border-t border-botanical-stone flex items-center justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2 text-xs font-semibold text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100 rounded-xl transition-colors"
            >
              Cancelar
            </button>

            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2.5 rounded-xl bg-botanical-forest hover:bg-botanical-forestDark text-white text-xs font-semibold shadow-md transition-all flex items-center gap-1.5 disabled:opacity-50"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Registrando acto clínico...</span>
                </>
              ) : (
                <>
                  <Check className="w-4 h-4 text-emerald-300" />
                  <span>Confirmar e Inmutabilizar</span>
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
