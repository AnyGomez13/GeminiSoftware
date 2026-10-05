# GEMINI.md - Reglas y Memoria Permanente del Proyecto

## 1. Proyecto
- **Cliente:** Clínica Veterinaria (Dres. Fabio y William).
- **Producto:** VetClinic Pro - Sistema de escritorio para gestión clínica veterinaria.
- **Objetivo:** Administrar pacientes, historias clínicas inmutables, vacunación y recordatorios sin costos de mensajería en una estación monopuesto.

## 2. Restricciones Duras No Negociables
- **Estación monopuesto local:** Base de datos SQLite embebida en `%LocalAppData%`; sin cliente-servidor ni sincronización en la nube (RNF-08, RNF-07).
- **Inmutabilidad legal:** Registros de `AtencionesClinicas` e `Inmunizaciones` son inmutables; bloqueo de `UPDATE` y `DELETE` mediante triggers SQLite (Ley 576 de 2000, RN-07, STF-05).
- **Trazabilidad nominal:** Asignación obligatoria del veterinario tratante ("Dr. Fabio" o "Dr. William") en cada acto clínico o inmunización (RN-02, STF-01).
- **Validación de celular:** Formato estricto para Colombia: 10 dígitos numéricos iniciando en 3 (RN-04).
- **Cero costo de mensajería:** Notificaciones exclusivas vía `https://wa.me/` y `mailto:`, sin pasarelas ni APIs de pago (RN-09, RNF-09, STF-04).
- **Seguridad de credenciales:** Hash con PBKDF2 HMAC-SHA256, sal de 128 bits y 100.000 iteraciones; sin texto plano (RN-01, RNF-01, STF-02).
- **Peso y Edad:** Peso en kilogramos ($0.01$ a $150.00$ Kg); edad calculada dinámicamente en visualización, nunca estática (RN-05, RN-06, STF-06).

## 3. Stack Cerrado
| Tecnología | Versión Exacta | Licencia | Costo |
| :--- | :--- | :--- | :--- |
| .NET | 8.0 LTS | MIT | Gratis |
| C# | 12.0 | MIT | Gratis |
| WPF (.NET Windows Desktop) | 8.0 | MIT | Gratis |
| Entity Framework Core | 8.0 | MIT | Gratis |
| SQLite (Microsoft.Data.Sqlite) | 8.0 | MIT / Public Domain | Gratis |
| QuestPDF | No especificado | QuestPDF Community | Gratis (<$1M USD) |
| Microsoft.Extensions.DependencyInjection | 8.0 | MIT | Gratis |
| xUnit / Test SDK | Por definir en Fase 1 | Apache 2.0 / MIT | Gratis |

## 4. Arquitectura en Síntesis
Arquitectura en 3 capas bajo MVVM estricto e Inyección de Dependencias nativa (`Microsoft.Extensions.DependencyInjection`):
- `VetClinic.Presentation`: Vistas XAML, ViewModels con `ObservableObject`, comandos (`RelayCommand`), validación `INotifyDataErrorInfo` y converters.
- `VetClinic.Domain`: Entidades (`Usuario`, `Veterinario`, `Propietario`, `Paciente`, `AtencionClinica`, `Inmunizacion`), Value Objects, Enums e interfaces.
- `VetClinic.Infrastructure`: `VetClinicDbContext` (SQLite), repositorios, `UnitOfWork`, `DbInitializer`, PBKDF2, QuestPDF y lanzadores URI.
Flujo unidireccional: Vista → ViewModel → Servicio de Dominio → Repositorio/DbContext → SQLite. Detalles en `diseño.md`.

