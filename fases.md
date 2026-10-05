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

## Fase 4: Servicios de Infraestructura (QuestPDF y Lanzadores Externos)
- **Objetivo:** Implementar el motor de exportación de carnet digital a PDF mediante QuestPDF y la apertura de hipervínculos del sistema operativo (`wa.me` y `mailto:`) a costo cero.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure/Services/QuestPdfExportService.cs`
  - `VetClinic.Infrastructure/Services/ExternalLauncherService.cs`
  - `VetClinic.Infrastructure.Tests/Services/QuestPdfExportServiceTests.cs`
  - `VetClinic.Infrastructure.Tests/Services/ExternalLauncherServiceTests.cs`
- **Dependencias de fases anteriores:** Fase 3.
- **IDs de requisitos que cubre:** RF-09, RF-10, RF-11, RN-08, RN-09, RNF-06, RNF-09, S-04, STF-03, STF-04.
- **Criterio de terminado verificable:** Prueba automatizada genera un archivo PDF binario válido en disco en tiempo $\le 3$ segundos (RNF-06); pruebas unitarias validan que las URIs generadas para WhatsApp y correo electrónico cumplan la especificación RFC sin alterar caracteres especiales.
- **Estado:** Completada.

## Fase 5: Capa de Presentación (Infraestructura MVVM, Estilos y Shell)
- **Objetivo:** Construir la base de la aplicación WPF con DI nativa, converters XAML, servicios de navegación/diálogos, estilos visuales de consultorio y la ventana principal `ShellView`.
- **Archivos a crear o modificar:**
  - `VetClinic.Presentation/App.xaml`
  - `VetClinic.Presentation/App.xaml.cs`
  - `VetClinic.Presentation/ViewModels/Common/ViewModelBase.cs`
  - `VetClinic.Presentation/ViewModels/Common/RelayCommand.cs`
  - `VetClinic.Presentation/ViewModels/Common/AsyncRelayCommand.cs`
  - `VetClinic.Presentation/Converters/BooleanToVisibilityConverter.cs`
  - `VetClinic.Presentation/Converters/DateFormatConverter.cs`
  - `VetClinic.Presentation/Converters/KilogramsFormatConverter.cs`
  - `VetClinic.Presentation/Services/IDialogService.cs`
  - `VetClinic.Presentation/Services/DialogService.cs`
  - `VetClinic.Presentation/Services/INavigationService.cs`
  - `VetClinic.Presentation/Services/NavigationService.cs`
  - `VetClinic.Presentation/Styles/Colors.xaml`
  - `VetClinic.Presentation/Styles/Typography.xaml`
  - `VetClinic.Presentation/Styles/Controls.xaml`
  - `VetClinic.Presentation/Views/ShellView.xaml`
  - `VetClinic.Presentation/Views/ShellView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/ShellViewModel.cs`
- **Dependencias de fases anteriores:** Fase 4.
- **IDs de requisitos que cubre:** RNF-03, RNF-04, RN-10.
- **Criterio de terminado verificable:** `ShellView` compila y se despliega con su barra de navegación lateral funcional (5 secciones), tipografía Segoe UI $\ge 14$pt y paleta institucional verde sin errores de recursos XAML en tiempo de diseño.
- **Estado:** Completada.

## Fase 6: Módulo de Autenticación y Control de Acceso (CU-01, RF-01)
- **Objetivo:** Implementar la pantalla y lógica de login validando credenciales contra SQLite mediante hash PBKDF2 y dando acceso a la sesión operativa.
- **Archivos a crear o modificar:**
  - `VetClinic.Domain/Services/AuthService.cs`
  - `VetClinic.Presentation/Views/LoginView.xaml`
  - `VetClinic.Presentation/Views/LoginView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/LoginViewModel.cs`
  - `VetClinic.Presentation/Behaviors/PasswordHelper.cs`
- **Dependencias de fases anteriores:** Fase 5.
- **IDs de requisitos que cubre:** RF-01, CU-01, RN-01, RNF-01, RNF-08.
- **Criterio de terminado verificable:** Inicio de sesión con usuario `admin` y clave `Clinica2026*` redirige exitosamente a `ShellView`; contraseña incorrecta despliega mensaje "Usuario o contraseña incorrectos" en rojo y limpia el campo sin permitir acceso.
- **Estado:** Completada.

## Fase 7: Módulo de Propietarios y Pacientes (CU-02, RF-02..05)
- **Objetivo:** Implementar el directorio de acudientes, censo de mascotas, búsqueda multifactor, cálculo dinámico de edad y alta ágil de propietario en el mismo formulario.
- **Archivos a crear o modificar:**
  - `VetClinic.Domain/Services/ClinicaService.cs` (métodos de propietarios y pacientes)
  - `VetClinic.Presentation/Views/PropietariosView.xaml`
  - `VetClinic.Presentation/Views/PropietariosView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/PropietariosViewModel.cs`
  - `VetClinic.Presentation/Views/PropietarioModalView.xaml`
  - `VetClinic.Presentation/Views/PropietarioModalView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/PropietarioModalViewModel.cs`
  - `VetClinic.Presentation/Views/PacientesView.xaml`
  - `VetClinic.Presentation/Views/PacientesView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/PacientesViewModel.cs`
- **Dependencias de fases anteriores:** Fase 6.
- **IDs de requisitos que cubre:** RF-02, RF-03, RF-04, RF-05, CU-02, RN-03, RN-04, RN-05, RN-06, RN-10, S-01, S-02.
- **Criterio de terminado verificable:** Comportamiento observable: creación de paciente vinculando un propietario creado en la misma ventana (`PropietarioModalView`); bloqueo ante teléfonos de menos de 10 dígitos o que no inicien en 3; cálculo y renderizado automático de edad en la ficha en formato verbal.
- **Estado:** Completada.

## Fase 8: Módulo de Historia Clínica y Atenciones Médicas (CU-03, CU-04, RF-06, RF-07)
- **Objetivo:** Implementar la captura rápida de consultas ambulatorias con selección obligatoria del profesional (Dr. Fabio o Dr. William) y el despliegue del expediente inmutable en orden cronológico descendente.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure/Services/ClinicaService.cs` (registro de consultas e historial clínico)
  - `VetClinic.Presentation/Views/HistoriaClinicaView.xaml`
  - `VetClinic.Presentation/Views/HistoriaClinicaView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/HistoriaClinicaViewModel.cs`
  - `VetClinic.Presentation/Views/NuevaAtencionModalView.xaml`
  - `VetClinic.Presentation/Views/NuevaAtencionModalView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/NuevaAtencionViewModel.cs`
