# Matriz de Trazabilidad del Proyecto (trazabilidad.md)

## 1. Requisitos Funcionales (RF)
| ID | Nombre del Requisito | Prioridad | Módulo | Archivos del Proyecto | Fase(s) | Cobertura |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **RF-01** | Autenticación y Control de Acceso Local | Alta (Must) | Autenticación | `Usuario.cs`, `IAuthService.cs`, `AuthService.cs`, `PasswordHasher.cs`, `LoginViewModel.cs`, `LoginView.xaml` | Fase 3, Fase 6 | Cubierto |
| **RF-02** | Gestión de Propietarios | Alta (Must) | Propietarios | `Propietario.cs`, `NumeroCelular.cs`, `IPropietarioRepository.cs`, `PropietarioRepository.cs`, `ClinicaService.cs`, `PropietariosViewModel.cs`, `PropietarioModalViewModel.cs`, `PropietariosView.xaml`, `PropietarioModalView.xaml` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RF-03** | Búsqueda y Consulta de Propietarios | Alta (Must) | Propietarios | `Propietario.cs`, `Paciente.cs`, `IPropietarioRepository.cs`, `PropietarioRepository.cs`, `ClinicaService.cs`, `PropietariosViewModel.cs`, `PropietariosView.xaml` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RF-04** | Registro de Paciente con Propietario | Alta (Must) | Pacientes | `Paciente.cs`, `Propietario.cs`, `PesoCorporal.cs`, `IPacienteRepository.cs`, `PacienteRepository.cs`, `ClinicaService.cs`, `PacientesViewModel.cs`, `PacientesView.xaml`, `PropietarioModalView.xaml` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RF-05** | Consulta y Actualización de Paciente | Media (Must) | Pacientes | `Paciente.cs`, `IPacienteRepository.cs`, `PacienteRepository.cs`, `ClinicaService.cs`, `PacientesViewModel.cs`, `PacientesView.xaml` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RF-06** | Registro de Consulta Médica | Alta (Must) | Historia Clínica | `AtencionClinica.cs`, `Veterinario.cs`, `IAtencionClinicaRepository.cs`, `AtencionClinicaRepository.cs`, `ClinicaService.cs`, `NuevaAtencionViewModel.cs`, `NuevaAtencionModalView.xaml` | Fase 2, Fase 3, Fase 8 | Cubierto |
| **RF-07** | Historia Clínica Consolidada | Alta (Must) | Historia Clínica | `AtencionClinica.cs`, `IAtencionClinicaRepository.cs`, `AtencionClinicaRepository.cs`, `ClinicaService.cs`, `HistoriaClinicaViewModel.cs`, `HistoriaClinicaView.xaml` | Fase 2, Fase 3, Fase 8 | Cubierto |
| **RF-08** | Inmunización y Desparasitación | Alta (Must) | Vacunación | `Inmunizacion.cs`, `Veterinario.cs`, `IInmunizacionRepository.cs`, `InmunizacionRepository.cs`, `ClinicaService.cs`, `InmunizacionesViewModel.cs`, `InmunizacionesView.xaml` | Fase 2, Fase 3, Fase 9 | Cubierto |
| **RF-09** | Carnet de Vacunación PDF | Alta (Must) | Vacunación / PDF | `IPdfExportService.cs`, `QuestPdfExportService.cs`, `InmunizacionesViewModel.cs`, `InmunizacionesView.xaml` | Fase 4, Fase 9 | Cubierto |
| **RF-10** | Recordatorio por WhatsApp (wa.me) | Alta (Must) | Recordatorios | `IExternalLauncherService.cs`, `ExternalLauncherService.cs`, `RecordatoriosViewModel.cs`, `RecordatoriosView.xaml` | Fase 4, Fase 9 | Cubierto |
| **RF-11** | Recordatorio por Correo (mailto:) | Media (Must) | Recordatorios | `IExternalLauncherService.cs`, `ExternalLauncherService.cs`, `RecordatoriosViewModel.cs`, `RecordatoriosView.xaml` | Fase 4, Fase 9 | Cubierto |

## 2. Casos de Uso (CU)
| ID | Nombre del Caso de Uso | Actores | Módulo Principal | Archivos de Presentación y Lógica | Fase(s) | Cobertura |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **CU-01** | Iniciar Sesión en el Sistema | Médico Veterinario | Autenticación | `LoginView.xaml`, `LoginViewModel.cs`, `AuthService.cs`, `PasswordHasher.cs` | Fase 6 | Cubierto |
| **CU-02** | Registrar Mascota y Propietario | Médico Veterinario | Pacientes / Propietarios | `PacientesView.xaml`, `PacientesViewModel.cs`, `PropietarioModalView.xaml`, `PropietarioModalViewModel.cs`, `ClinicaService.cs` | Fase 7 | Cubierto |
| **CU-03** | Registrar Atención Médica | Médico Veterinario | Historia Clínica | `NuevaAtencionModalView.xaml`, `NuevaAtencionViewModel.cs`, `ClinicaService.cs` | Fase 8 | Cubierto |
| **CU-04** | Consultar Historia Clínica | Médico Veterinario | Historia Clínica | `HistoriaClinicaView.xaml`, `HistoriaClinicaViewModel.cs`, `ClinicaService.cs`, `AtencionClinicaRepository.cs` | Fase 8 | Cubierto |
| **CU-05** | Registrar Vacuna y Carnet PDF | Médico Veterinario | Vacunación | `InmunizacionesView.xaml`, `InmunizacionesViewModel.cs`, `QuestPdfExportService.cs`, `ClinicaService.cs` | Fase 9 | Cubierto |
| **CU-06** | Emitir Recordatorio Gratuito | Médico Veterinario | Recordatorios | `RecordatoriosView.xaml`, `RecordatoriosViewModel.cs`, `ExternalLauncherService.cs` | Fase 9 | Cubierto |

