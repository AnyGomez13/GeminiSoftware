# Plan Detallado de Implementación por Fases (fases.md)

## Fase 1: Setup del Entorno y Estructura de la Solución
- **Objetivo:** Inicializar la solución .NET 8 con la arquitectura en tres capas, proyectos de pruebas y configuración de dependencias básicas sin código de negocio.
- **Archivos a crear o modificar:**
  - `VetClinicSolution.sln`
  - `VetClinic.Domain/VetClinic.Domain.csproj`
  - `VetClinic.Infrastructure/VetClinic.Infrastructure.csproj`
  - `VetClinic.Presentation/VetClinic.Presentation.csproj`
  - `VetClinic.Domain.Tests/VetClinic.Domain.Tests.csproj`
  - `VetClinic.Infrastructure.Tests/VetClinic.Infrastructure.Tests.csproj`
  - `.gitignore`
- **Dependencias de fases anteriores:** Ninguna.
- **IDs de requisitos que cubre:** RNF-08.
- **Criterio de terminado verificable:** `dotnet build VetClinicSolution.sln` finaliza con código de salida 0, 0 advertencias y 0 errores.
- **Estado:** Completada.

## Fase 2: Capa de Dominio (Entidades, Value Objects e Interfaces)
- **Objetivo:** Definir las entidades de negocio, objetos de valor inmutables, enums, contratos de repositorios y servicios de dominio con sus reglas de validación.
- **Archivos a crear o modificar:**
  - `VetClinic.Domain/Common/BaseEntity.cs`
  - `VetClinic.Domain/Common/Result.cs`
  - `VetClinic.Domain/Common/Error.cs`
  - `VetClinic.Domain/Entities/Usuario.cs`
  - `VetClinic.Domain/Entities/Veterinario.cs`
  - `VetClinic.Domain/Entities/Propietario.cs`
  - `VetClinic.Domain/Entities/Paciente.cs`
  - `VetClinic.Domain/Entities/AtencionClinica.cs`
  - `VetClinic.Domain/Entities/Inmunizacion.cs`
  - `VetClinic.Domain/Enums/TipoDocumento.cs`
  - `VetClinic.Domain/Enums/Especie.cs`
  - `VetClinic.Domain/Enums/Sexo.cs`
  - `VetClinic.Domain/Enums/EstadoReproductivo.cs`
  - `VetClinic.Domain/Enums/TipoBiologico.cs`
  - `VetClinic.Domain/ValueObjects/NumeroCelular.cs`
  - `VetClinic.Domain/ValueObjects/PesoCorporal.cs`
  - `VetClinic.Domain/Interfaces/Repositories/IRepository.cs`
  - `VetClinic.Domain/Interfaces/Repositories/IPropietarioRepository.cs`
  - `VetClinic.Domain/Interfaces/Repositories/IPacienteRepository.cs`
  - `VetClinic.Domain/Interfaces/Repositories/IAtencionClinicaRepository.cs`
  - `VetClinic.Domain/Interfaces/Repositories/IInmunizacionRepository.cs`
  - `VetClinic.Domain/Interfaces/IUnitOfWork.cs`
  - `VetClinic.Domain/Interfaces/Services/IAuthService.cs`
  - `VetClinic.Domain/Interfaces/Services/IClinicaService.cs`
  - `VetClinic.Domain/Interfaces/Services/IPdfExportService.cs`
  - `VetClinic.Domain/Interfaces/Services/IExternalLauncherService.cs`
  - `VetClinic.Domain.Tests/Entities/PacienteTests.cs`
  - `VetClinic.Domain.Tests/ValueObjects/NumeroCelularTests.cs`
  - `VetClinic.Domain.Tests/ValueObjects/PesoCorporalTests.cs`
- **Dependencias de fases anteriores:** Fase 1.
- **IDs de requisitos que cubre:** RN-03, RN-04, RN-05, RN-06, S-01, S-02, STF-06.
- **Criterio de terminado verificable:** `dotnet test VetClinic.Domain.Tests` aprueba 100% de las pruebas validando formato celular colombiano (10 dígitos iniciando en 3), rango de peso ($0.01 - 150.00$ Kg) y cálculo dinámico de edad en días/meses/años.
- **Estado:** Completada.