- **Dependencias de fases anteriores:** Fase 7.
- **IDs de requisitos que cubre:** RF-06, RF-07, CU-03, CU-04, RN-02, RN-06, RN-07, RN-10, RNF-02, RNF-03, RNF-05, S-03, S-05.
- **Criterio de terminado verificable:** Guardado de atención bloqueado con alerta visual si no se elige veterinario tratante; tiempo de captura clínica completable en $\le 90$ segundos; renderizado de historial con 100 atenciones en $< 1000$ ms en orden cronológico descendente sin opciones de edición/borrado.
- **Estado:** Completada.

## Fase 9: Módulo de Vacunación, Carnet PDF y Recordatorios Gratuitos (CU-05, CU-06, RF-08..11)
- **Objetivo:** Implementar el registro de inmunizaciones/desparasitaciones, generación asíncrona del carnet digital en PDF y panel de recordatorios con despacho a `wa.me` y `mailto:`.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure/Services/ClinicaService.cs` (registro y consulta de vacunas)
  - `VetClinic.Presentation/Views/InmunizacionesView.xaml`
  - `VetClinic.Presentation/Views/InmunizacionesView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/InmunizacionesViewModel.cs`
  - `VetClinic.Presentation/Views/RecordatoriosView.xaml`
  - `VetClinic.Presentation/Views/RecordatoriosView.xaml.cs`
  - `VetClinic.Presentation/ViewModels/RecordatoriosViewModel.cs`
- **Dependencias de fases anteriores:** Fase 8.
- **IDs de requisitos que cubre:** RF-08, RF-09, RF-10, RF-11, CU-05, CU-06, RN-08, RN-09, RNF-06, RNF-07, RNF-09, S-04.
- **Criterio de terminado verificable:** Validación impide registrar refuerzos con fecha anterior o igual a la aplicación; clic en "Descargar Carnet Digital PDF" genera el documento en disco en $\le 3$ segundos; botones de notificación abren el cliente de mensajería/correo con mensaje preformateado sin costos de infraestructura.
- **Estado:** Completada.

## Fase 10: Pruebas Integrales de Aceptación, Validación RNF y Empaquetado
- **Objetivo:** Ejecutar la suite completa de pruebas unitarias/integración, auditar métricas no funcionales y empaquetar el binario autocontenido para Windows.
- **Archivos a crear o modificar:**
  - `VetClinic.Infrastructure.Tests/Integration/EndToEndFlowTests.cs`
  - `VetClinic.Presentation/VetClinic.Presentation.csproj` (configuración de publish)
- **Dependencias de fases anteriores:** Fase 9.
- **IDs de requisitos que cubre:** RNF-01 a RNF-09, todos los RF y RN.
- **Criterio de terminado verificable:** `dotnet test VetClinicSolution.sln` finaliza con 100% de pruebas aprobadas; `dotnet publish VetClinic.Presentation/VetClinic.Presentation.csproj -c Release -r win-x64 --self-contained` genera la carpeta de distribución ejecutable con SQLite embebido.
- **Estado:** Completada.

---

## Supuestos y contradicciones
- **Supuesto 1 (Entorno de compilación y ejecución):** Dado que la capa de presentación utiliza WPF (`net8.0-windows`), se asume que las fases de compilación y empaquetado de la UI se ejecutan en un entorno Windows con el SDK de .NET 8 Desktop instalado, mientras que las capas de Dominio e Infraestructura son portables a cualquier entorno compatible con .NET 8.
- **Supuesto 2 (Proyectos de pruebas no especificados en estructura de carpetas):** Aunque `diseño.md` lista únicamente los 3 proyectos principales en su diagrama de solución, se asume la creación de `VetClinic.Domain.Tests` y `VetClinic.Infrastructure.Tests` bajo xUnit para soportar los criterios de terminado verificables de cada fase.
- **Supuesto 3 (Fragmento inicial residual en diseño.md):** Las líneas 1-18 de `diseño.md` contienen un borrador con nombres `VeterinariaDbContext` y `ProcedimientoClinico`; se asume como vinculante la especificación formal del resto del documento que nombra a la clase `VetClinicDbContext` y a la entidad `AtencionClinica`.
- **Supuesto 4 (Versión de QuestPDF):** Dado que la versión exacta de QuestPDF no está fijada en `diseño.md`, se asume el uso de la versión 2023.12 o 2024.3 bajo licencia comunitaria (`QuestPDF.Settings.License = LicenseType.Community`).
- **Supuesto 5 (Ruta de almacenamiento local de SQLite):** La base de datos SQLite se crea por defecto en `%LocalAppData%\VetClinic\vetclinic_local.db` según la configuración de `VetClinicDbContext` en `diseño.md`, creándose la carpeta automáticamente si no existe.
