import type {
  ActualizarPacienteDto,
  ActualizarPropietarioDto,
  AtencionClinica,
  CrearAtencionDto,
  CrearInmunizacionDto,
  CrearPacienteDto,
  CrearPropietarioDto,
  Inmunizacion,
  LoginResponse,
  Paciente,
  PacienteDetalle,
  Propietario,
  Recordatorio,
  Veterinario
} from '../types';

const API_BASE = '/api';

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let errorMessage = `Error en el servidor (${response.status})`;
    try {
      const errorJson = await response.json();
      errorMessage = errorJson.mensaje || errorJson.message || errorMessage;
    } catch {
      // Ignorar fallo de parseo si no era json
    }
    throw new ApiError(response.status, errorMessage);
  }

  return response.json() as Promise<T>;
}

export const api = {
  // Autenticación (CU-01, RF-01)
  auth: {
    login: (username: string, password: string): Promise<LoginResponse> =>
      request<LoginResponse>('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ username, password }),
      }),
  },

  // Veterinarios Oficiales (RN-02, STF-01)
  veterinarios: {
    getAll: (): Promise<Veterinario[]> =>
      request<Veterinario[]>('/veterinarios'),
  },

  // Propietarios (CU-02, RF-02, RF-03)
  propietarios: {
    buscar: (criterio?: string): Promise<Propietario[]> =>
      request<Propietario[]>(`/propietarios${criterio ? `?criterio=${encodeURIComponent(criterio)}` : ''}`),
    getById: (id: number): Promise<Propietario> =>
      request<Propietario>(`/propietarios/${id}`),
    crear: (dto: CrearPropietarioDto): Promise<Propietario> =>
      request<Propietario>('/propietarios', {
        method: 'POST',
        body: JSON.stringify(dto),
      }),
    actualizar: (id: number, dto: ActualizarPropietarioDto): Promise<Propietario> =>
      request<Propietario>(`/propietarios/${id}`, {
        method: 'PUT',
        body: JSON.stringify(dto),
      }),
  },

  // Pacientes (CU-03, RF-04, RF-05)
  pacientes: {
    buscar: (criterio?: string): Promise<Paciente[]> =>
      request<Paciente[]>(`/pacientes${criterio ? `?criterio=${encodeURIComponent(criterio)}` : ''}`),
    getById: (id: number): Promise<PacienteDetalle> =>
      request<PacienteDetalle>(`/pacientes/${id}`),
    crear: (dto: CrearPacienteDto): Promise<Paciente> =>
      request<Paciente>('/pacientes', {
        method: 'POST',
        body: JSON.stringify(dto),
      }),
    actualizar: (id: number, dto: ActualizarPacienteDto): Promise<Paciente> =>
      request<Paciente>(`/pacientes/${id}`, {
        method: 'PUT',
        body: JSON.stringify(dto),
      }),
  },

  // Atenciones Clínicas (CU-04, RF-06, RF-07, RN-07)
  atenciones: {
    getHistorial: (pacienteId: number): Promise<AtencionClinica[]> =>
      request<AtencionClinica[]>(`/atenciones/paciente/${pacienteId}`),
    crear: (dto: CrearAtencionDto): Promise<AtencionClinica> =>
      request<AtencionClinica>('/atenciones', {
        method: 'POST',
        body: JSON.stringify(dto),
      }),
  },

  // Inmunizaciones y Carnet Digital PDF (CU-05, RF-08, RF-09)
  inmunizaciones: {
    getPorPaciente: (pacienteId: number): Promise<Inmunizacion[]> =>
      request<Inmunizacion[]>(`/inmunizaciones/paciente/${pacienteId}`),
    crear: (dto: CrearInmunizacionDto): Promise<Inmunizacion> =>
      request<Inmunizacion>('/inmunizaciones', {
        method: 'POST',
        body: JSON.stringify(dto),
      }),
    descargarCarnetBlob: async (pacienteId: number): Promise<Blob> => {
      const response = await fetch(`${API_BASE}/inmunizaciones/carnet-pdf/${pacienteId}`);
      if (!response.ok) {
        throw new ApiError(response.status, 'No se pudo generar el carnet PDF.');
      }
      return response.blob();
    },
  },

  // Recordatorios y Notificaciones 1-Clic (CU-06, RF-10, RF-11, RN-09)
  recordatorios: {
    get: (dias = 30, estado: 'todos' | 'proximos' | 'vencidos' = 'todos'): Promise<Recordatorio[]> =>
      request<Recordatorio[]>(`/recordatorios?dias=${dias}&estado=${estado}`),
  },
};
