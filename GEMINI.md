# GEMINI.md - Reglas y Memoria Permanente del Proyecto

## 1. Proyecto
- **Cliente:** Clínica Veterinaria (Dres. Fabio y William).
- **Producto:** VetClinic Pro - Sistema de gestión clínica veterinaria con interfaz web moderna en estación monopuesto local.
- **Objetivo:** Administrar pacientes, historias clínicas inmutables, vacunación y recordatorios sin costos de mensajería ni servidores externos.

## 2. Restricciones Duras No Negociables
- **Estación monopuesto local:** Base de datos SQLite embebida en `%LocalAppData%`; ejecución en `localhost` sin servidores remotos ni sincronización en la nube (RNF-08, RNF-07).
- **Inmutabilidad legal:** Registros de `AtencionesClinicas` e `Inmunizaciones` son inmutables; bloqueo de `UPDATE` y `DELETE` mediante triggers SQLite (Ley 576 de 2000, RN-07, STF-05).
- **Trazabilidad nominal:** Asignación obligatoria del veterinario tratante ("Dr. Fabio" o "Dr. William") en cada acto clínico o inmunización (RN-02, STF-01).
- **Validación de celular:** Formato estricto para Colombia: 10 dígitos numéricos iniciando en 3 (RN-04).
- **Cero costo de mensajería:** Notificaciones exclusivas vía `https://wa.me/` y `mailto:`, sin pasarelas ni APIs de pago (RN-09, RNF-09, STF-04).
- **Seguridad de credenciales:** Hash con PBKDF2 HMAC-SHA256, sal de 128 bits y 100.000 iteraciones; sin texto plano (RN-01, RNF-01, STF-02).
- **Peso y Edad:** Peso en kilogramos ($0.01$ a $150.00$ Kg); edad calculada dinámicamente en visualización, nunca estática (RN-05, RN-06, STF-06).

