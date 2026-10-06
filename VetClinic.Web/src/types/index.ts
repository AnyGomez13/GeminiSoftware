// Tipos y Enums de Dominio (VetClinic Pro Web)

export type TipoDocumento = 'CC' | 'TI' | 'CE' | 'Pasaporte';
export type Especie = 'Canino' | 'Felino' | 'Otro';
export type Sexo = 'Macho' | 'Hembra';
export type EstadoReproductivo = 'Entero' | 'CastradoEsterilizado';
export type TipoBiologico = 'Vacuna' | 'DesparasitacionInterna' | 'DesparasitacionExterna';

export interface Usuario {
  id: number;
  username: string;
  nombreCompleto: string;
  rol: string;
  isActive: boolean;
}

export interface LoginResponse {
  success: boolean;
  mensaje: string;
  usuario?: Usuario | null;
  token?: string | null;
}

export interface Veterinario {
  id: number;
  nombre: string;
  tarjetaProfesional: string;
  isActive: boolean;
}

export interface Propietario {
  id: number;
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  telefono: string;
  email?: string | null;
  direccion?: string | null;
  cantidadPacientes: number;
  createdAt: string;
  updatedAt: string;
}

export interface CrearPropietarioDto {
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  telefono: string;
  email?: string | null;
  direccion?: string | null;
}

export interface ActualizarPropietarioDto {
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  telefono: string;
  email?: string | null;
  direccion?: string | null;
}

export interface Paciente {
  id: number;
  propietarioId: number;
  nombrePropietario?: string | null;
  telefonoPropietario?: string | null;
  emailPropietario?: string | null;
  nombre: string;
  especie: Especie;
  raza: string;
  sexo: Sexo;
  fechaNacimiento: string;
  esFechaEstimada: boolean;
  edadFormateada: string;
  pesoActualKg: number;
  colorSenas?: string | null;
  estadoReproductivo: EstadoReproductivo;
  createdAt: string;
  updatedAt: string;
}

export interface PuntoCurvaPeso {
  fecha: string;
  pesoKg: number;
  tipoEvento: string;
  detalle?: string | null;
}

export interface PacienteDetalle extends Paciente {
  propietario?: Propietario | null;
  atenciones: AtencionClinica[];
  inmunizaciones: Inmunizacion[];
  curvaPeso: PuntoCurvaPeso[];
}

export interface CrearPacienteDto {
  propietarioId: number;
  nombre: string;
  especie: Especie;
  raza: string;
  sexo: Sexo;
  fechaNacimiento: string;
  esFechaEstimada: boolean;
  pesoActualKg: number;
  colorSenas?: string | null;
  estadoReproductivo: EstadoReproductivo;
}

export interface ActualizarPacienteDto {
  propietarioId: number;
  nombre: string;
  especie: Especie;
  raza: string;
  sexo: Sexo;
  fechaNacimiento: string;
  esFechaEstimada: boolean;
  pesoActualKg: number;
  colorSenas?: string | null;
  estadoReproductivo: EstadoReproductivo;
}

export interface AtencionClinica {
  id: number;
  pacienteId: number;
  nombrePaciente?: string | null;
  veterinarioId: number;
  nombreVeterinario?: string | null;
  fechaHoraAtencion: string;
  pesoConsultaKg: number;
  motivoConsulta: string;
  examenClinico?: string | null;
  diagnostico: string;
  tratamiento: string;
  indicaciones?: string | null;
  fechaControl?: string | null;
  createdAt: string;
}

export interface CrearAtencionDto {
  pacienteId: number;
  veterinarioId: number;
  fechaHoraAtencion?: string | null;
  pesoConsultaKg: number;
  motivoConsulta: string;
  examenClinico?: string | null;
  diagnostico: string;
  tratamiento: string;
  indicaciones?: string | null;
  fechaControl?: string | null;
}

export interface Inmunizacion {
  id: number;
  pacienteId: number;
  nombrePaciente?: string | null;
  veterinarioId: number;
  nombreVeterinario?: string | null;
  tipoBiologico: TipoBiologico;
  nombreProducto: string;
  loteFabricante?: string | null;
  fechaAplicacion: string;
  fechaRefuerzo: string;
  estadoRefuerzo: 'AlDia' | 'Proximo' | 'Vencido';
  diasRestantes: number;
  observaciones?: string | null;
  createdAt: string;
}

export interface CrearInmunizacionDto {
  pacienteId: number;
  veterinarioId: number;
  tipoBiologico: TipoBiologico;
  nombreProducto: string;
  loteFabricante?: string | null;
  fechaAplicacion: string;
  fechaRefuerzo: string;
  observaciones?: string | null;
}

export interface Recordatorio {
  inmunizacionId: number;
  pacienteId: number;
  nombrePaciente: string;
  especiePaciente: string;
  propietarioId: number;
  nombrePropietario: string;
  telefonoPropietario: string;
  emailPropietario?: string | null;
  tipoBiologico: TipoBiologico;
  nombreProducto: string;
  fechaAplicacion: string;
  fechaRefuerzo: string;
  estado: 'Proximo' | 'Vencido';
  diasDiferencia: number;
  mensajeSugerido: string;
  whatsAppUrl: string;
  mailtoUrl?: string | null;
}