## Fase 3: Capa de Infraestructura y Persistencia (EF Core, SQLite y Seguridad)
- **Objetivo:** Implementar el contexto de base de datos local SQLite, mapeo relacional fluido, migraciones, triggers de inmutabilidad, sembrado de datos y hashing de credenciales PBKDF2.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure/Data/VetClinicDbContext.cs`
  - `VetClinic.Infrastructure/Data/Configurations/UsuarioConfiguration.cs`
  - `VetClinic.Infrastructure/Data/Configurations/VeterinarioConfiguration.cs`
  - `VetClinic.Infrastructure/Data/Configurations/PropietarioConfiguration.cs`
  - `VetClinic.Infrastructure/Data/Configurations/PacienteConfiguration.cs`
  - `VetClinic.Infrastructure/Data/Configurations/AtencionClinicaConfiguration.cs`
  - `VetClinic.Infrastructure/Data/Configurations/InmunizacionConfiguration.cs`
  - `VetClinic.Infrastructure/Data/DbInitializer.cs`
  - `VetClinic.Infrastructure/Repositories/Repository.cs`
  - `VetClinic.Infrastructure/Repositories/PropietarioRepository.cs`
  - `VetClinic.Infrastructure/Repositories/PacienteRepository.cs`
  - `VetClinic.Infrastructure/Repositories/AtencionClinicaRepository.cs`
  - `VetClinic.Infrastructure/Repositories/InmunizacionRepository.cs`
  - `VetClinic.Infrastructure/Repositories/UnitOfWork.cs`
  - `VetClinic.Infrastructure/Security/PasswordHasher.cs`
  - `VetClinic.Infrastructure.Tests/Security/PasswordHasherTests.cs`
  - `VetClinic.Infrastructure.Tests/Data/PersistenceAndTriggersTests.cs`
- **Dependencias de fases anteriores:** Fase 2.
- **IDs de requisitos que cubre:** RF-01, RN-01, RN-02, RN-07, RNF-01, RNF-05, RNF-07, S-03, S-05, STF-01, STF-02, STF-05.
- **Criterio de terminado verificable:** `dotnet test VetClinic.Infrastructure.Tests` aprueba 100% de las pruebas: derivación PBKDF2 (100.000 iteraciones, salt 16 bytes), inicialización de SQLite con PRAGMAs (`foreign_keys=ON`, `journal_mode=WAL`), aborto en base de datos ante sentencias `UPDATE`/`DELETE` en atenciones e inmunizaciones, y actualización de peso del paciente vía trigger.
- **Estado:** Completada.

## Fase 4: Servicios de Infraestructura (QuestPDF y Persistencia)
- **Objetivo:** Implementar el motor de exportación de carnet digital a PDF mediante QuestPDF y persistencia verificada.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure/Services/QuestPdfExportService.cs`
  - `VetClinic.Infrastructure/Services/ExternalLauncherService.cs`
  - `VetClinic.Infrastructure.Tests/Services/QuestPdfExportServiceTests.cs`
  - `VetClinic.Infrastructure.Tests/Services/ExternalLauncherServiceTests.cs`
- **Dependencias de fases anteriores:** Fase 3.
- **IDs de requisitos que cubre:** RF-09, RF-10, RF-11, RN-08, RN-09, RNF-06, RNF-09, S-04, STF-03, STF-04.
- **Criterio de terminado verificable:** Prueba automatizada genera un archivo PDF binario válido en disco en tiempo $\le 3$ segundos (RNF-06); pruebas unitarias validan que las URIs generadas para WhatsApp y correo electrónico cumplan la especificación RFC sin alterar caracteres especiales.
- **Estado:** Completada.

---

# Fases de la Rama Web (Arquitectura Localhost: ASP.NET Core API + React/TypeScript SPA)

## Fase W-05: Backend Web API y DTOs (`VetClinic.Api`)
- **Objetivo:** Crear el proyecto ASP.NET Core Web API (.NET 8), definir DTOs desacoplados para eliminar ciclos circulares JSON, e implementar controladores REST con inyección de dependencias de `VetClinic.Infrastructure`.
- **Archivos a crear o modificar:**
  - `VetClinic.Api/VetClinic.Api.csproj`
  - `VetClinic.Api/Program.cs`
  - `VetClinic.Api/Dtos/AuthDtos.cs`
  - `VetClinic.Api/Dtos/PropietarioDtos.cs`
  - `VetClinic.Api/Dtos/PacienteDtos.cs`
  - `VetClinic.Api/Dtos/AtencionDtos.cs`
  - `VetClinic.Api/Dtos/InmunizacionDtos.cs`
  - `VetClinic.Api/Dtos/RecordatorioDtos.cs`
  - `VetClinic.Api/Controllers/AuthController.cs`
  - `VetClinic.Api/Controllers/PropietariosController.cs`
  - `VetClinic.Api/Controllers/PacientesController.cs`
  - `VetClinic.Api/Controllers/AtencionesController.cs`
  - `VetClinic.Api/Controllers/InmunizacionesController.cs`
  - `VetClinic.Api/Controllers/VeterinariosController.cs`
  - `VetClinic.Infrastructure/Services/ClinicaService.cs` (actualizaciones para sincronización inmediata de peso en memoria y búsqueda mejorada)
  - `VetClinicSolution.sln` (incorporar proyecto `VetClinic.Api`)