## 3. Stack Cerrado (Rama Web)
| Capa / Tecnología | Versión / Ecosistema | Licencia | Costo |
| :--- | :--- | :--- | :--- |
| **Backend API** | ASP.NET Core Web API (.NET 8.0 LTS / C# 12.0) | MIT | Gratis |
| **Persistencia** | Entity Framework Core 8.0 + SQLite (Microsoft.Data.Sqlite) | MIT / Public Domain | Gratis |
| **Motor PDF** | QuestPDF Community | QuestPDF Community (<$1M USD) | Gratis |
| **Frontend Web** | React 18 + TypeScript + Vite | MIT | Gratis |
| **Estilos & UI** | Tailwind CSS + Lucide React + Sonner Toasts | MIT | Gratis |
| **Testing** | xUnit (.NET) + Test SDK | Apache 2.0 / MIT | Gratis |

## 4. Arquitectura en Síntesis
Arquitectura desacoplada en estación local:
- `VetClinic.Api`: Endpoints REST en ASP.NET Core (.NET 8), DTOs para evitar ciclos circulares, autenticación, streaming de PDFs y servidor Kestrel en `http://localhost:5000`.
- `VetClinic.Web`: SPA moderna en React + TypeScript + Tailwind CSS con paleta cálida/botánica, búsqueda global (`Ctrl+K`), expedientes 360°, curva de peso y enlaces nativos a WhatsApp Web y correo.
- `VetClinic.Domain`: Entidades (`Usuario`, `Veterinario`, `Propietario`, `Paciente`, `AtencionClinica`, `Inmunizacion`), Value Objects, Enums e interfaces (reutilizadas 100%).
- `VetClinic.Infrastructure`: `VetClinicDbContext` (SQLite), triggers de inmutabilidad, PBKDF2 y QuestPDF (reutilizadas 100%).
Flujo unidireccional: UI React → HTTP/REST JSON → Controlador/Endpoint API → Servicio de Dominio → Repositorio/DbContext → SQLite.

## 5. Convenciones
- **Idiomas:** Código en C# y TypeScript; UI y mensajes de error 100% español profesional; commits bajo Conventional Commits sin mención a IA.
- **Nombres:** PascalCase para clases, métodos y C#; camelCase para TypeScript/React (funciones, variables, hooks); PascalCase para componentes React (`PacienteCard.tsx`); DTOs con sufijo `Dto`.
- **Estructura de carpetas:** `VetClinic.Api/` (`Controllers`, `Dtos`), `VetClinic.Web/` (`src/components`, `src/pages`, `src/services`, `src/types`), `VetClinic.Domain/`, `VetClinic.Infrastructure/`.
- **Trazabilidad en código y pruebas:** Referenciar IDs RF/CU/RNF en comentarios y pruebas (ej. `// RF-01, RN-01`, `// CU-04`).

## 6. Reglas de Trabajo
- Por cada tarea, consultar la sección correspondiente en `requisitos.md` y `diseño.md`.
- Ejecutar exclusivamente la fase activa según `fases.md`; no avanzar a fases futuras de forma anticipada.
- Verificar el criterio de terminado antes de dar por cerrada una fase.
- Actualizar el estado en `fases.md` y detenerse a solicitar aprobación del usuario al concluir cada fase.
- Registrar supuestos con justificación de una línea en `fases.md` ante cualquier vacío o ambigüedad no bloqueante.
- Prohibido inventar pantallas, campos, servicios o tecnologías fuera de alcance (facturación, inventarios, nube, etc.).

## 7. Reglas de UI (Paleta Verde Botánico y Neutros Cálidos)
- **Fondo de aplicación:** Lino / Papel cálido (`#FBF9F5`).
- **Superficie de tarjetas:** Blanco puro (`#FFFFFF`) con bordes sutiles piedra cálida (`#E7E5E4`).
- **Verde Institucional / Sidebar:** Verde Bosque Eucalipto (`#166534` / `#14532D`).
- **Verde Primario de Acción:** Esmeralda Botánico (`#059669` / `#10B981`).
- **Acento Cálido (Alertas/Próximos):** Ámbar Miel (`#F59E0B` / `#D97706`).
- **Tipografía y Textos:** Grafito cálido (`#1C1917` para primario, `#57534E` para secundario).
- **Legibilidad:** Contraste superior a 4.5:1 (RNF-04, WCAG AAA).

## 8. Comandos
- **Build Backend:** `dotnet build VetClinicSolution.sln`
- **Test Backend:** `dotnet test VetClinicSolution.sln`
- **Dev Frontend:** `cd VetClinic.Web && npm run dev`
- **Build Frontend:** `cd VetClinic.Web && npm run build`
- **Ejecución Local API:** `dotnet run --project VetClinic.Api`

## 9. Índice de Fases (Rama Web)
| Fase | Nombre | Estado |
| :--- | :--- | :--- |
| Fase 1 | Setup del Entorno y Estructura de la Solución (Core) | Completada |
| Fase 2 | Capa de Dominio (Entidades, Value Objects e Interfaces) | Completada |
| Fase 3 | Capa de Infraestructura y Persistencia (EF Core, SQLite y Seguridad) | Completada |
| Fase 4 | Servicios de Infraestructura (QuestPDF y Persistencia) | Completada |
| Fase W-05 | Backend Web API y DTOs (`VetClinic.Api`) | Completada |
| Fase W-06 | Setup del Frontend Web y Sistema de Diseño (`VetClinic.Web`) | Completada |
| Fase W-07 | Módulo de Autenticación y Sesión Local (CU-01, RF-01) | Completada |
| Fase W-08 | Módulo de Propietarios y Pacientes (CU-02, CU-03, RF-02..05) | Completada |
| Fase W-09 | Módulo de Historia Clínica, Consulta y Curva de Peso (CU-04, RF-06, RF-07) | Pendiente |
| Fase W-10 | Módulo de Vacunación, Carnet PDF y Recordatorios 1-Clic (CU-05, CU-06, RF-08..11) | Pendiente |
| Fase W-11 | Integración en Kestrel, Pruebas E2E y Empaquetado Monopuesto Local | Pendiente |

*Detalle completo de dependencias, entregables y criterios en `fases.md`.*
