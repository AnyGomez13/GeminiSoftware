# VetClinic Pro - Sistema de Gestión Clínica Veterinaria

Sistema de gestión médica veterinaria de estación monopuesto local para la administración de historias clínicas inmutables, pacientes, propietarios, planes de inmunización y recordatorios sin costos operativos de mensajería ni servidores remotos. Diseñado para la práctica clínica de los **Dres. Fabio y William** bajo estricto cumplimiento del Código de Ética y la **Ley 576 de 2000** de Colombia.

---

## 📋 Tabla de Contenidos

- [🌿 Estado Actual de la Rama `web`](#-estado-actual-de-la-rama-web)
- [⚖️ Comparativa de Ramas: `main` vs. `web`](#-comparativa-de-ramas-main-vs-web)
- [🛠️ Resumen de lo Realizado en Esta Sesión](#️-resumen-de-lo-realizado-en-esta-sesión)
- [🏗️ Arquitectura y Stack Tecnológico](#️-arquitectura-y-stack-tecnológico)
- [📁 Estructura del Repositorio](#-estructura-del-repositorio)
- [🚀 Puesta en Marcha y Ejecución Local](#-puesta-en-marcha-y-ejecución-local)
- [🧪 Pruebas Automatizadas y Calidad](#-pruebas-automatizadas-y-calidad)
- [📜 Marco Legal y Reglas de Negocio Destacadas](#-marco-legal-y-reglas-de-negocio-destacadas)
- [📚 Documentación Técnica](#-documentación-técnica)

---

## 🌿 Estado Actual de la Rama `web`

La rama **`web`** representa la evolución y modernización integral de VetClinic Pro, sustituyendo la interfaz gráfica de escritorio legacy (WPF) por una **aplicación web monopuesto local** de alto rendimiento, ejecutada sobre el servidor embebido Kestrel de ASP.NET Core 8 y una SPA moderna desarrollada en React + TypeScript + Tailwind CSS.

---

## ⚖️ Comparativa de Ramas: `main` vs. `web`

| Dimensión | Rama `main` (Legacy Desktop) | Rama `web` (Moderna Web Local) |
| :--- | :--- | :--- |
| **Arquitectura de UI** | Desktop monolítico WPF (`VetClinic.Presentation`) basado en XAML y MVVM clásico. | Arquitectura desacoplada: **ASP.NET Core Web API 8** (`VetClinic.Api`) + **SPA React 19** (`VetClinic.Web`). |
| **Plataforma y Acceso** | Exclusivo para entornos Windows Desktop (.NET Windows Desktop Runtime). | **Acceso vía navegador en `http://localhost:5000`**, compatible con cualquier explorador moderno sin plugins. |
| **Sistema de Diseño** | Paleta corporativa básica en XAML con ventanas rígidas. | **Sistema de diseño botánico cálido** (`#FBF9F5` lino suave, `#166534` verde bosque, `#059669` esmeralda, `#F59E0B` ámbar), tipografía accesible y contraste WCAG AAA ($\ge 4.5:1$). |
| **Búsqueda Global** | Búsqueda por vistas aisladas. | **Omnibox unificado reactivo con atajo `Ctrl+K` (o `⌘K`)** para filtrado instantáneo de pacientes, tutores y teléfonos. |
| **Carnet de Vacunación** | Diálogo simple de generación y guardado de archivo PDF en disco. | **Pasaporte médico digital interactivo** con sello oficial, **impresión directa en navegador (`window.print()`)** con reglas `@media print` de alta fidelidad y descarga en streaming QuestPDF en $\le 3$ s. |
| **Evolución de Peso** | Grilla tabular numérica de datos. | **Curva gráfica interactiva SVG** con trazado de variación (+/- Kg), tooltips contextuales y sincronización automática tras cada consulta. |
| **Gestión de Historias** | Ventana estática de consultas. | **Expediente 360° con línea de tiempo cronológica inmutable**, panel deslizante (*Drawer*) para nueva atención y sin botones de edición/borrado (Ley 576 de 2000). |
| **Recordatorios de Refuerzo** | Listado simple en grilla. | **Tablero interactivo segmentado (Próximos y Vencidos)** con selector de horizonte de tiempo (7 a 90 días) y **botones 1-Clic a WhatsApp Web (`https://wa.me/57...`) y correo (`mailto:`) a costo $0**. |
| **Despliegue y Empaquetado** | Ejecución de ejecutable `.exe` dependiente de WPF. | **Empaquetado unificado en Kestrel**: el backend sirve los archivos estáticos desde `wwwroot` y la API REST en un solo proceso autónomo (`.\run-local.ps1`). |
| **Cobertura de Pruebas** | 79 pruebas automatizadas (Domain e Infrastructure). | **107 pruebas automatizadas aprobadas (100%)**, incorporando pruebas unitarias de controladores y pruebas de integración E2E con `WebApplicationFactory`. |

---

## 🛠️ Resumen de lo Realizado en Esta Sesión

Durante esta sesión de trabajo se completó la transición planificada a la rama web mediante la ejecución secuencial y verificación estricta de las siguientes fases:

1. **Fase W-05 — Backend Web API y DTOs (`VetClinic.Api`)**:
   - Creación del proyecto ASP.NET Core 8 Web API referenciado en la solución.
   - Configuración de controladores RESTful limpios con DTOs especializados para evitar ciclos circulares.
   - Sincronización automática del peso del paciente ante el registro de consultas clínicas en SQLite.
   - Endpoint de streaming binario de carnet de vacunación en memoria con QuestPDF (`GET /api/inmunizaciones/carnet-pdf/{id}`).
   - Integración de 17 pruebas unitarias de controladores API.

2. **Fase W-06 — Setup Frontend Web y Sistema de Diseño (`VetClinic.Web`)**:
   - Inicialización del proyecto Vite + React 19 + TypeScript.
   - Configuración de tokens de Tailwind CSS con la paleta cálida/botánica aprobada y soporte `@media print`.
   - Implementación del layout clínico maestro: `Sidebar`, `Header` con Omnibox `Ctrl+K`, y notificaciones Sonner.
   - Cliente HTTP tipado centralizado (`apiClient.ts`).

3. **Fase W-07 — Módulo de Autenticación y Control de Sesión (CU-01, RF-01)**:
   - Creación de `LoginPage.tsx` con diseño botánico acogedor y feedback visual de error.
   - Contexto de sesión reactivo `AuthContext.tsx` con almacenamiento de token y persistencia en `localStorage`.
   - Protección de vistas mediante `ProtectedRoute.tsx`.
   - Validación criptográfica local contra PBKDF2 HMAC-SHA256 (100.000 iteraciones).

4. **Fase W-08 — Directorio de Propietarios y Censo de Pacientes (CU-02..05)**:
   - Vistas `PropietariosPage.tsx` y `PacientesPage.tsx` con búsqueda reactiva en tiempo real.
   - Modales `PropietarioModal.tsx` y `PacienteModal.tsx` con validación estricta de celular Colombia (10 dígitos iniciando en 3, `^3\d{9}$`).
   - Cálculo dinámico de edad en tiempo real al seleccionar fecha de nacimiento.
   - Badges clínicos para especies (Canino/Felino), estado reproductivo y alertas médicas (`AlertasBadge.tsx`).
   - Flujo de creación rápida de tutores embebido directamente dentro del registro de mascotas.

5. **Fase W-09 — Historia Clínica 360°, Consulta y Curva de Peso (CU-04, RF-06, RF-07)**:
   - Vista integral `HistoriaClinicaPage.tsx` con selector de paciente y pestañas contextuales.
   - Panel deslizante lateral (*Drawer*) `NuevaAtencionDrawer.tsx` para registro ágil de consulta con asignación nominal obligatoria al Dr. Fabio o Dr. William.
   - Línea de tiempo cronológica inmutable `TimelineAtenciones.tsx` sin opciones de borrado ni alteración (Ley 576 de 2000).
   - Componente gráfico vectorial responsivo `CurvaPesoChart.tsx` en SVG con visualización de variación de peso.

6. **Fase W-10 — Módulo de Vacunación, Carnet Digital y Recordatorios 1-Clic (CU-05, CU-06, RF-08..11)**:
   - **Rediseño del carnet de vacunación**: `CarnetPdfViewer.tsx` presenta un pasaporte clínico interactivo con sello médico oficial, soporte de impresión directa vía `window.print()` y descarga del PDF nativo de QuestPDF.
   - Modal `NuevaInmunizacionModal.tsx` con validación temporal estricta (`FechaRefuerzo > FechaAplicacion`).
   - Tablero `RecordatoriosPage.tsx` con segmentación de refuerzos próximos y vencidos, selector de ventana de tiempo y chips KPI.
   - Tarjetas `RecordatorioCard.tsx` con enlaces nativos directos a **WhatsApp Web** (`https://wa.me/57...`) y correo (`mailto:`) con plantillas dinámicas sin costos de mensajería (RN-09).

7. **Fase W-11 — Integración en Kestrel, Pruebas E2E y Empaquetado Monopuesto Local (RNF-07, RNF-08)**:
   - Configuración de `Program.cs` para servir los activos de la SPA (`wwwroot`) y fallback a `index.html`.
   - Incorporación de pruebas de integración E2E en `WebApiE2ETests.cs` utilizando `WebApplicationFactory`.
   - Creación del script de arranque rápido `run-local.ps1` configurado para enlazar Kestrel en `http://localhost:5000`.
   - Sincronización del bundle de producción del frontend en `VetClinic.Api/wwwroot`.

---

## 🏗️ Arquitectura y Stack Tecnológico

```
[ Cliente Navegador Local ] 
      │  (HTTP / localhost:5000)
      ▼
[ Kestrel Web Server (.NET 8) ]
   ├── Static Files (React SPA en wwwroot)
   └── ASP.NET Core Web API (Controllers REST)
            │
            ▼
   [ Servicios de Aplicación / Dominio ]
   (ClinicaService, AuthService, QuestPdfExportService)
            │
            ▼
   [ Infraestructura EF Core 8 + Triggers ]
            │
            ▼
   [ SQLite Embebido (%LocalAppData%\VetClinic\vetclinic_local.db) ]
```

- **Backend API:** ASP.NET Core Web API (.NET 8.0 LTS / C# 12)
- **Persistencia:** Entity Framework Core 8.0 + SQLite (`Microsoft.Data.Sqlite`) con modo WAL
- **Frontend SPA:** React 19 + TypeScript + Vite 8
- **Estilos & UI:** Tailwind CSS + Lucide React + Sonner Toasts
- **Motor de Reportes:** QuestPDF Community License (diseño botánico y generación en memoria)
- **Seguridad:** Hash criptográfico PBKDF2 HMAC-SHA256 (100.000 iteraciones, sal 128 bits)
- **Testing:** xUnit + Test SDK + `Microsoft.AspNetCore.Mvc.Testing`

---

## 📁 Estructura del Repositorio

```text
GeminiSoftware/
│
├── VetClinic.Api/                  # Backend REST Web API y Servidor Local Kestrel
│   ├── Controllers/                # Auth, Propietarios, Pacientes, Atenciones, etc.
│   ├── Dtos/                       # Data Transfer Objects limpios y tipados
│   ├── Properties/                 # launchSettings.json (Puerto 5000)
│   ├── wwwroot/                    # Bundle de producción de la SPA React
│   └── Program.cs                  # Configuración de Kestrel, SPA fallback y pipeline
│
├── VetClinic.Web/                  # Frontend Web SPA (React + TypeScript + Tailwind)
│   ├── src/
│   │   ├── components/             # Layout, Auth, Propietarios, Pacientes, Historia, Vacunación
│   │   ├── context/                # AuthContext (Sesión local y tokens)
│   │   ├── pages/                  # LoginPage, Propietarios, Pacientes, Historias, etc.
│   │   ├── services/               # apiClient.ts (Servicio de API tipado)
│   │   └── types/                  # Modelos de TypeScript alineados al dominio
│   ├── tailwind.config.js          # Tokens de diseño botánico cálido
│   └── vite.config.ts              # Proxy de desarrollo a localhost:5000
│
├── VetClinic.Domain/               # Capa de Dominio (Entidades, Enums, Value Objects e Interfaces)
├── VetClinic.Infrastructure/       # DbContext, SQLite, Triggers Ley 576, PBKDF2 y QuestPDF
├── VetClinic.Domain.Tests/         # Pruebas unitarias de dominio
├── VetClinic.Infrastructure.Tests/ # Pruebas de base de datos SQLite y persistencia
├── VetClinic.Api.Tests/            # Pruebas de controladores y pruebas de integración E2E
│
├── run-local.ps1                   # Script de compilación y lanzamiento en un solo paso
├── fases.md                        # Registro formal de fases y criterios de aceptación
├── GEMINI.md                       # Reglas permanentes y memoria del proyecto
└── README.md                       # Documentación general del repositorio
```

---

## 🚀 Puesta en Marcha y Ejecución Local

### Opción 1: Ejecución con Script Automático (Recomendada)

En una terminal PowerShell en la raíz del proyecto, ejecuta:

```powershell
.\run-local.ps1
```

Este script:
1. Compila la SPA React si no existe o hubo cambios.
2. Sincroniza los archivos generados con `VetClinic.Api/wwwroot`.
3. Inicia Kestrel en el puerto estándar `http://localhost:5000`.

### Opción 2: Ejecución Manual con .NET CLI

```powershell
# 1. Compilar y empaquetar frontend (si aplica)
cd VetClinic.Web
npm run build
cd ..

# 2. Iniciar servidor backend
dotnet run --project VetClinic.Api --urls "http://localhost:5000"
```

### Acceso a la Aplicación

- **Aplicación Web:** [http://localhost:5000](http://localhost:5000)
- **Documentación Swagger / OpenAPI:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Credenciales por defecto:**
  - **Usuario:** `admin`
  - **Contraseña:** `Clinica2026*`

---

## 🧪 Pruebas Automatizadas y Calidad

Para ejecutar la suite completa de pruebas unitarias, de persistencia y de integración E2E:

```powershell
dotnet test VetClinicSolution.sln
```

### Resultados de la Suite de Pruebas:
- **`VetClinic.Domain.Tests`**: **39 superadas** (Cálculo dinámico de edad, formato de celular colombiano `3XXXXXXXXX`, rangos de peso).
- **`VetClinic.Infrastructure.Tests`**: **44 superadas** (Inmutabilidad de triggers SQLite, PBKDF2, DbInitializer y repositorios).
- **`VetClinic.Api.Tests`**: **24 superadas** (Validaciones de endpoints, autenticación, streaming PDF y pruebas E2E `WebApplicationFactory`).
- **Total:** **107 pruebas aprobadas (100% de éxito, 0 fallos)**.

Para compilar y verificar el tipado del frontend Web:
```powershell
cd VetClinic.Web
npm run build
```
- Resultado: **0 errores de compilación TypeScript**, bundle generado en $\approx 2.0$ segundos.

---

## 📜 Marco Legal y Reglas de Negocio Destacadas

| Código | Requisito / Regla | Implementación en la Rama Web |
| :--- | :--- | :--- |
| **Ley 576 de 2000** | Inmutabilidad de Historias Clínicas | Triggers SQLite `TR_AtencionesClinicas_PreventUpdate` y `TR_PreventDelete`. En la interfaz web no existen botones ni rutas para modificar o borrar registros confirmados. |
| **RN-01** | Seguridad de Acceso | Hash criptográfico PBKDF2 HMAC-SHA256 (100.000 iteraciones). |
| **RN-02 / STF-01** | Trazabilidad Nominal Médica | Asignación obligatoria y explícita del veterinario tratante ("Dr. Fabio" o "Dr. William") en cada acto clínico o biológico. |
| **RN-04** | Validación de Celular Colombia | Validación estricta de 10 dígitos numéricos iniciando en `3`. |
| **RN-05 / RN-06** | Peso y Edad Dinámica | El peso se estandariza en kilogramos ($0.01 - 150.00$ Kg) y la edad se calcula dinámicamente en tiempo real (nunca almacenada estática). |
| **RN-08** | Control de Refuerzos | La fecha del próximo refuerzo debe ser estrictamente posterior a la fecha de aplicación. |
| **RN-09 / RNF-09** | Cero Costo de Mensajería | Notificaciones automáticas exclusivas mediante esquemas `https://wa.me/` y `mailto:`, sin cobros de pasarelas ni APIs de terceros. |
| **RNF-07 / RNF-08** | Estación Monopuesto Localhost | Operación autónoma sin internet externa con base de datos SQLite embebida en `%LocalAppData%\VetClinic\vetclinic_local.db`. |

---

## 📚 Documentación Técnica

- [`requisitos.md`](file:///C:/Users/jorsh/Desktop/GeminiSoftware/requisitos.md): Especificación formal de requisitos bajo estándar IEEE 830, reglas de negocio y casos de uso.
- [`diseño.md`](file:///C:/Users/jorsh/Desktop/GeminiSoftware/diseño.md): Especificación de arquitectura técnica, esquemas relacionales SQLite y diseño de componentes.
- [`fases.md`](file:///C:/Users/jorsh/Desktop/GeminiSoftware/fases.md): Plan detallado de fases de ejecución, entregables y criterios verificables completados.
- [`GEMINI.md`](file:///C:/Users/jorsh/Desktop/GeminiSoftware/GEMINI.md): Reglas permanentes del proyecto, decisiones técnicas y memoria de arquitectura.