- **Dependencias de fases anteriores:** Fase 4.
- **IDs de requisitos que cubre:** RF-01 a RF-11, RN-01 a RN-09, RNF-01, RNF-05, RNF-08.
- **Criterio de terminado verificable:** `dotnet build VetClinicSolution.sln` finaliza con código 0 y 0 errores; los endpoints REST devuelven respuestas HTTP JSON válidas en Swagger / pruebas unitarias, incluyendo descarga del PDF de carnet.
- **Estado:** Completada.

## Fase W-06: Setup del Frontend Web y Sistema de Diseño (`VetClinic.Web`)
- **Objetivo:** Inicializar la SPA en React 18 con TypeScript y Vite, configurar Tailwind CSS con la paleta cálida/botánica, Lucide Icons, Sonner y el layout clínico maestro.
- **Archivos a crear o modificar:**
  - `VetClinic.Web/package.json`
  - `VetClinic.Web/vite.config.ts`
  - `VetClinic.Web/tailwind.config.js`
  - `VetClinic.Web/src/index.css` (tokens de diseño: lino cálido `#FBF9F5`, verde bosque `#166534`, esmeralda `#059669`, ámbar `#F59E0B`)
  - `VetClinic.Web/src/components/layout/Sidebar.tsx`
  - `VetClinic.Web/src/components/layout/Header.tsx` (con buscador global `Ctrl + K`)
  - `VetClinic.Web/src/components/layout/MainLayout.tsx`
  - `VetClinic.Web/src/services/apiClient.ts`
  - `VetClinic.Web/src/types/index.ts`
- **Dependencias de fases anteriores:** Fase W-05.
- **IDs de requisitos que cubre:** RNF-03, RNF-04, RNF-08.
- **Criterio de terminado verificable:** `npm run build` en `VetClinic.Web` genera el bundle sin errores TypeScript; el layout se renderiza con navegación lateral, header, contraste accesible $\ge 4.5:1$ y diseño responsivo.
- **Estado:** Completada.

## Fase W-07: Módulo de Autenticación y Control de Sesión (CU-01, RF-01)
- **Objetivo:** Implementar la pantalla de inicio de sesión web con diseño acogedor, validación de credenciales contra la API y gestión de sesión local.
- **Archivos a crear o modificar:**
  - `VetClinic.Web/src/pages/LoginPage.tsx`
  - `VetClinic.Web/src/context/AuthContext.tsx`
  - `VetClinic.Web/src/components/auth/ProtectedRoute.tsx`
- **Dependencias de fases anteriores:** Fase W-06.
- **IDs de requisitos que cubre:** RF-01, CU-01, RN-01, RNF-01, RNF-08.
- **Criterio de terminado verificable:** Login con `admin` / `Clinica2026*` autentica y redirige a la vista principal almacenando sesión; contraseña inválida despliega feedback visual en rojo y bloquea el acceso.
- **Estado:** Completada.

## Fase W-08: Módulo de Directorio de Propietarios y Censo de Pacientes (CU-02, CU-03, RF-02..05)
- **Objetivo:** Construir las interfaces para directorio de acudientes y censo de mascotas, con búsqueda reactiva, modales de alta/edición y validación de celular Colombia.
- **Archivos a crear o modificar:**
  - `VetClinic.Web/src/pages/PropietariosPage.tsx`
  - `VetClinic.Web/src/components/propietarios/PropietarioModal.tsx`
  - `VetClinic.Web/src/pages/PacientesPage.tsx`
  - `VetClinic.Web/src/components/pacientes/PacienteModal.tsx`
  - `VetClinic.Web/src/components/pacientes/AlertasBadge.tsx`
- **Dependencias de fases anteriores:** Fase W-07.
- **IDs de requisitos que cubre:** RF-02, RF-03, RF-04, RF-05, CU-02, CU-03, RN-03, RN-04, RN-05, RN-06.
- **Criterio de terminado verificable:** Búsqueda en tiempo real por nombre, documento o teléfono; alta exitosa de propietario con validación estricta de celular Colombia (10 dígitos iniciando en 3); visualización de edad dinámica calculada y badges de alertas médicas.
- **Estado:** Completada.

