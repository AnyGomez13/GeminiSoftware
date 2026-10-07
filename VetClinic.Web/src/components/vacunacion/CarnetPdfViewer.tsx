import React, { useState } from 'react';
import type { PacienteDetalle } from '../../types';
import { api, ApiError } from '../../services/apiClient';
import {
  Download,
  Printer,
  ShieldCheck,
  Stethoscope,
  Calendar,
  Scale,
  User,
  Phone,
  FileCheck2,
  Loader2,
  Syringe,
  CheckCircle2,
  AlertTriangle,
  Clock
} from 'lucide-react';
import { toast } from 'sonner';

interface CarnetPdfViewerProps {
  paciente: PacienteDetalle;
  onRefresh?: () => void;
}

export const CarnetPdfViewer: React.FC<CarnetPdfViewerProps> = ({ paciente }) => {
  const [isDownloading, setIsDownloading] = useState(false);

  // Descarga del PDF nativo QuestPDF desde el backend (RF-09, RNF-06)
  const handleDownloadPdf = async () => {
    setIsDownloading(true);
    const toastId = toast.loading('Generando carnet digital en alta resolución...');
    try {
      const blob = await api.inmunizaciones.descargarCarnetBlob(paciente.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Carnet_Vacunacion_${paciente.nombre.replace(/\s+/g, '_')}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);

      toast.success('Carnet descargado exitosamente.', { id: toastId });
    } catch (err: any) {
      const msg = err instanceof ApiError ? err.message : 'Error al generar el carnet PDF.';
      toast.error(msg, { id: toastId });
    } finally {
      setIsDownloading(false);
    }
  };

  // Impresión directa del navegador usando @media print
  const handleDirectPrint = () => {
    window.print();
  };

  const listaInmunizaciones = [...(paciente.inmunizaciones || [])].sort(
    (a, b) => new Date(b.fechaAplicacion).getTime() - new Date(a.fechaAplicacion).getTime()
  );

  return (
    <div className="space-y-6">
      {/* Top Action Toolbar (Oculto al imprimir) */}
      <div className="no-print bg-white p-4 rounded-2xl border border-botanical-stone shadow-sm flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h4 className="text-sm font-bold text-botanical-graphite flex items-center gap-2">
            <ShieldCheck className="w-4 h-4 text-botanical-emerald" />
            <span>Pasaporte y Carnet Digital de Vacunación</span>
          </h4>
          <p className="text-xs text-botanical-muted mt-0.5">
            Vista interactiva web optimizada para pantalla y formato de impresión legal A4 / Carta
          </p>
        </div>

        <div className="flex items-center space-x-3">
          <button
            onClick={handleDirectPrint}
            className="flex items-center px-4 py-2 rounded-xl bg-botanical-linen hover:bg-stone-200 text-botanical-graphite text-xs font-semibold border border-botanical-stone transition-all"
          >
            <Printer className="w-4 h-4 mr-1.5 text-botanical-forest" />
            <span>Imprimir Carnet</span>
          </button>

          <button
            onClick={handleDownloadPdf}
            disabled={isDownloading}
            className="flex items-center px-4 py-2 rounded-xl bg-botanical-forest hover:bg-botanical-forestDark text-white text-xs font-semibold shadow-sm transition-all disabled:opacity-60"
          >
            {isDownloading ? (
              <>
                <Loader2 className="w-4 h-4 mr-1.5 animate-spin" />
                <span>Generando PDF...</span>
              </>
            ) : (
              <>
                <Download className="w-4 h-4 mr-1.5 text-emerald-300" />
                <span>Descargar PDF Oficial</span>
              </>
            )}
          </button>
        </div>
      </div>

      {/* Modern Botanical Medical Passport (Imprimible) */}
      <div className="carnet-print-area bg-white rounded-3xl p-8 sm:p-10 border-2 border-botanical-forest/20 shadow-xl max-w-4xl mx-auto relative overflow-hidden">
        {/* Subtle Decorative Background Watermark */}
        <div className="absolute top-0 right-0 w-80 h-80 bg-gradient-to-br from-botanical-forest/5 to-transparent rounded-bl-full pointer-events-none" />

        {/* Passport Clinic Header */}
        <div className="border-b-2 border-botanical-forest pb-6 flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-4 text-center sm:text-left">
            <div className="w-16 h-16 rounded-2xl bg-botanical-forest text-white flex items-center justify-center shadow-md flex-shrink-0">
              <Stethoscope className="w-8 h-8 text-emerald-300" />
            </div>
            <div>
              <div className="text-[10px] tracking-widest font-black uppercase text-botanical-forest">
                República de Colombia • Medicina Veterinaria
              </div>
              <h1 className="text-xl sm:text-2xl font-black text-botanical-graphite tracking-tight">
                VETCLINIC PRO — CARNET DE VACUNACIÓN
              </h1>
              <p className="text-xs text-botanical-muted font-medium mt-0.5">
                Clínica Veterinaria Dres. Fabio & William • Trazabilidad y Seguridad Legal
              </p>
            </div>
          </div>

          <div className="text-center sm:text-right border sm:border-0 p-2 rounded-xl bg-stone-50 sm:bg-transparent">
            <div className="text-[10px] font-semibold text-botanical-subtle uppercase">
              Expediente Digital
            </div>
            <div className="text-xs font-mono font-bold text-botanical-graphite">
              FOLIO-PAC-{paciente.id.toString().padStart(5, '0')}
            </div>
            <div className="text-[10px] text-emerald-800 font-semibold bg-emerald-50 px-2 py-0.5 rounded-full inline-block mt-1">
              Ley 576 de 2000
            </div>
          </div>
        </div>

        {/* Pet & Owner Identity Grid */}
        <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4 bg-botanical-linen/60 rounded-2xl p-5 border border-botanical-stone">
          {/* Pet Information */}
          <div className="space-y-1.5 text-xs">
            <div className="text-[10px] uppercase font-bold text-botanical-forest tracking-wider mb-1 flex items-center gap-1">
              <Syringe className="w-3.5 h-3.5" />
              <span>Datos del Paciente</span>
            </div>
            <div className="text-lg font-black text-botanical-graphite">
              {paciente.nombre}
            </div>
            <div className="text-botanical-muted font-medium">
              <strong className="text-botanical-graphite">Especie:</strong> {paciente.especie} •{' '}
              <strong className="text-botanical-graphite">Raza:</strong> {paciente.raza}
            </div>
            <div className="text-botanical-muted font-medium">
              <strong className="text-botanical-graphite">Sexo:</strong> {paciente.sexo} (
              {paciente.estadoReproductivo === 'CastradoEsterilizado'
                ? 'Esterilizado/a'
                : 'Entero/a'}
              )
            </div>
            <div className="flex items-center gap-3 pt-1">
              <span className="inline-flex items-center gap-1 bg-white px-2.5 py-1 rounded-lg border border-botanical-stone font-semibold text-botanical-graphite">
                <Calendar className="w-3.5 h-3.5 text-botanical-emerald" />
                <span>{paciente.edadFormateada}</span>
              </span>
              <span className="inline-flex items-center gap-1 bg-white px-2.5 py-1 rounded-lg border border-botanical-stone font-semibold text-botanical-graphite font-mono">
                <Scale className="w-3.5 h-3.5 text-botanical-emerald" />
                <span>{paciente.pesoActualKg.toFixed(2)} Kg</span>
              </span>
            </div>
          </div>

          {/* Owner Information */}
          <div className="space-y-1.5 text-xs border-t md:border-t-0 md:border-l border-botanical-stone/80 pt-4 md:pt-0 md:pl-5">
            <div className="text-[10px] uppercase font-bold text-botanical-forest tracking-wider mb-1 flex items-center gap-1">
              <User className="w-3.5 h-3.5" />
              <span>Tutor / Acudiente Responsable</span>
            </div>
            <div className="text-base font-bold text-botanical-graphite">
              {paciente.propietario?.nombreCompleto || paciente.nombrePropietario || 'Tutor no asignado'}
            </div>
            <div className="text-botanical-muted font-medium">
              <strong className="text-botanical-graphite">Documento:</strong>{' '}
              {paciente.propietario
                ? `${paciente.propietario.tipoDocumento} ${paciente.propietario.numeroDocumento}`
                : 'No registrado'}
            </div>
            <div className="text-botanical-muted font-medium flex items-center gap-1.5">
              <Phone className="w-3.5 h-3.5 text-botanical-emerald" />
              <strong className="text-botanical-graphite">Teléfono:</strong>{' '}
              <span className="font-mono">
                {paciente.propietario?.telefono || paciente.telefonoPropietario || 'Sin celular'}
              </span>
            </div>
            {paciente.propietario?.direccion && (
              <div className="text-botanical-muted text-[11px] truncate">
                <strong className="text-botanical-graphite">Dirección:</strong>{' '}
                {paciente.propietario.direccion}
              </div>
            )}
          </div>
        </div>

        {/* Immunization & Deworming Record Table */}
        <div className="mt-8">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-xs font-bold uppercase tracking-wider text-botanical-forest flex items-center gap-1.5">
              <FileCheck2 className="w-4 h-4 text-botanical-emerald" />
              <span>Historial Oficial de Inmunizaciones y Desparasitaciones</span>
            </h3>
            <span className="text-[11px] font-mono font-bold text-botanical-muted">
              {listaInmunizaciones.length} Registros
            </span>
          </div>

          <div className="border border-botanical-stone rounded-2xl overflow-hidden">
            <table className="w-full text-left text-xs">
              <thead className="bg-botanical-forest text-white uppercase text-[10px] tracking-wider">
                <tr>
                  <th className="py-3 px-3.5 font-bold">Fecha Aplicación</th>
                  <th className="py-3 px-3 font-bold">Tipo</th>
                  <th className="py-3 px-3.5 font-bold">Biológico / Fármaco</th>
                  <th className="py-3 px-3 font-bold">Lote</th>
                  <th className="py-3 px-3.5 font-bold">Próximo Refuerzo</th>
                  <th className="py-3 px-3.5 font-bold">Médico Responsable</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-botanical-stone">
                {listaInmunizaciones.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="py-8 text-center text-xs text-botanical-muted">
                      No se registran eventos de inmunización o desparasitación para este paciente.
                    </td>
                  </tr>
                ) : (
                  listaInmunizaciones.map((inm) => {
                    const fAplicacion = new Date(inm.fechaAplicacion).toLocaleDateString('es-CO');
                    const fRefuerzo = new Date(inm.fechaRefuerzo).toLocaleDateString('es-CO');
                    const isVencido = inm.estadoRefuerzo === 'Vencido';
                    const isProximo = inm.estadoRefuerzo === 'Proximo';

                    return (
                      <tr key={inm.id} className="hover:bg-botanical-linen/40 transition-colors">
                        <td className="py-3 px-3.5 font-mono text-botanical-graphite font-semibold whitespace-nowrap">
                          {fAplicacion}
                        </td>
                        <td className="py-3 px-3 text-botanical-muted">
                          {inm.tipoBiologico === 'Vacuna' ? 'Vacuna' : 'Desparasitante'}
                        </td>
                        <td className="py-3 px-3.5 font-bold text-botanical-graphite">
                          {inm.nombreProducto}
                        </td>
                        <td className="py-3 px-3 font-mono text-[11px] text-botanical-muted">
                          {inm.loteFabricante || '—'}
                        </td>
                        <td className="py-3 px-3.5 whitespace-nowrap">
                          <span
                            className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-lg text-[11px] font-bold ${
                              isVencido
                                ? 'bg-red-50 text-red-800 border border-red-200'
                                : isProximo
                                ? 'bg-amber-50 text-amber-800 border border-amber-200'
                                : 'bg-emerald-50 text-emerald-800 border border-emerald-200'
                            }`}
                          >
                            {isVencido ? (
                              <AlertTriangle className="w-3 h-3 text-red-600" />
                            ) : isProximo ? (
                              <Clock className="w-3 h-3 text-amber-600" />
                            ) : (
                              <CheckCircle2 className="w-3 h-3 text-emerald-600" />
                            )}
                            <span>{fRefuerzo}</span>
                          </span>
                        </td>
                        <td className="py-3 px-3.5 text-botanical-graphite font-medium whitespace-nowrap">
                          {inm.nombreVeterinario || 'Dr. Fabio / Dr. William'}
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Passport Footer & Medical Signatures */}
        <div className="mt-10 pt-6 border-t-2 border-botanical-forest/40 flex flex-col sm:flex-row items-center justify-between gap-6 text-xs text-botanical-muted">
          <div className="space-y-1 text-center sm:text-left">
            <div className="font-bold text-botanical-forest flex items-center justify-center sm:justify-start gap-1">
              <ShieldCheck className="w-4 h-4 text-botanical-emerald" />
              <span>Certificación Médico-Veterinaria Oficial</span>
            </div>
            <p className="text-[11px] leading-relaxed max-w-sm">
              Documento emitido conforme al Código de Ética Profesional del Médico Veterinario en Colombia. Registros inmutables con trazabilidad médica nominal.
            </p>
          </div>

          {/* Sello y Firma Box */}
          <div className="border border-botanical-stone rounded-2xl p-4 bg-botanical-linen/30 text-center w-56">
            <div className="h-10 border-b border-dashed border-stone-300 flex items-center justify-center">
              <span className="text-[10px] text-stone-400 italic font-serif">Firma y Sello Oficial</span>
            </div>
            <div className="text-[11px] font-bold text-botanical-graphite mt-1.5">
              Dres. Fabio & William
            </div>
            <div className="text-[9px] text-botanical-subtle">
              Médicos Veterinarios Tratantes
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