## 3. Requisitos No Funcionales (RNF)
| ID | Nombre del Requisito | Categoría | Criterio de Aceptación / Métrica | Archivos / Componentes | Fase(s) | Cobertura |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **RNF-01** | Protección de Credenciales | Seguridad | PBKDF2 HMAC-SHA256, sal 128 bits, 100k iteraciones | `PasswordHasher.cs`, `UsuarioConfiguration.cs` | Fase 3 | Cubierto |
| **RNF-02** | Reserva Legal de Datos | Seguridad | Acceso restringido a UI local (Ley 576 de 2000) | `VetClinicDbContext.cs`, `HistoriaClinicaView.xaml` | Fase 3, Fase 8 | Cubierto |
| **RNF-03** | Reducción de Fricción | Usabilidad | Registro de atención en $\le 90$ segundos | `NuevaAtencionModalView.xaml`, `NuevaAtencionViewModel.cs` | Fase 5, Fase 8 | Cubierto |
| **RNF-04** | Legibilidad Visual | Usabilidad | Segoe UI $\ge 14$pt, contraste $\ge 4.5:1$ a 1 metro | `Typography.xaml`, `Colors.xaml`, `Controls.xaml` | Fase 5 | Cubierto |
| **RNF-05** | Tiempo de Respuesta Local | Rendimiento | Consulta de historial $< 1$ s con hasta 100 registros | Índices en `AtencionesClinicas`, `AtencionClinicaRepository.cs` | Fase 3, Fase 8, Fase 10 | Cubierto |
| **RNF-06** | Eficiencia Generación PDF | Rendimiento | Creación de PDF en $\le 3$ segundos | `QuestPdfExportService.cs`, `InmunizacionesViewModel.cs` | Fase 4, Fase 9, Fase 10 | Cubierto |
| **RNF-07** | Operación Autónoma Offline | Disponibilidad | 100% funciones operativas sin conexión a internet | Base de datos SQLite local, arquitectura autocontenida | Fase 3, Fase 10 | Cubierto |
| **RNF-08** | Estación Monopuesto | Restricción | Sin servidor dedicado; datos en `%LocalAppData%` | `VetClinicSolution.sln`, `VetClinicDbContext.cs` | Fase 1, Fase 3, Fase 10 | Cubierto |
| **RNF-09** | Cero Costo de Mensajería | Restricción | Uso exclusivo de esquemas wa.me y mailto: | `ExternalLauncherService.cs`, `RecordatoriosViewModel.cs` | Fase 4, Fase 9 | Cubierto |

## 4. Reglas de Negocio (RN)
| Código | Regla de Negocio | Mecanismo de Implementación | Archivos / Componentes | Fase(s) | Cobertura |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **RN-01** | Autenticación Obligatoria | Validación hash PBKDF2 contra tabla `Usuarios` | `AuthService.cs`, `PasswordHasher.cs`, `LoginViewModel.cs` | Fase 3, Fase 6 | Cubierto |
| **RN-02** | Trazabilidad del Veterinario | FK obligatoria a `Veterinarios` en atenciones/inmunizaciones | `AtencionClinicaConfiguration.cs`, `NuevaAtencionViewModel.cs` | Fase 2, Fase 3, Fase 8 | Cubierto |
| **RN-03** | Cardinalidad Propietario-Paciente | Relación $1:N$, FK restrict en `Pacientes` | `PacienteConfiguration.cs`, `Paciente.cs`, `Propietario.cs` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RN-04** | Validación Celular Colombia | Regex `^3[0-9]{9}$` y CHECK en tabla | `NumeroCelular.cs`, `PropietarioConfiguration.cs`, `PropietarioModalViewModel.cs` | Fase 2, Fase 3, Fase 7 | Cubierto |
| **RN-05** | Dinamismo en Edad | Cálculo dinámico en visualización (días/meses/años) | `Paciente.cs` (`CalcularEdadFormateada`), `PacientesViewModel.cs` | Fase 2, Fase 7 | Cubierto |
| **RN-06** | Estandarización de Peso en Kg | Validación numérico decimal en rango $(0.00, 150.00]$ | `PesoCorporal.cs`, `PacienteConfiguration.cs`, `AtencionClinicaConfiguration.cs` | Fase 2, Fase 3, Fase 7, Fase 8 | Cubierto |
| **RN-07** | Inmutabilidad de Historia Clínica | Triggers SQLite que bloquean UPDATE y DELETE | `DbInitializer.cs` (`TR_AtencionesClinicas_Prevent*`), `AtencionClinicaRepository.cs` | Fase 3, Fase 8 | Cubierto |
| **RN-08** | Estructura Mínima Carnet PDF | Consolidación de producto, fechas, refuerzo y veterinario | `QuestPdfExportService.cs`, `InmunizacionesViewModel.cs` | Fase 4, Fase 9 | Cubierto |
| **RN-09** | Notificación Gratuita | Disparo de `https://wa.me/57...` y `mailto:...` vía SO | `ExternalLauncherService.cs`, `RecordatoriosViewModel.cs` | Fase 4, Fase 9 | Cubierto |
| **RN-10** | Entrada Ágil de Datos | Precarga de fechas/horas y modal en el mismo flujo | `NuevaAtencionModalView.xaml`, `PropietarioModalView.xaml`, ViewModels | Fase 5, Fase 7, Fase 8 | Cubierto |
