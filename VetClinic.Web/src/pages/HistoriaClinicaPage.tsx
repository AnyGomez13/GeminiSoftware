import React, { useState, useEffect } from 'react';
import type { Paciente, PacienteDetalle } from '../types';
import { api, ApiError } from '../services/apiClient';
import { TimelineAtenciones } from '../components/historia/TimelineAtenciones';
import { CurvaPesoChart } from '../components/historia/CurvaPesoChart';
import { NuevaAtencionDrawer } from '../components/historia/NuevaAtencionDrawer';
import { EspecieBadge, SexoReproductivoBadge } from '../components/pacientes/AlertasBadge';
import {
  ClipboardList,
  Scale,
  Calendar,
  User,
  Phone,
  Plus,
  Loader2,
  RefreshCw,
  MessageCircle,
  TrendingUp
} from 'lucide-react';
import { toast } from 'sonner';

interface HistoriaClinicaPageProps {
  initialPacienteId?: number | null;
  onGoToVacunacion?: (pacienteId: number) => void;
}

export const HistoriaClinicaPage: React.FC<HistoriaClinicaPageProps> = ({
  initialPacienteId,
}) => {
  const [pacientesList, setPacientesList] = useState<Paciente[]>([]);
  const [selectedPacienteId, setSelectedPacienteId] = useState<number | null>(
    initialPacienteId || null
  );
  const [pacienteDetalle, setPacienteDetalle] = useState<PacienteDetalle | null>(null);
  const [isLoadingList, setIsLoadingList] = useState(true);
  const [isLoadingDetalle, setIsLoadingDetalle] = useState(false);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [activeSubTab, setActiveSubTab] = useState<'timeline' | 'curva'>('timeline');

  // Cargar lista de pacientes
  const loadPacientes = async () => {
    setIsLoadingList(true);
    try {
      const data = await api.pacientes.buscar();
      setPacientesList(data);
      if (!selectedPacienteId && data.length > 0) {
        setSelectedPacienteId(data[0].id);
      }
    } catch {
      // Ignorar
    } finally {
      setIsLoadingList(false);
    }
  };

  // Cargar expediente 360° del paciente seleccionado
  const loadPacienteDetalle = async (id: number) => {
    setIsLoadingDetalle(true);
    try {
      const detalle = await api.pacientes.getById(id);
      setPacienteDetalle(detalle);
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al cargar expediente.';
      toast.error(msg);
    } finally {
      setIsLoadingDetalle(false);
    }
  };

  useEffect(() => {
    loadPacientes();
  }, []);

  useEffect(() => {
    if (selectedPacienteId) {
      loadPacienteDetalle(selectedPacienteId);
    }
  }, [selectedPacienteId]);

  const handleConsultaCreada = () => {
    if (selectedPacienteId) {
      loadPacienteDetalle(selectedPacienteId);
    }
  };

  const getWhatsAppLink = (telefono?: string, pacienteNombre?: string, tutorNombre?: string) => {
    if (!telefono) return '#';
    const cleanPhone = telefono.replace(/\D/g, '');
    const mensaje = encodeURIComponent(
      `Hola ${tutorNombre || 'Tutor'}, te escribimos de la Clínica Veterinaria de los Dres. Fabio y William respecto a ${pacienteNombre || 'tu mascota'}.`
    );
    return `https://wa.me/57${cleanPhone}?text=${mensaje}`;
  };

  return (
    <div className="space-y-6">
      {/* Patient Selection Bar */}
      <div className="bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1 max-w-md">
          <label className="text-xs font-bold text-botanical-graphite uppercase whitespace-nowrap">
            Expediente de:
          </label>
          <select
            disabled={isLoadingList}
            value={selectedPacienteId || 0}
            onChange={(e) => setSelectedPacienteId(Number(e.target.value))}
            className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-semibold"
          >
            {pacientesList.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre} ({p.especie} • {p.nombrePropietario || 'Sin tutor'})
              </option>
            ))}
          </select>
        </div>

        <div className="flex items-center space-x-2 self-end sm:self-center">
          <button
            onClick={() => {
              if (selectedPacienteId) loadPacienteDetalle(selectedPacienteId);
            }}
            title="Recargar expediente"
            className="p-2 text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100 rounded-xl transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
          </button>

          {pacienteDetalle && (
            <button
              onClick={() => setIsDrawerOpen(true)}
              className="flex items-center px-4 py-2 rounded-xl bg-botanical-forest hover:bg-botanical-forestDark text-white text-xs font-semibold shadow-sm transition-all"
            >
              <Plus className="w-4 h-4 mr-1.5" />
              <span>Nueva Consulta Médica</span>
            </button>
          )}
        </div>
      </div>

      {/* Patient 360° Header Summary Card */}
      {isLoadingDetalle ? (
        <div className="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-botanical-stone">
          <Loader2 className="w-8 h-8 animate-spin text-botanical-emerald mb-2" />
          <span className="text-xs text-botanical-muted">Cargando expediente 360°...</span>
        </div>
      ) : pacienteDetalle ? (
        <div className="bg-white rounded-2xl p-6 border border-botanical-stone shadow-card space-y-6">
          {/* Top Patient Bio Info */}
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-6 border-b border-botanical-stone/80">
            <div>
              <div className="flex items-center gap-2 flex-wrap mb-2">
                <EspecieBadge especie={pacienteDetalle.especie} />
                <SexoReproductivoBadge
                  sexo={pacienteDetalle.sexo}
                  estadoReproductivo={pacienteDetalle.estadoReproductivo}
                />
              </div>

              <h2 className="text-2xl font-bold text-botanical-graphite">
                {pacienteDetalle.nombre}
              </h2>
              <p className="text-xs text-botanical-muted font-medium mt-0.5">
                {pacienteDetalle.raza}{' '}
                {pacienteDetalle.colorSenas ? `• ${pacienteDetalle.colorSenas}` : ''}
              </p>
            </div>

            {/* Quick Stats Pill Boxes */}
            <div className="flex items-center gap-3">
              <div className="p-3 rounded-xl bg-botanical-linen border border-botanical-stone text-right">
                <div className="text-[10px] text-botanical-muted font-semibold uppercase flex items-center justify-end gap-1">
                  <Calendar className="w-3 h-3 text-botanical-emerald" />
                  <span>Edad Calculada</span>
                </div>
                <div className="text-sm font-bold text-botanical-graphite mt-0.5">
                  {pacienteDetalle.edadFormateada}
                </div>
              </div>

              <div className="p-3 rounded-xl bg-botanical-linen border border-botanical-stone text-right">
                <div className="text-[10px] text-botanical-muted font-semibold uppercase flex items-center justify-end gap-1">
                  <Scale className="w-3 h-3 text-botanical-emerald" />
                  <span>Peso Actual</span>
                </div>
                <div className="text-sm font-bold text-botanical-graphite font-mono mt-0.5">
                  {pacienteDetalle.pesoActualKg.toFixed(2)} Kg
                </div>
              </div>
            </div>
          </div>

          {/* Tutor & Contact Strip */}
          <div className="flex flex-wrap items-center justify-between gap-3 text-xs bg-stone-50 p-3 rounded-xl border border-stone-200/80">
            <div className="flex items-center gap-2">
              <User className="w-4 h-4 text-botanical-emerald" />
              <span className="text-botanical-muted">Tutor Responsable:</span>
              <strong className="text-botanical-graphite">
                {pacienteDetalle.propietario?.nombreCompleto || pacienteDetalle.nombrePropietario || 'No asignado'}
              </strong>
            </div>

            {pacienteDetalle.propietario?.telefono && (
              <div className="flex items-center gap-3">
                <div className="flex items-center gap-1 font-mono text-botanical-graphite">
                  <Phone className="w-3.5 h-3.5 text-stone-400" />
                  <span>{pacienteDetalle.propietario.telefono}</span>
                </div>

                <a
                  href={getWhatsAppLink(
                    pacienteDetalle.propietario.telefono,
                    pacienteDetalle.nombre,
                    pacienteDetalle.propietario.nombres
                  )}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-800 bg-emerald-100 hover:bg-emerald-200 px-2.5 py-1 rounded-lg transition-colors"
                >
                  <MessageCircle className="w-3.5 h-3.5 text-emerald-700" />
                  <span>WhatsApp</span>
                </a>
              </div>
            )}
          </div>

          {/* Sub-Tabs: Timeline vs Curva de Peso */}
          <div className="flex items-center space-x-2 border-b border-botanical-stone pb-2">
            <button
              onClick={() => setActiveSubTab('timeline')}
              className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                activeSubTab === 'timeline'
                  ? 'bg-botanical-forest text-white shadow-sm'
                  : 'text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100'
              }`}
            >
              <ClipboardList className="w-4 h-4" />
              <span>Historia Cronológica ({pacienteDetalle.atenciones?.length || 0})</span>
            </button>

            <button
              onClick={() => setActiveSubTab('curva')}
              className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all ${
                activeSubTab === 'curva'
                  ? 'bg-botanical-forest text-white shadow-sm'
                  : 'text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100'
              }`}
            >
              <TrendingUp className="w-4 h-4" />
              <span>Curva de Peso ({pacienteDetalle.curvaPeso?.length || 0} puntos)</span>
            </button>
          </div>

          {/* Sub-View Content */}
          {activeSubTab === 'timeline' ? (
            <TimelineAtenciones atenciones={pacienteDetalle.atenciones || []} />
          ) : (
            <CurvaPesoChart puntos={pacienteDetalle.curvaPeso || []} />
          )}
        </div>
      ) : (
        <div className="bg-white rounded-2xl p-12 border border-botanical-stone text-center">
          <p className="text-xs text-botanical-muted">Seleccione un paciente para consultar su historia clínica.</p>
        </div>
      )}

      {/* Drawer para registrar nueva atención */}
      {pacienteDetalle && (
        <NuevaAtencionDrawer
          isOpen={isDrawerOpen}
          onClose={() => setIsDrawerOpen(false)}
          onSuccess={handleConsultaCreada}
          paciente={pacienteDetalle}
        />
      )}
    </div>
  );
};
