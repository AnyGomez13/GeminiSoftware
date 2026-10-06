import React, { useState, useEffect } from 'react';
import type { Paciente, PacienteDetalle } from '../types';
import { api, ApiError } from '../services/apiClient';
import { CarnetPdfViewer } from '../components/vacunacion/CarnetPdfViewer';
import { NuevaInmunizacionModal } from '../components/vacunacion/NuevaInmunizacionModal';
import {
  Plus,
  Loader2,
  RefreshCw,
  Dog
} from 'lucide-react';
import { toast } from 'sonner';

interface VacunacionPageProps {
  initialPacienteId?: number | null;
}

export const VacunacionPage: React.FC<VacunacionPageProps> = ({ initialPacienteId }) => {
  const [pacientesList, setPacientesList] = useState<Paciente[]>([]);
  const [selectedPacienteId, setSelectedPacienteId] = useState<number | null>(
    initialPacienteId || null
  );
  const [pacienteDetalle, setPacienteDetalle] = useState<PacienteDetalle | null>(null);
  const [isLoadingList, setIsLoadingList] = useState(true);
  const [isLoadingDetalle, setIsLoadingDetalle] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);

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

  // Cargar paciente completo para carnet
  const loadPacienteDetalle = async (id: number) => {
    setIsLoadingDetalle(true);
    try {
      const detalle = await api.pacientes.getById(id);
      setPacienteDetalle(detalle);
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al cargar carnet.';
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

  const handleInmunizacionCreada = () => {
    if (selectedPacienteId) {
      loadPacienteDetalle(selectedPacienteId);
    }
  };

  return (
    <div className="space-y-6">
      {/* Patient Selection Bar */}
      <div className="no-print bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1 max-w-md">
          <label className="text-xs font-bold text-botanical-graphite uppercase whitespace-nowrap">
            Carnet de:
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
            title="Recargar carnet"
            className="p-2 text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100 rounded-xl transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
          </button>

          {pacienteDetalle && (
            <button
              onClick={() => setIsModalOpen(true)}
              className="flex items-center px-4 py-2 rounded-xl bg-botanical-emerald hover:bg-botanical-emeraldHover text-white text-xs font-semibold shadow-sm transition-all"
            >
              <Plus className="w-4 h-4 mr-1.5" />
              <span>Aplicar Biológico / Vacuna</span>
            </button>
          )}
        </div>
      </div>

      {/* Main Carnet Document Area */}
      {isLoadingDetalle ? (
        <div className="flex flex-col items-center justify-center p-16 bg-white rounded-2xl border border-botanical-stone">
          <Loader2 className="w-8 h-8 animate-spin text-botanical-emerald mb-2" />
          <span className="text-xs text-botanical-muted">Generando pasaporte médico...</span>
        </div>
      ) : pacienteDetalle ? (
        <CarnetPdfViewer paciente={pacienteDetalle} onRefresh={handleInmunizacionCreada} />
      ) : (
        <div className="bg-white rounded-2xl p-12 border border-botanical-stone text-center">
          <Dog className="w-8 h-8 text-botanical-subtle mx-auto mb-2" />
          <p className="text-xs text-botanical-muted">Seleccione una mascota para visualizar su carnet digital.</p>
        </div>
      )}

      {/* Modal Nueva Inmunizacion */}
      {pacienteDetalle && (
        <NuevaInmunizacionModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          onSuccess={handleInmunizacionCreada}
          paciente={pacienteDetalle}
        />
      )}
    </div>
  );
};
