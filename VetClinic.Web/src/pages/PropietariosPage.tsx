import React, { useState, useEffect } from 'react';
import type { Propietario } from '../types';
import { api, ApiError } from '../services/apiClient';
import { PropietarioModal } from '../components/propietarios/PropietarioModal';
import {
  Users,
  Search,
  Plus,
  Phone,
  Mail,
  MapPin,
  Edit2,
  ExternalLink,
  Dog,
  Loader2,
  MessageCircle,
  RefreshCw
} from 'lucide-react';
import { toast } from 'sonner';

interface PropietariosPageProps {
  onSelectPacienteTab?: (propietarioId?: number) => void;
}

export const PropietariosPage: React.FC<PropietariosPageProps> = ({ onSelectPacienteTab }) => {
  const [propietarios, setPropietarios] = useState<Propietario[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPropietario, setEditingPropietario] = useState<Propietario | null>(null);

  const fetchPropietarios = async (criterio = '') => {
    setIsLoading(true);
    try {
      const data = await api.propietarios.buscar(criterio);
      setPropietarios(data);
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al cargar propietarios.';
      toast.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPropietarios();
  }, []);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchPropietarios(searchTerm);
  };

  const handleOpenCreate = () => {
    setEditingPropietario(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (prop: Propietario) => {
    setEditingPropietario(prop);
    setIsModalOpen(true);
  };

  const handleModalSuccess = () => {
    fetchPropietarios(searchTerm);
  };

  // Enlace directo WhatsApp Web (RN-09, STF-04)
  const getWhatsAppLink = (telefono: string, nombre: string) => {
    const cleanPhone = telefono.replace(/\D/g, '');
    const mensaje = encodeURIComponent(
      `Hola ${nombre}, te saludamos de la Clínica Veterinaria de los Dres. Fabio y William.`
    );
    return `https://wa.me/57${cleanPhone}?text=${mensaje}`;
  };

  return (
    <div className="space-y-6">
      {/* Top Action Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm">
        {/* Search Input (RF-03) */}
        <form onSubmit={handleSearchSubmit} className="relative flex-1 max-w-md">
          <Search className="w-4 h-4 text-botanical-subtle absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Buscar por nombre, documento o teléfono..."
            className="w-full pl-10 pr-24 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite placeholder-botanical-subtle focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
          />
          <button
            type="submit"
            className="absolute right-1.5 top-1/2 -translate-y-1/2 px-3 py-1 bg-botanical-forest text-white rounded-lg text-xs font-semibold hover:bg-botanical-forestDark transition-colors"
          >
            Buscar
          </button>
        </form>

        <div className="flex items-center space-x-3">
          <button
            onClick={() => {
              setSearchTerm('');
              fetchPropietarios('');
            }}
            title="Recargar directorio"
            className="p-2 text-botanical-muted hover:text-botanical-graphite hover:bg-stone-100 rounded-xl transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
          </button>

          <button
            onClick={handleOpenCreate}
            className="flex items-center px-4 py-2 rounded-xl bg-botanical-emerald hover:bg-botanical-emeraldHover text-white text-xs font-semibold shadow-sm transition-all"
          >
            <Plus className="w-4 h-4 mr-1.5" />
            <span>Registrar Propietario</span>
          </button>
        </div>
      </div>

      {/* Directory Grid */}
      {isLoading ? (
        <div className="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-botanical-stone">
          <Loader2 className="w-8 h-8 animate-spin text-botanical-emerald mb-2" />
          <span className="text-xs text-botanical-muted font-medium">Cargando directorio de propietarios...</span>
        </div>
      ) : propietarios.length === 0 ? (
        <div className="flex flex-col items-center justify-center p-12 bg-white rounded-2xl border border-botanical-stone text-center">
          <div className="w-12 h-12 rounded-2xl bg-stone-100 text-stone-400 flex items-center justify-center mb-3">
            <Users className="w-6 h-6" />
          </div>
          <h4 className="text-sm font-bold text-botanical-graphite">No se encontraron propietarios</h4>
          <p className="text-xs text-botanical-muted mt-1 max-w-sm">
            {searchTerm
              ? `No hay coincidencias para "${searchTerm}". Intente con otro documento o nombre.`
              : 'Aún no hay tutores registrados en la base de datos local.'}
          </p>
          <button
            onClick={handleOpenCreate}
            className="mt-4 px-4 py-2 bg-botanical-emerald text-white rounded-xl text-xs font-semibold hover:bg-botanical-emeraldHover shadow-sm transition-all"
          >
            Registrar Primer Propietario
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {propietarios.map((prop) => (
            <div
              key={prop.id}
              className="bg-white rounded-2xl p-5 border border-botanical-stone shadow-card hover:shadow-card-hover transition-all flex flex-col justify-between"
            >
              <div>
                {/* Header card info */}
                <div className="flex items-start justify-between">
                  <div>
                    <span className="inline-block px-2 py-0.5 rounded text-[11px] font-mono bg-stone-100 text-stone-700 font-semibold mb-1">
                      {prop.tipoDocumento} {prop.numeroDocumento}
                    </span>
                    <h3 className="text-base font-bold text-botanical-graphite leading-tight">
                      {prop.nombreCompleto}
                    </h3>
                  </div>

                  <button
                    onClick={() => handleOpenEdit(prop)}
                    title="Editar información"
                    className="p-1.5 rounded-lg text-botanical-muted hover:text-botanical-forest hover:bg-emerald-50 transition-colors"
                  >
                    <Edit2 className="w-3.5 h-3.5" />
                  </button>
                </div>

                {/* Contact data */}
                <div className="mt-4 space-y-2 text-xs text-botanical-muted">
                  <div className="flex items-center gap-2">
                    <Phone className="w-3.5 h-3.5 text-botanical-emerald flex-shrink-0" />
                    <span className="font-mono text-botanical-graphite font-medium">{prop.telefono}</span>
                    <a
                      href={getWhatsAppLink(prop.telefono, prop.nombres)}
                      target="_blank"
                      rel="noopener noreferrer"
                      title="Abrir chat en WhatsApp Web (RN-09)"
                      className="inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 px-2 py-0.5 rounded-md transition-colors ml-auto"
                    >
                      <MessageCircle className="w-3 h-3 text-emerald-600" />
                      <span>WhatsApp</span>
                    </a>
                  </div>

                  {prop.email && (
                    <div className="flex items-center gap-2">
                      <Mail className="w-3.5 h-3.5 text-botanical-subtle flex-shrink-0" />
                      <a
                        href={`mailto:${prop.email}`}
                        className="truncate hover:text-botanical-forest hover:underline"
                        title={prop.email}
                      >
                        {prop.email}
                      </a>
                    </div>
                  )}

                  {prop.direccion && (
                    <div className="flex items-center gap-2">
                      <MapPin className="w-3.5 h-3.5 text-botanical-subtle flex-shrink-0" />
                      <span className="truncate">{prop.direccion}</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Card Footer: Mascotas Count */}
              <div className="mt-5 pt-3 border-t border-botanical-stone flex items-center justify-between">
                <div className="flex items-center gap-1.5 text-xs text-botanical-graphite font-medium">
                  <Dog className="w-4 h-4 text-botanical-emerald" />
                  <span>
                    {prop.cantidadPacientes === 1
                      ? '1 Mascota'
                      : `${prop.cantidadPacientes} Mascotas`}
                  </span>
                </div>

                {onSelectPacienteTab && (
                  <button
                    onClick={() => onSelectPacienteTab(prop.id)}
                    className="inline-flex items-center text-xs font-semibold text-botanical-forest hover:text-botanical-forestDark group"
                  >
                    <span>Ver Pacientes</span>
                    <ExternalLink className="w-3 h-3 ml-1 group-hover:translate-x-0.5 transition-transform" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal Alta / Edicion */}
      <PropietarioModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={handleModalSuccess}
        propietarioToEdit={editingPropietario}
      />
    </div>
  );
};
