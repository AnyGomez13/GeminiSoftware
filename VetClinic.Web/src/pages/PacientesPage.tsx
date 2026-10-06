import React, { useState, useEffect } from 'react';
import type { Paciente, Especie } from '../types';
import { api, ApiError } from '../services/apiClient';
import { PacienteModal } from '../components/pacientes/PacienteModal';
import { EspecieBadge, SexoReproductivoBadge } from '../components/pacientes/AlertasBadge';
import {
  Dog,
  Search,
  Plus,
  Scale,
  Calendar,
  User,
  Phone,
  ClipboardList,
  Syringe,
  Edit2,
  RefreshCw,
  Loader2,
  MessageCircle
} from 'lucide-react';
import { toast } from 'sonner';

interface PacientesPageProps {
  onSelectHistoriaTab?: (pacienteId: number) => void;
  onSelectVacunacionTab?: (pacienteId: number) => void;
  filterPropietarioId?: number | null;
}

export const PacientesPage: React.FC<PacientesPageProps> = ({
  onSelectHistoriaTab,
  onSelectVacunacionTab,
  filterPropietarioId,
}) => {
  const [pacientes, setPacientes] = useState<Paciente[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [especieFilter, setEspecieFilter] = useState<'Todos' | Especie>('Todos');
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPaciente, setEditingPaciente] = useState<Paciente | null>(null);

  const fetchPacientes = async (criterio = '') => {
    setIsLoading(true);
    try {
      const data = await api.pacientes.buscar(criterio);
      setPacientes(data);
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al cargar pacientes.';
      toast.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPacientes();
  }, []);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchPacientes(searchTerm);
  };

  const handleOpenCreate = () => {
    setEditingPaciente(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (p: Paciente) => {
    setEditingPaciente(p);
    setIsModalOpen(true);
  };

  const handleModalSuccess = () => {
    fetchPacientes(searchTerm);
  };

  // Filtrado local por especie o propietario
  const pacientesFiltrados = pacientes.filter((p) => {
    if (filterPropietarioId && p.propietarioId !== filterPropietarioId) {
      return false;
    }
    if (especieFilter !== 'Todos' && p.especie !== especieFilter) {
      return false;
    }
    return true;
  });

  const getWhatsAppLink = (telefono: string, pacienteNombre: string, tutorNombre: string) => {
    const cleanPhone = telefono.replace(/\D/g, '');
    const mensaje = encodeURIComponent(
      `Hola ${tutorNombre}, te escribimos de la Clínica Veterinaria de los Dres. Fabio y William respecto a tu mascota ${pacienteNombre}.`
    );
    return `https://wa.me/57${cleanPhone}?text=${mensaje}`;
  };

  return (
    <div className="space-y-6">
      {/* Top Filter and Search Bar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm">
        {/* Search */}
        <form onSubmit={handleSearchSubmit} className="relative flex-1 max-w-md">
          <Search className="w-4 h-4 text-botanical-subtle absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Buscar por mascota, tutor, raza o documento..."
            className="w-full pl-10 pr-24 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite placeholder-botanical-subtle focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
          />
          <button
            type="submit"
            className="absolute right-1.5 top-1/2 -translate-y-1/2 px-3 py-1 bg-botanical-forest text-white rounded-lg text-xs font-semibold hover:bg-botanical-forestDark transition-colors"
          >
            Buscar
          </button>
        </form>

        {/* Especie Filter Pills */}
        <div className="flex items-center space-x-1.5">
          {(['Todos', 'Canino', 'Felino'] as const).map((esp) => (
            <button
              key={esp}
              onClick={() => setEspecieFilter(esp)}
              className={`px-3 py-1.5 rounded-xl text-xs font-semibold transition-all ${
                especieFilter === esp
                  ? 'bg-botanical-forest text-white shadow-sm'
                  : 'bg-stone-100 text-stone-600 hover:bg-stone-200'
              }`}
            >
              {esp === 'Todos' ? 'Todos' : esp === 'Canino' ? 'Caninos' : 'Felinos'}
            </button>
          ))}

          <button
            onClick={() => {
              setSearchTerm('');
              setEspecieFilter('Todos');
              fetchPacientes('');
            }}
            title="Recargar censo"
            className="p-2 text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100 rounded-xl transition-colors ml-1"
          >
            <RefreshCw className="w-4 h-4" />
          </button>

          <button
            onClick={handleOpenCreate}
            className="flex items-center px-4 py-2 rounded-xl bg-botanical-emerald hover:bg-botanical-emeraldHover text-white text-xs font-semibold shadow-sm transition-all ml-2"
          >
            <Plus className="w-4 h-4 mr-1.5" />
            <span>Registrar Paciente</span>
          </button>
        </div>
      </div>

      {/* Patients Grid */}
      {isLoading ? (
        <div className="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-botanical-stone">
          <Loader2 className="w-8 h-8 animate-spin text-botanical-emerald mb-2" />
          <span className="text-xs text-botanical-muted font-medium">Cargando censo de pacientes...</span>
        </div>
      ) : pacientesFiltrados.length === 0 ? (
        <div className="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-botanical-stone text-center">
          <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-botanical-emerald flex items-center justify-center mb-3">
            <Dog className="w-6 h-6" />
          </div>
          <h4 className="text-sm font-bold text-botanical-graphite">No se encontraron mascotas registradas</h4>
          <p className="text-xs text-botanical-muted mt-1 max-w-sm">
            {searchTerm
              ? `No hay coincidencias para "${searchTerm}". Intente con otro término.`
              : 'El censo médico no cuenta con pacientes con el filtro seleccionado.'}
          </p>
          <button
            onClick={handleOpenCreate}
            className="mt-4 px-4 py-2 bg-botanical-emerald text-white rounded-xl text-xs font-semibold hover:bg-botanical-emeraldHover shadow-sm transition-all"
          >
            Registrar Primera Mascota
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {pacientesFiltrados.map((paciente) => (
            <div
              key={paciente.id}
              className="bg-white rounded-2xl p-5 border border-botanical-stone shadow-card hover:shadow-card-hover transition-all flex flex-col justify-between"
            >
              <div>
                {/* Header card with badges */}
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-2 flex-wrap">
                    <EspecieBadge especie={paciente.especie} />
                    <SexoReproductivoBadge
                      sexo={paciente.sexo}
                      estadoReproductivo={paciente.estadoReproductivo}
                    />
                  </div>

                  <button
                    onClick={() => handleOpenEdit(paciente)}
                    title="Editar ficha"
                    className="p-1.5 rounded-lg text-botanical-muted hover:text-botanical-forest hover:bg-emerald-50 transition-colors"
                  >
                    <Edit2 className="w-3.5 h-3.5" />
                  </button>
                </div>

                {/* Name & Breed */}
                <div className="mt-3">
                  <h3 className="text-lg font-bold text-botanical-graphite leading-snug">
                    {paciente.nombre}
                  </h3>
                  <p className="text-xs text-botanical-muted font-medium">
                    {paciente.raza} {paciente.colorSenas ? `• ${paciente.colorSenas}` : ''}
                  </p>
                </div>

                {/* Age & Weight Highlights */}
                <div className="mt-4 grid grid-cols-2 gap-2 bg-botanical-linen/60 p-3 rounded-xl border border-botanical-stone/70">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-botanical-emerald flex-shrink-0" />
                    <div>
                      <div className="text-[10px] text-botanical-subtle font-semibold uppercase">
                        Edad
                      </div>
                      <div className="text-xs font-bold text-botanical-graphite truncate">
                        {paciente.edadFormateada}
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    <Scale className="w-4 h-4 text-botanical-emerald flex-shrink-0" />
                    <div>
                      <div className="text-[10px] text-botanical-subtle font-semibold uppercase">
                        Peso Actual
                      </div>
                      <div className="text-xs font-bold text-botanical-graphite">
                        {paciente.pesoActualKg.toFixed(2)} Kg
                      </div>
                    </div>
                  </div>
                </div>

                {/* Tutor Info */}
                <div className="mt-3 p-2.5 rounded-xl bg-stone-50 border border-stone-200/80 text-xs text-botanical-muted space-y-1">
                  <div className="flex items-center justify-between">
                    <span className="font-semibold text-botanical-graphite flex items-center gap-1.5">
                      <User className="w-3.5 h-3.5 text-botanical-emerald" />
                      <span>{paciente.nombrePropietario || 'Tutor no asignado'}</span>
                    </span>
                    {paciente.telefonoPropietario && (
                      <a
                        href={getWhatsAppLink(
                          paciente.telefonoPropietario,
                          paciente.nombre,
                          paciente.nombrePropietario || 'Tutor'
                        )}
                        target="_blank"
                        rel="noopener noreferrer"
                        title="Contactar acudiente vía WhatsApp Web (RN-09)"
                        className="text-emerald-700 hover:text-emerald-800 p-1 rounded hover:bg-emerald-100 transition-colors"
                      >
                        <MessageCircle className="w-3.5 h-3.5" />
                      </a>
                    )}
                  </div>
                  {paciente.telefonoPropietario && (
                    <div className="flex items-center gap-1 text-[11px] font-mono text-botanical-muted pl-5">
                      <Phone className="w-3 h-3 text-stone-400" />
                      <span>{paciente.telefonoPropietario}</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Action Buttons for 360° View */}
              <div className="mt-5 pt-3 border-t border-botanical-stone grid grid-cols-2 gap-2">
                <button
                  onClick={() => onSelectHistoriaTab?.(paciente.id)}
                  className="flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl bg-botanical-forest/10 hover:bg-botanical-forest/20 text-botanical-forest text-xs font-semibold transition-colors"
                >
                  <ClipboardList className="w-3.5 h-3.5" />
                  <span>Historia 360°</span>
                </button>

                <button
                  onClick={() => onSelectVacunacionTab?.(paciente.id)}
                  className="flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl bg-emerald-50 hover:bg-emerald-100 text-botanical-emerald text-xs font-semibold transition-colors"
                >
                  <Syringe className="w-3.5 h-3.5" />
                  <span>Carnet PDF</span>
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal Alta / Edicion */}
      <PacienteModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={handleModalSuccess}
        pacienteToEdit={editingPaciente}
        initialPropietarioId={filterPropietarioId || undefined}
      />
    </div>
  );
};
