import React, { useState, useEffect } from 'react';
import type { Propietario, CrearPropietarioDto, TipoDocumento } from '../../types';
import { api, ApiError } from '../../services/apiClient';
import { X, User, Phone, Mail, MapPin, CreditCard, Loader2, Check } from 'lucide-react';
import { toast } from 'sonner';

interface PropietarioModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (propietario: Propietario) => void;
  propietarioToEdit?: Propietario | null;
}

export const PropietarioModal: React.FC<PropietarioModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  propietarioToEdit,
}) => {
  const [tipoDocumento, setTipoDocumento] = useState<TipoDocumento>('CC');
  const [numeroDocumento, setNumeroDocumento] = useState('');
  const [nombres, setNombres] = useState('');
  const [apellidos, setApellidos] = useState('');
  const [telefono, setTelefono] = useState('');
  const [email, setEmail] = useState('');
  const [direccion, setDireccion] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const isEditing = !!propietarioToEdit;

  useEffect(() => {
    if (propietarioToEdit) {
      setTipoDocumento(propietarioToEdit.tipoDocumento);
      setNumeroDocumento(propietarioToEdit.numeroDocumento);
      setNombres(propietarioToEdit.nombres);
      setApellidos(propietarioToEdit.apellidos);
      setTelefono(propietarioToEdit.telefono);
      setEmail(propietarioToEdit.email || '');
      setDireccion(propietarioToEdit.direccion || '');
    } else {
      setTipoDocumento('CC');
      setNumeroDocumento('');
      setNombres('');
      setApellidos('');
      setTelefono('');
      setEmail('');
      setDireccion('');
    }
    setErrorMsg(null);
  }, [propietarioToEdit, isOpen]);

  if (!isOpen) return null;

  const sanitizePhone = (val: string) => val.replace(/[\s\-\(\)\.]+/g, '');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    // Validación de celular Colombia: 10 dígitos iniciando en 3 (RN-04)
    const phoneClean = sanitizePhone(telefono);
    if (!/^3\d{9}$/.test(phoneClean)) {
      setErrorMsg('El número de celular debe contener exactamente 10 dígitos iniciando en 3 (ej. 3001234567).');
      return;
    }

    if (!numeroDocumento.trim() || !nombres.trim() || !apellidos.trim()) {
      setErrorMsg('Por favor complete los campos obligatorios (Documento, Nombres y Apellidos).');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: CrearPropietarioDto = {
        tipoDocumento,
        numeroDocumento: numeroDocumento.trim(),
        nombres: nombres.trim(),
        apellidos: apellidos.trim(),
        telefono: phoneClean,
        email: email.trim() ? email.trim() : null,
        direccion: direccion.trim() ? direccion.trim() : null,
      };

      let result: Propietario;
      if (isEditing && propietarioToEdit) {
        result = await api.propietarios.actualizar(propietarioToEdit.id, payload);
        toast.success('Propietario actualizado correctamente.');
      } else {
        result = await api.propietarios.crear(payload);
        toast.success('Propietario registrado con éxito.');
      }

      onSuccess(result);
      onClose();
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al guardar propietario.';
      setErrorMsg(msg);
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fadeIn">
      <div className="bg-white rounded-2xl shadow-2xl border border-botanical-stone max-w-lg w-full overflow-hidden flex flex-col max-h-[90vh]">
        {/* Modal Header */}
        <div className="px-6 py-4 border-b border-botanical-stone flex items-center justify-between bg-botanical-linen/50">
          <div>
            <h3 className="text-base font-bold text-botanical-graphite">
              {isEditing ? 'Editar Propietario / Tutor' : 'Registrar Nuevo Propietario'}
            </h3>
            <p className="text-xs text-botanical-muted mt-0.5">
              Directorio de tutores con validación de celular Colombia (RN-04)
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-1 rounded-lg text-botanical-muted hover:text-botanical-graphite hover:bg-stone-200 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Modal Body */}
        <form onSubmit={handleSubmit} className="p-6 overflow-y-auto space-y-4">
          {errorMsg && (
            <div className="p-3 rounded-xl bg-red-50 border border-red-200 text-red-800 text-xs font-medium">
              {errorMsg}
            </div>
          )}

          {/* Documento */}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Tipo Doc.
              </label>
              <select
                value={tipoDocumento}
                onChange={(e) => setTipoDocumento(e.target.value as TipoDocumento)}
                className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              >
                <option value="CC">CC (Cédula)</option>
                <option value="TI">TI (Identidad)</option>
                <option value="CE">CE (Extranjería)</option>
                <option value="Pasaporte">Pasaporte</option>
              </select>
            </div>
            <div className="col-span-2">
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Número de Documento *
              </label>
              <div className="relative">
                <CreditCard className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  required
                  value={numeroDocumento}
                  onChange={(e) => setNumeroDocumento(e.target.value)}
                  placeholder="Ej. 1020304050"
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>
          </div>

          {/* Nombres y Apellidos */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Nombres *
              </label>
              <div className="relative">
                <User className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  required
                  value={nombres}
                  onChange={(e) => setNombres(e.target.value)}
                  placeholder="Ej. Maria Elena"
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Apellidos *
              </label>
              <input
                type="text"
                required
                value={apellidos}
                onChange={(e) => setApellidos(e.target.value)}
                placeholder="Ej. Rodriguez"
                className="w-full px-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
              />
            </div>
          </div>

          {/* Celular / WhatsApp */}
          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="text-xs font-semibold text-botanical-graphite uppercase">
                Celular / WhatsApp (Colombia) *
              </label>
              <span className="text-[10px] text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded font-medium">
                10 dígitos (inicia en 3)
              </span>
            </div>
            <div className="relative">
              <Phone className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="tel"
                required
                maxLength={14}
                value={telefono}
                onChange={(e) => setTelefono(e.target.value)}
                placeholder="300 123 4567"
                className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald font-mono"
              />
            </div>
          </div>

          {/* Correo y Dirección */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Correo Electrónico
              </label>
              <div className="relative">
                <Mail className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="ejemplo@correo.com"
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-botanical-graphite uppercase mb-1">
                Dirección
              </label>
              <div className="relative">
                <MapPin className="w-4 h-4 text-botanical-subtle absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  value={direccion}
                  onChange={(e) => setDireccion(e.target.value)}
                  placeholder="Calle 12 # 34-56"
                  className="w-full pl-9 pr-3 py-2 text-sm bg-botanical-linen/60 border border-botanical-stone rounded-xl text-botanical-graphite focus:outline-none focus:ring-2 focus:ring-botanical-emerald/30 focus:border-botanical-emerald"
                />
              </div>
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
                  <span>{isEditing ? 'Actualizar Propietario' : 'Guardar Propietario'}</span>
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