## Fase W-09: Módulo de Historia Clínica, Consulta y Curva de Peso (CU-04, RF-06, RF-07)
- **Objetivo:** Construir la vista 360° del expediente clínico con línea de tiempo cronológica inmutable, registro de consulta en panel deslizante (*Drawer*) y gráfica de evolución de peso.
- **Archivos a crear o modificar:**
  - `VetClinic.Web/src/pages/HistoriaClinicaPage.tsx`
  - `VetClinic.Web/src/components/historia/TimelineAtenciones.tsx`
  - `VetClinic.Web/src/components/historia/NuevaAtencionDrawer.tsx`
  - `VetClinic.Web/src/components/historia/CurvaPesoChart.tsx`
- **Dependencias de fases anteriores:** Fase W-08.
- **IDs de requisitos que cubre:** RF-06, RF-07, CU-04, RN-02, RN-05, RN-07, STF-01, STF-05.
- **Criterio de terminado verificable:** Registro de atención asigna obligatoriamente al Dr. Fabio o Dr. William; actualiza en tiempo real el peso del paciente y agrega un punto a la curva gráfica; no existen opciones para editar ni eliminar historias confirmadas (inmutabilidad legal Ley 576).
- **Estado:** Completada.

## Fase W-10: Módulo de Vacunación, Carnet PDF y Recordatorios 1-Clic (CU-05, CU-06, RF-08..11)
- **Objetivo:** Implementar la gestión de biológicos, descarga y previsualización del Carnet Digital PDF, y tablero de recordatorios con segmentación de vencidas y próximas a vencer con enlaces directos a WhatsApp y correo.
- **Archivos a crear o modificar:**
  - `VetClinic.Web/src/pages/VacunacionPage.tsx`
  - `VetClinic.Web/src/components/vacunacion/NuevaInmunizacionModal.tsx`
  - `VetClinic.Web/src/components/vacunacion/CarnetPdfViewer.tsx`
  - `VetClinic.Web/src/pages/RecordatoriosPage.tsx`
  - `VetClinic.Web/src/components/recordatorios/RecordatorioCard.tsx`
- **Dependencias de fases anteriores:** Fase W-09.
- **IDs de requisitos que cubre:** RF-08, RF-09, RF-10, RF-11, CU-05, CU-06, RN-08, RN-09, RNF-06, RNF-09, S-04.
- **Criterio de terminado verificable:** Generación y descarga de Carnet PDF en $\le 3$ s; tablero muestra refuerzos próximos y refuerzos vencidos; clic en "WhatsApp" abre `https://wa.me/57...` con mensaje preformateado y clic en "Correo" abre `mailto:`.
- **Estado:** Pendiente.

## Fase W-11: Integración en Kestrel, Pruebas E2E y Empaquetado Monopuesto Local (RNF-07, RNF-08)
- **Objetivo:** Configurar ASP.NET Core para servir los archivos estáticos de la SPA React (`wwwroot`) desde Kestrel en `http://localhost:5000`, verificar pruebas completas y empaquetar para ejecución local en un solo paso.
- **Archivos a crear o modificar:**
  - `VetClinic.Api/Program.cs` (Static files y SPA fallback)
  - `VetClinic.Api.Tests/Integration/WebApiE2ETests.cs`
  - Scripts de compilación y empaquetado de producción
- **Dependencias de fases anteriores:** Fase W-10.
- **IDs de requisitos que cubre:** RNF-01 a RNF-09, todos los RF y RN.
- **Criterio de terminado verificable:** `dotnet test VetClinicSolution.sln` finaliza con 100% de pruebas aprobadas; `dotnet run --project VetClinic.Api` levanta el servidor Kestrel en `http://localhost:5000` y sirve la aplicación web completa de forma autónoma sin internet externa (RNF-07, RNF-08).
- **Estado:** Pendiente.

---

## Supuestos y Decisiones de la Rama Web
- **Supuesto 1 (Localhost autónomo):** La aplicación web opera en modo monopuesto local sobre Kestrel (`http://localhost:5000`) utilizando la base de datos embebida SQLite en `%LocalAppData%\VetClinic\vetclinic_local.db`, cumpliendo RNF-07 y RNF-08.
- **Supuesto 2 (Reutilización de Capas Core):** Las capas `VetClinic.Domain` y `VetClinic.Infrastructure` se mantienen al 100% compatibles, asegurando los triggers de inmutabilidad de la Ley 576 y las 79 pruebas automatizadas existentes.
- **Supuesto 3 (Paleta de Diseño):** Se aplica la paleta cálida y botánica aprobada (#166534 verde bosque, #059669 esmeralda, #FBF9F5 lino suave, #F59E0B ámbar) garantizando ratio de contraste $\ge 4.5:1$ (RNF-04).
