import React, { useState, useEffect } from 'react';
import type {
  Paciente,
  CrearPacienteDto,
  ActualizarPacienteDto,
  Propietario,
  Especie,
  Sexo,
  EstadoReproductivo
} from '../../types';
import { api, ApiError } from '../../services/apiClient';
import { PropietarioModal } from '../propietarios/PropietarioModal';
import {
  X,
  Dog,
  Cat,
  User,
  Calendar,
  Scale,
  Plus,
  Loader2,
  Check,
  AlertCircle
} from 'lucide-react';
import { toast } from 'sonner';

interface PacienteModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (paciente: Paciente) => void;
  pacienteToEdit?: Paciente | null;
  initialPropietarioId?: number;
}

export const PacienteModal: React.FC<PacienteModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  pacienteToEdit,
  initialPropietarioId,
}) => {
  const [propietarios, setPropietarios] = useState<Propietario[]>([]);
  const [propietarioId, setPropietarioId] = useState<number>(initialPropietarioId || 0);
  const [nombre, setNombre] = useState('');
  const [especie, setEspecie] = useState<Especie>('Canino');
  const [raza, setRaza] = useState('');
  const [sexo, setSexo] = useState<Sexo>('Macho');
  const [fechaNacimiento, setFechaNacimiento] = useState(
    new Date().toISOString().substring(0, 10)
  );
  const [esFechaEstimada, setEsFechaEstimada] = useState(false);
  const [pesoActualKg, setPesoActualKg] = useState<string>('5.0');
  const [colorSenas, setColorSenas] = useState('');
  const [estadoReproductivo, setEstadoReproductivo] = useState<EstadoReproductivo>('Entero');

  const [isLoadingProps, setIsLoadingProps] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [isQuickOwnerOpen, setIsQuickOwnerOpen] = useState(false);

  const isEditing = !!pacienteToEdit;

  // Cargar propietarios disponibles
  const loadPropietarios = async () => {
    setIsLoadingProps(true);
    try {
      const data = await api.propietarios.buscar();
      setPropietarios(data);
      if (!isEditing && !propietarioId && data.length > 0) {
        setPropietarioId(initialPropietarioId || data[0].id);
      }
    } catch {
      // Ignorar error secundario
    } finally {
      setIsLoadingProps(false);
    }
  };

  useEffect(() => {
    if (isOpen) {
      loadPropietarios();
      if (pacienteToEdit) {
        setPropietarioId(pacienteToEdit.propietarioId);
        setNombre(pacienteToEdit.nombre);
        setEspecie(pacienteToEdit.especie);
        setRaza(pacienteToEdit.raza);
        setSexo(pacienteToEdit.sexo);
        setFechaNacimiento(pacienteToEdit.fechaNacimiento.substring(0, 10));
        setEsFechaEstimada(pacienteToEdit.esFechaEstimada);
        setPesoActualKg(pacienteToEdit.pesoActualKg.toString());
        setColorSenas(pacienteToEdit.colorSenas || '');
        setEstadoReproductivo(pacienteToEdit.estadoReproductivo);
      } else {
        setNombre('');
        setEspecie('Canino');
        setRaza('');
        setSexo('Macho');
        setFechaNacimiento(new Date().toISOString().substring(0, 10));
        setEsFechaEstimada(false);
        setPesoActualKg('5.0');
        setColorSenas('');
        setEstadoReproductivo('Entero');
        if (initialPropietarioId) {
          setPropietarioId(initialPropietarioId);
        }
      }
      setErrorMsg(null);
    }
  }, [isOpen, pacienteToEdit, initialPropietarioId]);

  if (!isOpen) return null;

  // Cálculo de edad dinámico en cliente (RN-06)
  const calcularEdadPreview = () => {
    if (!fechaNacimiento) return '';
    const fecha = new Date(fechaNacimiento);
    const hoy = new Date();
    if (fecha > hoy) return 'Fecha futura no válida';

    const totalDias = Math.floor((hoy.getTime() - fecha.getTime()) / (1000 * 60 * 60 * 24));
    if (totalDias < 30) {
      return totalDias === 1 ? '1 día' : `${totalDias} días`;
    }

    let anios = hoy.getFullYear() - fecha.getFullYear();
    let meses = hoy.getMonth() - fecha.getMonth();
    const dias = hoy.getDate() - fecha.getDate();

    if (dias < 0) meses--;
    if (meses < 0) {
      anios--;
      meses += 12;
    }

    if (anios === 0) {
      return meses === 1 ? '1 mes' : `${meses} meses`;
    }
    if (meses === 0) {
      return anios === 1 ? '1 año' : `${anios} años`;
    }
    const txtAnios = anios === 1 ? '1 año' : `${anios} años`;
    const txtMeses = meses === 1 ? '1 mes' : `${meses} meses`;
    return `${txtAnios} y ${txtMeses}`;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    const pesoNum = parseFloat(pesoActualKg);
    if (isNaN(pesoNum) || pesoNum < 0.01 || pesoNum > 150.0) {
      setErrorMsg('El peso corporal debe estar entre 0.01 y 150.00 Kg (RN-05).');
      return;
    }

    if (!propietarioId) {
      setErrorMsg('Debe asociar un propietario responsable.');
      return;
    }

    if (!nombre.trim() || !raza.trim()) {
      setErrorMsg('Nombre y Raza son obligatorios.');
      return;
    }

    const fechaDate = new Date(fechaNacimiento);
    if (fechaDate > new Date()) {
      setErrorMsg('La fecha de nacimiento no puede ser una fecha futura.');
      return;
    }

    setIsSubmitting(true);
    try {
      let result: Paciente;
      if (isEditing && pacienteToEdit) {
        const updatePayload: ActualizarPacienteDto = {
          propietarioId: pacienteToEdit.propietarioId,
          nombre: nombre.trim(),
          especie,
          raza: raza.trim(),
          sexo,
          fechaNacimiento: new Date(fechaNacimiento).toISOString(),
          esFechaEstimada,
          pesoActualKg: pesoNum,
          colorSenas: colorSenas.trim() ? colorSenas.trim() : null,
          estadoReproductivo,
        };
        result = await api.pacientes.actualizar(pacienteToEdit.id, updatePayload);
        toast.success(`Paciente ${result.nombre} actualizado correctamente.`);
      } else {
        const createPayload: CrearPacienteDto = {
          propietarioId,
          nombre: nombre.trim(),
          especie,
          raza: raza.trim(),
          sexo,
          fechaNacimiento: new Date(fechaNacimiento).toISOString(),
          esFechaEstimada,
          pesoActualKg: pesoNum,
          colorSenas: colorSenas.trim() ? colorSenas.trim() : null,
          estadoReproductivo,
        };
        result = await api.pacientes.crear(createPayload);
        toast.success(`Paciente ${result.nombre} registrado con éxito.`);
      }

      onSuccess(result);
      onClose();
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al guardar el paciente.';
      setErrorMsg(msg);
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleQuickOwnerSuccess = (nuevoProp: Propietario) => {
    setPropietarios((prev) => [nuevoProp, ...prev]);
    setPropietarioId(nuevoProp.id);
  };

  return (
    <>
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fadeIn">
        <div className="bg-white rounded-2xl shadow-2xl border border-botanical-stone max-w-xl w-full overflow-hidden flex flex-col max-h-[92vh]">
          {/* Header */}
          <div className="px-6 py-4 border-b border-botanical-stone flex items-center justify-between bg-botanical-linen/50">
            <div>
              <h3 className="text-base font-bold text-botanical-graphite flex items-center gap-2">
                {especie === 'Canino' ? (
                  <Dog className="w-5 h-5 text-botanical-emerald" />
                ) : (
                  <Cat className="w-5 h-5 text-botanical-amberDark" />
                )}
                <span>{isEditing ? `Editar Paciente: ${nombre}` : 'Registrar Nuevo Paciente'}</span>
              </h3>
              <p className="text-xs text-botanical-muted mt-0.5">
                Ficha médica clínica de la mascota y asociación de tutor (RF-04, RF-05)
              </p>
            </div>
            <button
              onClick={onClose}
              className="p-1 rounded-lg text-botanical-muted hover:text-botanical-graphite hover:bg-stone-200 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Body Form */}
          <form onSubmit={handleSubmit} className="p-6 overflow-y-auto space-y-4">
            {errorMsg && (
              <div className="p-3 rounded-xl bg-red-50 border border-red-200 text-red-800 text-xs font-medium flex items-center gap-2">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                <span>{errorMsg}</span>
              </div>
            )}

            {/* Propietario Selector (RF-04) */}
            <div>
              <div className="flex items-center justify-between mb-1">
                <label className="text-xs font-semibold text-botanical-graphite uppercase">
                  Tutor / Propietario Responsable *
                </label>
                {!isEditing && (
                  <button
                    type="button"
                    onClick={() => setIsQuickOwnerOpen(true)}
                    className="inline-flex items-center text-xs text-botanical-emerald hover:text-botanical-emeraldHover font-semibold"
                  >
                    <Plus className="w-3.5 h-3.5 mr-0.5" />
                    <span>Nuevo Tutor</span>
                  </button>
                )}
              </div>
              <div className="relative">
                <User className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <select
                  disabled={isEditing || isLoadingProps}
                  value={propietarioId}
                  onChange={(e) => setPropietarioId(Number(e.target.value))}
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald disabled:bg-stone-100 disabled:cursor-not-allowed"
                >
                  <option value={0} disabled>
                    -- Seleccione un propietario registrado --
                  </option>
                  {propietarios.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nombreCompleto} ({p.tipoDocumento} {p.numeroDocumento} - {p.telefono})
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Datos Básicos: Nombre y Especie */}
            <div className="grid grid-cols-3 gap-3">
              <div className="col-span-2">
                <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                  Nombre de la Mascota *
                </label>
                <input
                  type="text"
                  required
                  value={nombre}
                  onChange={(e) => setNombre(e.target.value)}
                  placeholder="Ej. Maya, Osita, Max..."
                  className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                  Especie *
                </label>
                <select
                  value={especie}
                  onChange={(e) => setEspecie(e.target.value as Especie)}
                  className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                >
                  <option value="Canino">Canino</option>
                  <option value="Felino">Felino</option>
                  <option value="Otro">Otro</option>
                </select>
              </div>
            </div>

            {/* Raza y Sexo */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                  Raza *
                </label>
                <input
                  type="text"
                  required
                  value={raza}
                  onChange={(e) => setRaza(e.target.value)}
                  placeholder="Ej. Pastor Alemán, Criollo..."
                  className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                  Sexo *
                </label>
                <select
                  value={sexo}
                  onChange={(e) => setSexo(e.target.value as Sexo)}
                  className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                >
                  <option value="Macho">Macho</option>
                  <option value="Hembra">Hembra</option>
                </select>
              </div>
            </div>

            {/* Fecha Nacimiento y Edad Calculada (RN-06) */}
            <div className="grid grid-cols-2 gap-3 items-end">
              <div>
                <div className="flex items-center justify-between mb-1">
                  <label className="text-xs font-semibold text-botanical-graphite uppercase">
                    Fecha Nacimiento *
                  </label>
                </div>
                <div className="relative">
                  <Calendar className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                  <input
                    type="date"
                    required
                    max={new Date().toISOString().substring(0, 10)}
                    value={fechaNacimiento}
                    onChange={(e) => setFechaNacimiento(e.target.value)}
                    className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                  />
                </div>
              </div>

              {/* Vista previa de edad calculada */}
              <div className="bg-emerald-50/70 border border-emerald-200 rounded-xl p-2.5 flex items-center justify-between">
                <div>
                  <div className="text-[10px] text-emerald-800 font-bold uppercase tracking-wider">
                    Edad Actual Calculada
                  </div>
                  <div className="text-xs font-bold text-botanical-forest">
                    {calcularEdadPreview() || 'Ingrese fecha'}
                  </div>
                </div>
                <div className="flex items-center gap-1.5">
                  <label className="text-[11px] text-botanical-muted flex items-center gap-1 cursor-pointer select-none">
                    <input
                      type="checkbox"
                      checked={esFechaEstimada}
                      onChange={(e) => setEsFechaEstimada(e.target.checked)}
                      className="rounded border-botanical-stone text-botanical-emerald focus:ring-botanical-emerald"
                    />
                    <span>Estimada</span>
                  </label>
                </div>
              </div>
            </div>

            {/* Peso Corporal (RN-05) y Estado Reproductivo */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <div className="flex items-center justify-between mb-1">
                  <label className="text-xs font-semibold text-botanical-graphite uppercase">
                    Peso Actual (Kg) *
                  </label>
                  <span className="text-[10px] text-botanical-subtle">0.01 - 150.00 Kg</span>
                </div>
                <div className="relative">
                  <Scale className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                  <input
                    type="number"
                    step="0.01"
                    min="0.01"
                    max="150.00"
                    required
                    value={pesoActualKg}
                    onChange={(e) => setPesoActualKg(e.target.value)}
                    placeholder="Ej. 12.5"
                    className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-mono"
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                  Estado Reproductivo
                </label>
                <select
                  value={estadoReproductivo}
                  onChange={(e) => setEstadoReproductivo(e.target.value as EstadoReproductivo)}
                  className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                >
                  <option value="Entero">Entero (Sin esterilizar)</option>
                  <option value="CastradoEsterilizado">Castrado / Esterilizado</option>
                </select>
              </div>
            </div>

            {/* Color y Señas Particulares */}
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Color y Señas Particulares
              </label>
              <input
                type="text"
                value={colorSenas}
                onChange={(e) => setColorSenas(e.target.value)}
                placeholder="Ej. Manto negro con manchas fuego en patas y pecho..."
                className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              />
            </div>

            {/* Footer Buttons */}
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
                className="px-5 py-2 rounded-xl bg-botanical-emerald hover:bg-botanical-emeraldHover text-white text-xs font-semibold shadow-sm transition-all flex items-center gap-1.5 disabled:opacity-50"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="w-3.5 h-3.5 animate-spin" />
                    <span>Guardando...</span>
                  </>
                ) : (
                  <>
                    <Check className="w-3.5 h-3.5" />
                    <span>{isEditing ? 'Actualizar Ficha Médica' : 'Registrar Mascota'}</span>
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Modal de alta rápida de tutor */}
      <PropietarioModal
        isOpen={isQuickOwnerOpen}
        onClose={() => setIsQuickOwnerOpen(false)}
        onSuccess={handleQuickOwnerSuccess}
      />
    </>
  );
};