## 5. Convenciones
- **Idiomas:** Código (clases, métodos, variables) en inglés/español conforme a `diseño.md`; UI y mensajes de error 100% español profesional; commits bajo Conventional Commits sin mención a IA.
- **Nombres:** PascalCase para clases, métodos y propiedades (`RegistrarAtencionAsync`); prefijo `_camelCase` en campos privados; prefijo `I` en interfaces; sufijo `Command` en comandos; vistas y viewmodels 1:1 (`XView.xaml` ↔ `XViewModel.cs`). Tablas en plural PascalCase; columnas en PascalCase.
- **Estructura de carpetas:** `VetClinic.Presentation/` (`Views`, `ViewModels`, `Services`, `Styles`), `VetClinic.Domain/` (`Entities`, `Enums`, `Interfaces`, `ValueObjects`), `VetClinic.Infrastructure/` (`Data`, `Repositories`, `Security`, `Services`).
- **Entidades vs Código:** Heredan de `BaseEntity` (`Id`, `CreatedAt`) reflejando fielmente el modelo relacional de `diseño.md`.
- **Trazabilidad en código y pruebas:** Referenciar IDs RF/CU/RNF en comentarios y métodos de prueba (ej. `// RF-01, RN-01`, `[Fact] public void Autenticar_CredencialesInvalidas_RetornaError_RF01()`).

## 6. Reglas de Trabajo
- Por cada tarea, consultar la sección correspondiente en `requisitos.md` y `diseño.md`.
- Ejecutar exclusivamente la fase activa según `fases.md`; no avanzar a fases futuras de forma anticipada.
- Verificar el criterio de terminado antes de dar por cerrada una fase.
- Actualizar el estado en `fases.md` y detenerse a solicitar aprobación del usuario al concluir cada fase.
- Registrar supuestos con justificación de una línea en `fases.md` ante cualquier vacío o ambigüedad no bloqueante.
- Prohibido inventar pantallas, campos, servicios o tecnologías fuera de alcance (facturación, inventarios, nube, etc.).

## 7. Reglas de UI
- Implementación exclusiva de las 9 vistas catalogadas en `diseño.md` (`LoginView`, `ShellView`, `PropietariosView`, `PropietarioModalView`, `PacientesView`, `HistoriaClinicaView`, `NuevaAtencionModalView`, `InmunizacionesView`, `RecordatoriosView`).
- Prohibido agregar pantallas, modales, campos, pestañas o funciones que no figuren en `diseño.md`.
- Respetar paleta (#1B5E20, #2E7D32, #F8F9FA, #212121), tipografía Segoe UI (≥14pt) y contraste ≥4.5:1 (RNF-04).

## 8. Comandos
- **Build:** `dotnet build VetClinicSolution.sln`
- **Test:** `dotnet test VetClinicSolution.sln`
- **Lint / Formato:** `dotnet format VetClinicSolution.sln --verify-no-changes`
- **Ejecución:** `dotnet run --project VetClinic.Presentation`
*(Flags y paquetes específicos de test y lint por consolidar en Fase 1).*

## 9. Índice de Fases
| Fase | Nombre | Estado |
| :--- | :--- | :--- |
| Fase 1 | Setup del Entorno y Estructura de la Solución | Completada |
| Fase 2 | Capa de Dominio (Entidades, Value Objects e Interfaces) | Completada |
| Fase 3 | Capa de Infraestructura y Persistencia (EF Core, SQLite y Seguridad) | Completada |
| Fase 4 | Servicios de Infraestructura (QuestPDF y Lanzadores Externos) | Completada |
| Fase 5 | Capa de Presentación (Infraestructura MVVM, Estilos y Shell) | Completada |
| Fase 6 | Módulo de Autenticación y Control de Acceso (CU-01, RF-01) | Completada |
| Fase 7 | Módulo de Propietarios y Pacientes (CU-02, RF-02..05) | Completada |
| Fase 8 | Módulo de Historia Clínica y Atenciones Médicas (CU-03, CU-04, RF-06, RF-07) | Completada |
| Fase 9 | Módulo de Vacunación, Carnet PDF y Recordatorios Gratuitos (CU-05, CU-06, RF-08..11) | Completada |
| Fase 10 | Pruebas Integrales de Aceptación, Validación RNF y Empaquetado | Completada |

*Detalle completo de dependencias, entregables y criterios en `fases.md`.*
