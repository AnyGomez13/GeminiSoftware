import React, { useState, useEffect } from 'react';
import type {
  Paciente,
  Veterinario,
  TipoBiologico,
  CrearInmunizacionDto
} from '../../types';
import { api, ApiError } from '../../services/apiClient';
import {
  X,
  Syringe,
  Calendar,
  UserCheck,
  Tag,
  FileText,
  AlertCircle,
  Loader2,
  ShieldCheck
} from 'lucide-react';
import { toast } from 'sonner';

interface NuevaInmunizacionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  paciente: Paciente;
}

export const NuevaInmunizacionModal: React.FC<NuevaInmunizacionModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  paciente,
}) => {
  const [veterinarios, setVeterinarios] = useState<Veterinario[]>([]);
  const [veterinarioId, setVeterinarioId] = useState<number>(0);
  const [tipoBiologico, setTipoBiologico] = useState<TipoBiologico>('Vacuna');
  const [nombreProducto, setNombreProducto] = useState('');
  const [loteFabricante, setLoteFabricante] = useState('');
  const [fechaAplicacion, setFechaAplicacion] = useState(
    new Date().toISOString().substring(0, 10)
  );
  // Por defecto 1 año después para vacunas
  const nextYear = new Date();
  nextYear.setFullYear(nextYear.getFullYear() + 1);
  const [fechaRefuerzo, setFechaRefuerzo] = useState(
    nextYear.toISOString().substring(0, 10)
  );
  const [observaciones, setObservaciones] = useState('');

  const [isLoadingVets, setIsLoadingVets] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setNombreProducto('');
      setLoteFabricante('');
      const today = new Date().toISOString().substring(0, 10);
      setFechaAplicacion(today);

      const d = new Date();
      d.setFullYear(d.getFullYear() + 1);
      setFechaRefuerzo(d.toISOString().substring(0, 10));

      setObservaciones('');
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
  }, [isOpen]);

  if (!isOpen) return null;

  const handleTipoChange = (tipo: TipoBiologico) => {
    setTipoBiologico(tipo);
    const d = new Date(fechaAplicacion);
    if (tipo === 'Vacuna') {
      d.setFullYear(d.getFullYear() + 1); // 1 año para vacunas
    } else {
      d.setMonth(d.getMonth() + 3); // 3 meses para desparasitaciones
    }
    setFechaRefuerzo(d.toISOString().substring(0, 10));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    if (!veterinarioId) {
      setErrorMsg('Debe seleccionar obligatoriamente al profesional que aplica la dosis (RN-02).');
      return;
    }

    if (!nombreProducto.trim()) {
      setErrorMsg('El nombre de la vacuna o biológico es obligatorio.');
      return;
    }

    const dAplicacion = new Date(fechaAplicacion);
    const dRefuerzo = new Date(fechaRefuerzo);

    if (dRefuerzo <= dAplicacion) {
      setErrorMsg('La fecha de próximo refuerzo debe ser estrictamente posterior a la fecha de aplicación (RN-08).');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: CrearInmunizacionDto = {
        pacienteId: paciente.id,
        veterinarioId,
        tipoBiologico,
        nombreProducto: nombreProducto.trim(),
        loteFabricante: loteFabricante.trim() ? loteFabricante.trim() : null,
        fechaAplicacion: new Date(fechaAplicacion).toISOString(),
        fechaRefuerzo: new Date(fechaRefuerzo).toISOString(),
        observaciones: observaciones.trim() ? observaciones.trim() : null,
      };

      await api.inmunizaciones.crear(payload);
      toast.success('Biológico registrado correctamente', {
        description: `Se programó el refuerzo de ${nombreProducto} para ${new Date(fechaRefuerzo).toLocaleDateString('es-CO')}.`
      });
      onSuccess();
      onClose();
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al registrar inmunización.';
      setErrorMsg(msg);
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fadeIn">
      <div className="bg-white rounded-2xl shadow-2xl border border-botanical-stone max-w-lg w-full overflow-hidden flex flex-col max-h-[92vh]">
        {/* Header */}
        <div className="px-6 py-4 border-b border-botanical-stone flex items-center justify-between bg-botanical-linen/60">
          <div className="flex items-center gap-2">
            <span className="p-1.5 rounded-lg bg-botanical-forest text-white">
              <Syringe className="w-4 h-4 text-emerald-300" />
            </span>
            <div>
              <h3 className="text-base font-bold text-botanical-graphite">
                Registrar Biológico / Inmunización
              </h3>
              <p className="text-xs text-botanical-muted">
                Paciente: <strong className="text-botanical-graphite">{paciente.nombre}</strong>
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-botanical-muted hover:text-botanical-graphite hover:bg-stone-200 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit} className="p-6 overflow-y-auto space-y-4">
          {errorMsg && (
            <div className="p-3 rounded-xl bg-red-50 border border-red-200 text-red-800 text-xs font-semibold flex items-center gap-2">
              <AlertCircle className="w-4 h-4 flex-shrink-0" />
              <span>{errorMsg}</span>
            </div>
          )}

          {/* Tipo de Biológico */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1.5">
              Tipo de Biológico / Procedimiento *
            </label>
            <div className="grid grid-cols-2 gap-2">
              {(
                [
                  { id: 'Vacuna', label: 'Vacuna' },
                  { id: 'Desparasitante', label: 'Desparasitante' },
                ] as const
              ).map((t) => (
                <button
                  key={t.id}
                  type="button"
                  onClick={() => handleTipoChange(t.id)}
                  className={`py-2 px-3 text-center rounded-xl text-xs font-semibold border transition-all ${
                    tipoBiologico === t.id
                      ? 'bg-botanical-forest text-white border-botanical-forest shadow-sm'
                      : 'bg-stone-50 text-stone-700 border-stone-200 hover:bg-stone-100'
                  }`}
                >
                  {t.label}
                </button>
              ))}
            </div>
          </div>

          {/* Veterinario Responsable (RN-02, STF-01) */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
              Médico Veterinario Responsable *
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
                    {vet.nombre} (TP: {vet.tarjetaProfesional})
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Nombre del Producto y Lote */}
          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Nombre del Fármaco / Vacuna *
              </label>
              <div className="relative">
                <Tag className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  required
                  value={nombreProducto}
                  onChange={(e) => setNombreProducto(e.target.value)}
                  placeholder="Ej. Rabia, Pentavalente, Simparica..."
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-medium"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Lote Fabricante
              </label>
              <input
                type="text"
                value={loteFabricante}
                onChange={(e) => setLoteFabricante(e.target.value)}
                placeholder="L-12345"
                className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-mono uppercase"
              />
            </div>
          </div>

          {/* Fechas: Aplicación y Próximo Refuerzo (RN-08) */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Fecha Aplicación *
              </label>
              <div className="relative">
                <Calendar className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="date"
                  required
                  value={fechaAplicacion}
                  onChange={(e) => setFechaAplicacion(e.target.value)}
                  className="w-full pl-9 pr-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <label className="text-xs font-semibold text-botanical-forest uppercase">
                  Próximo Refuerzo *
                </label>
                <span className="text-[10px] text-emerald-700 bg-emerald-50 px-1.5 py-0.5 rounded font-bold">
                  Refuerzo
                </span>
              </div>
              <div className="relative">
                <Calendar className="w-4 h-4 text-botanical-emerald absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="date"
                  required
                  min={fechaAplicacion}
                  value={fechaRefuerzo}
                  onChange={(e) => setFechaRefuerzo(e.target.value)}
                  className="w-full pl-9 pr-3 py-2 text-xs bg-emerald-50/50 border border-emerald-300 rounded-xl text-botanical-graphite font-bold focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>
          </div>

          {/* Observaciones */}
          <div>
            <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
              Observaciones Clínicas / Reacciones
            </label>
            <div className="relative">
              <FileText className="w-4 h-4 text-botanical-subtle absolute left-3 top-3" />
              <textarea
                rows={2}
                value={observaciones}
                onChange={(e) => setObservaciones(e.target.value)}
                placeholder="Tolerancia del paciente, laboratorio, dosificación..."
                className="w-full pl-9 pr-3 py-2 text-xs bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              />
            </div>
          </div>

          {/* Modal Actions */}
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
                  <span>Guardando registro...</span>
                </>
              ) : (
                <>
                  <ShieldCheck className="w-4 h-4 text-emerald-300" />
                  <span>Registrar Dosis Oficial</span>
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
