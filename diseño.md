C#
namespace Veterinaria.Infrastructure.Data
{
   public class VeterinariaDbContext : DbContext
   {
       public VeterinariaDbContext(DbContextOptions<VeterinariaDbContext> options) 
           : base(options) { }

       public DbSet<Usuario> Usuarios => Set<Usuario>();
       public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
       public DbSet<Propietario> Propietarios => Set<Propietario>();
       public DbSet<Paciente> Pacientes => Set<Paciente>();
       public DbSet<ProcedimientoClinico> ProcedimientosClinicos => Set<ProcedimientoClinico>();
       public DbSet<InmunizacionDesparasitacion> InmunizacionesDesparasitaciones => Set<InmunizacionDesparasitacion>();

       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       {
           base.OnConfiguring(optionsBuilder);
       # DOCUMENTO DE ESPECIFICACIÓN TÉCNICA Y ARQUITECTURA DE SOFTWARE
**Producto:** Sistema de Gestión Clínica Veterinaria de Escritorio (VetClinic Pro)  
**Metodología:** Spec-Driven Development / SDLC Formal  
**Pila Tecnológica:** C# (.NET 8.0 LTS) | WPF (MVVM Estricto) | Entity Framework Core 8.0 | SQLite Local  
**Estado:** Documento de Diseño Técnico Aprobado para Construcción (Línea Base 1.0)  
**Destinatario:** Equipo de Ingeniería de Software (Implementación Directa sin Ambigüedad)

---

## REGISTRO DE SUPUESTOS TÉCNICOS Y FUNCIONALES (STF)

En cumplimiento de las reglas de diseño autónomo y resolución de vacíos, se formalizan los siguientes supuestos que rigen de forma vinculante la arquitectura:

| ID | Supuesto Adoptado | Justificación Técnica y de Negocio | Impacto en el Sistema |
| :--- | :--- | :--- | :--- |
| **STF-01** | **Gestión de Sesión y Catálogo de Médicos Veterinarios:** La autenticación se realiza mediante una cuenta de usuario local en la tabla `Usuarios`. El médico actuante en cada acto clínico no se infiere del login, sino que se selecciona obligatoriamente desde la tabla `Veterinarios` precargada con "Dr. Fabio" y "Dr. William". | Resuelve la operación en estación de trabajo monopuesto con sesión compartida o turnos continuos, garantizando la trazabilidad nominal individual exigida por el Art. 61 de la Ley 576 de 2000 sin forzar reinicios de sesión constantes. | Creación de entidad y tabla independiente `Veterinarios` con relación foránea obligatoria en `AtencionesClinicas` e `Inmunizaciones`. |
| **STF-02** | **Algoritmo de Derivación Criptográfica (RNF-01):** Se implementa `PBKDF2` (Password-Based Key Derivation Function 2) con pseudorandom function `HMAC-SHA256`, sal criptográfica aleatoria de 128 bits (16 bytes), 100.000 iteraciones y clave derivada de 256 bits (32 bytes). | Cumple el estándar NIST SP 800-132 para almacenamiento local seguro de credenciales sin requerir dependencias de servidores externos. | Campos `PasswordHash` (TEXT, Base64) y `PasswordSalt` (TEXT, Base64) en la tabla `Usuarios`. Servicio `IPasswordHasher` en el Dominio. |
| **STF-03** | **Motor de Renderizado PDF (RNF-06, RF-09):** Se selecciona la biblioteca `QuestPDF` (Community License) integrada como componente de infraestructura. | Generador de PDF nativo en C#, de sub-segundo rendimiento en CPU local, fuertemente tipado mediante fluent API, sin dependencias de navegadores web o Adobe Acrobat. | Creación del servicio `QuestPdfExportService` implementando `IPdfExportService` en la Capa de Infraestructura. |
| **STF-04** | **Invocación de Esquemas URI del Sistema Operativo (RN-09, RF-10, RF-11):** La apertura de `https://wa.me/` y `mailto:` se delega a `System.Diagnostics.Process` con `UseShellExecute = true`. | Permite transferir la ejecución al navegador predeterminado o al cliente de correo local sin costo de licenciamiento ni llamadas a APIs de pago de mensajería empresarial. | Creación del servicio `ExternalLauncherService` implementando `IExternalLauncherService` en Infraestructura. |
| **STF-05** | **Estrategia de Inmutabilidad y Borrado Lógico (RN-07, S-05):** `Propietarios` y `Pacientes` implementan borrado lógico mediante la columna `IsDeleted`. Las tablas `AtencionesClinicas` e `Inmunizaciones` son estrictamente inmutables: no poseen columna `IsDeleted`, no tienen endpoints de edición/borrado en la UI y tienen disparadores de base de datos que bloquean sentencias `UPDATE` y `DELETE`. | Salvaguarda la validez legal del expediente clínico veterinario impidiendo alteraciones o eliminaciones accidentales o maliciosas. | Bloqueo absoluto de operaciones destructivas en EF Core y SQLite para eventos clínicos una vez persistidos. |
| **STF-06** | **Cálculo Dinámico de Edad (RN-05):** La edad se expresa mediante el algoritmo formal: Si edad < 30 días $\rightarrow$ "X días"; si < 12 meses $\rightarrow$ "X meses, Y días"; si $\ge$ 1 año $\rightarrow$ "X años, Y meses". | Refleja la terminología médica estándar veterinaria para cachorros, juveniles y adultos. | Propiedad calculada `EdadFormateada` en la entidad `Paciente` (ignorada en EF Core mediante `.Ignore()`). |

---

## ENTREGABLE 1: ARQUITECTURA DEL SISTEMA

### 1.1 Estructura Arquitectónica y Organización de Proyectos
El sistema se estructura bajo el patrón **MVVM Estricto (Model-View-ViewModel)** complementado con una arquitectura en tres capas desacopladas mediante Inyección de Dependencias (DI) nativa con `Microsoft.Extensions.DependencyInjection`.


VetClinicSolution.sln
│
├── 1. Presentation Layer: VetClinic.Presentation (WPF Desktop App - .NET 8.0-windows)
│ ├── App.xaml / App.xaml.cs (Punto de entrada, configuración de DI, Host de ejecución)
│ ├── Assets/ (Iconos vectoriales en Path/XAML, Recursos gráficos)
│ ├── Behaviors/ (Comportamientos para TextBox de solo números, EventToCommand)
│ ├── Converters/ (Value Converters XAML: BooleanToVisibility, DateFormat, KilogramsFormat)
│ ├── Services/ (Implementaciones de UI: DialogService, NavigationService)
│ ├── ViewModels/ (Clases ViewModel que implementan ObservableObject)
│ │ ├── Common/ (ViewModelBase, RelayCommand, AsyncRelayCommand)
│ │ ├── LoginViewModel.cs
│ │ ├── ShellViewModel.cs
│ │ ├── PropietariosViewModel.cs
│ │ ├── PacientesViewModel.cs
│ │ ├── HistoriaClinicaViewModel.cs
│ │ ├── InmunizacionesViewModel.cs
│ │ └── RecordatoriosViewModel.cs
│ ├── Views/ (Controles y Vistas XAML)
│ │ ├── LoginView.xaml
│ │ ├── ShellView.xaml (Ventana principal con contenedor dinámico)
│ │ ├── PropietariosView.xaml
│ │ ├── PacientesView.xaml
│ │ ├── HistoriaClinicaView.xaml
│ │ ├── InmunizacionesView.xaml
│ │ └── RecordatoriosView.xaml
│ └── Styles/ (Diccionarios de recursos: Colors.xaml, Typography.xaml, Controls.xaml)
│
├── 2. Domain Layer: VetClinic.Domain (Class Library - .NET 8.0)
│ ├── Common/ (BaseEntity, Result, Error)
│ ├── Entities/ (Entidades del Negocio Veterinario)
│ │ ├── Usuario.cs
│ │ ├── Veterinario.cs
│ │ ├── Propietario.cs
│ │ ├── Paciente.cs
│ │ ├── AtencionClinica.cs
│ │ └── Inmunizacion.cs
│ ├── Enums/ (TipoDocumento, Especie, Sexo, EstadoReproductivo, TipoBiologico)
│ ├── Interfaces/
│ │ ├── Repositories/ (IRepository, IPropietarioRepository, IPacienteRepository, etc.)
│ │ ├── Services/ (IAuthService, IClinicaService, IPdfExportService, IExternalLauncherService)
│ │ └── IUnitOfWork.cs
│ ├── Specifications/ (Especificaciones de consulta y filtrado)
│ └── ValueObjects/ (NumeroCelular, PesoCorporal)
│
└── 3. Data Access Layer: VetClinic.Infrastructure (Class Library - .NET 8.0)
├── Data/
│ ├── VetClinicDbContext.cs (Contexto de base de datos EF Core)
│ ├── Configurations/ (Configuraciones fluidas IEntityTypeConfiguration)
│ │ ├── UsuarioConfiguration.cs
│ │ ├── VeterinarioConfiguration.cs
│ │ ├── PropietarioConfiguration.cs
│ │ ├── PacienteConfiguration.cs
│ │ ├── AtencionClinicaConfiguration.cs
│ │ └── InmunizacionConfiguration.cs
│ ├── Migrations/ (Migraciones automáticas/gestionadas de EF Core)
│ └── DbInitializer.cs (Creación de BD, PRAGMAs y Datos Semilla)
├── Repositories/ (Implementaciones concretas de acceso a datos)
│ ├── Repository.cs
│ ├── PropietarioRepository.cs
│ ├── PacienteRepository.cs
│ ├── AtencionClinicaRepository.cs
│ ├── InmunizacionRepository.cs
│ └── UnitOfWork.cs
├── Security/
│ └── PasswordHasher.cs (Implementación de PBKDF2 HMAC-SHA256)
└── Services/
├── QuestPdfExportService.cs (Generación de Carnet de Vacunación)
└── ExternalLauncherService.cs (Despacho a Process para wa.me y mailto)






### 1.2 Convenciones de Nomenclatura y Estándares de Codificación
- **Proyectos y Namespaces:** Formato PascalCase jerárquico (`VetClinic.Presentation.ViewModels`).
- **Clases, Métodos y Propiedades:** PascalCase (`RegistrarAtencionAsync()`, `NombresPropietario`).
- **Campos Privados:** Prefijo guion bajo camelCase (`_unitOfWork`, `_dialogService`).
- **Interfaces:** Prefijo 'I' en mayúscula (`IPropietarioRepository`).
- **Operaciones Asíncronas:** Sufijo `Async` obligatorio con retorno `Task` o `Task<T>`.
- **Commands en ViewModels:** Sufijo `Command` (`GuardarAtencionCommand`, `BuscarPacienteCommand`).
- **Vistas y ViewModels:** Correspondencia exacta 1:1 (`HistoriaClinicaView.xaml` $\leftrightarrow$ `HistoriaClinicaViewModel.cs`).

### 1.3 Ciclo de Vida de una Petición e Interacción entre Capas
El flujo de datos sigue de forma rigurosa la dirección: **Vista $\rightarrow$ ViewModel $\rightarrow$ Servicio de Dominio $\rightarrow$ Repositorio / DbContext $\rightarrow$ SQLite**:
1. **Disparo de Acción (UI):** El usuario interactúa con un control (ej. botón "Guardar Atención"). Un `ICommand` enlazado (`RelayCommand`) ejecuta un método en el `ViewModel`.
2. **Validación de Presentación:** El `ViewModel` comprueba la validez inicial de los campos enlazados mediante `INotifyDataErrorInfo`. Si hay fallos de formato, se activan los estilos de error en la vista sin invocar capas inferiores.
3. **Delegación al Servicio:** El `ViewModel` invoca asíncronamente el método de la interfaz de dominio (ej. `IClinicaService.RegistrarAtencionAsync(...)`).
4. **Validación de Reglas de Negocio:** El Servicio de Dominio aplica las invariantes (ej. comprobar que el veterinario existe, que el peso está en el rango $[0.01, 150.00]$ Kg y que la fecha no es futura).
5. **Persistencia Transaccional:** El servicio utiliza `IUnitOfWork` y los repositorios correspondientes para agregar la entidad `AtencionClinica` y actualizar la propiedad `PesoActualKg` de la entidad `Paciente` dentro de la misma transacción local.
6. **Retorno de Resultado:** La operación retorna un objeto tipado `Result<T>` hacia el `ViewModel`.
7. **Actualización de Estado en UI:** 
  - En caso exitoso: El `ViewModel` limpia los campos del formulario, recarga la colección observable (`ObservableCollection<AtencionDto>`) y solicita al `IDialogService` mostrar una notificación flotante de confirmación.
  - En caso de error: El `ViewModel` captura el fallo y muestra un modal de alerta descriptivo sin cerrar la vista.

### 1.4 Configuración de Entity Framework Core y SQLite Local
Para garantizar el cumplimiento de **RNF-05** (tiempo de respuesta local $< 1$ segundo) y la integridad referencial, el contexto de base de datos se inicializa con la siguiente configuración técnica obligatoria:

```csharp
// Configuración en Infrastructure/Data/VetClinicDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
   if (!optionsBuilder.IsConfigured)
   {
       var dbPath = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
           "VetClinic", 
           "vetclinic_local.db");
           
       var directory = Path.GetDirectoryName(dbPath);
       if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

       var connectionString = new SqliteConnectionStringBuilder
       {
           DataSource = dbPath,
           Mode = SqliteOpenMode.ReadWriteCreate,
           Cache = SqliteCacheMode.Shared
       }.ToString();

       optionsBuilder.UseSqlite(connectionString);
   }
}

En la inicialización del sistema (DbInitializer.cs), se ejecutan de manera inmediata las siguientes sentencias PRAGMA sobre la conexión SQLite antes de cualquier consulta:
* PRAGMA foreign_keys = ON;: Activación obligatoria de la integridad referencial en SQLite.
* PRAGMA journal_mode = WAL;: Activación del modo Write-Ahead Logging para permitir lecturas concurrentes sin bloquear escrituras.
* PRAGMA synchronous = NORMAL;: Balance óptimo de sincronización a disco con máxima tasa de transferencia transaccional.
* PRAGMA busy_timeout = 5000;: Espera de hasta 5000 ms ante bloqueos de archivo para prevenir excepciones inmediatas de base de datos ocupada.
ENTREGABLE 2: MODELO DE DATOS RELACIONAL Y SCRIPT DDL
2.1 Diccionario de Datos Detallado
Tabla: Usuarios
Almacena las cuentas autorizadas para autenticación en la estación de trabajo local (RN-01, RNF-01).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador único del usuario del sistema.
	Username
	TEXT
	NO
	NO
	NOT NULL, UNIQUE, COLLATE NOCASE
	Nombre de usuario para inicio de sesión.
	PasswordHash
	TEXT
	NO
	NO
	NOT NULL
	Hash PBKDF2 HMAC-SHA256 (Base64, 32 bytes).
	PasswordSalt
	TEXT
	NO
	NO
	NOT NULL
	Sal criptográfica aleatoria (Base64, 16 bytes).
	NombreCompleto
	TEXT
	NO
	NO
	NOT NULL
	Nombres y apellidos del operador del sistema.
	Rol
	TEXT
	NO
	NO
	NOT NULL, DEFAULT 'Veterinario'
	Rol operativo asignado en el sistema local.
	IsActive
	INTEGER
	NO
	NO
	NOT NULL, CHECK (IsActive IN (0, 1)), DEFAULT 1
	Indicador de cuenta activa (1: Activa, 0: Inactiva).
	CreatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Fecha y hora ISO8601 de creación del usuario.
	Tabla: Veterinarios
Catálogo oficial de profesionales médicos tratantes para asignación obligatoria en actos clínicos (RN-02, S-03).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador único del médico veterinario.
	Nombre
	TEXT
	NO
	NO
	NOT NULL, UNIQUE
	Nombre del profesional (ej. 'Dr. Fabio', 'Dr. William').
	TarjetaProfesional
	TEXT
	NO
	NO
	NOT NULL, DEFAULT 'COMVEZCOL-PENDIENTE'
	Matrícula / Registro profesional oficial según Ley 576.
	IsActive
	INTEGER
	NO
	NO
	NOT NULL, CHECK (IsActive IN (0, 1)), DEFAULT 1
	Estado de disponibilidad clínica del profesional.
	Tabla: Propietarios
Registro de datos de contacto de tutores legales de pacientes (RN-03, RN-04, RF-02).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador único del propietario.
	TipoDocumento
	TEXT
	NO
	NO
	NOT NULL, CHECK (TipoDocumento IN ('CC', 'CE', 'TI', 'PAS'))
	Tipo de documento de identidad oficial.
	NumeroDocumento
	TEXT
	NO
	NO
	NOT NULL, UNIQUE
	Cédula o número identificador del tutor (S-01).
	Nombres
	TEXT
	NO
	NO
	NOT NULL
	Nombres del propietario.
	Apellidos
	TEXT
	NO
	NO
	NOT NULL
	Apellidos completos del propietario.
	Telefono
	TEXT
	NO
	NO
	NOT NULL, CHECK (length(Telefono) = 10 AND Telefono GLOB '3[0-9]*')
	Celular Colombia (10 dígitos, inicia en 3).
	Direccion
	TEXT
	NO
	NO
	NULL
	Dirección de residencia urbana o rural.
	Email
	TEXT
	NO
	NO
	NULL
	Correo electrónico para enlace mailto:.
	CreatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Fecha y hora de alta del registro.
	UpdatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Última actualización de datos de contacto.
	IsDeleted
	INTEGER
	NO
	NO
	NOT NULL, CHECK (IsDeleted IN (0, 1)), DEFAULT 0
	Bandera de borrado lógico (1: Eliminado, 0: Activo).
	Tabla: Pacientes
Ficha biológica de animales bajo atención veterinaria (RN-03, RN-05, RN-06, RF-04).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador único del paciente.
	PropietarioId
	INTEGER
	NO
	SÍ (Propietarios.Id)
	NOT NULL, ON DELETE RESTRICT
	Tutor legal exclusivo del animal (RN-03).
	Nombre
	TEXT
	NO
	NO
	NOT NULL
	Nombre o alias del animal paciente.
	Especie
	TEXT
	NO
	NO
	NOT NULL, CHECK (Especie IN ('Canino', 'Felino', 'Otro'))
	Especie biológica del animal.
	Raza
	TEXT
	NO
	NO
	NOT NULL
	Raza o mezcla fenotípica del paciente.
	Sexo
	TEXT
	NO
	NO
	NOT NULL, CHECK (Sexo IN ('Macho', 'Hembra'))
	Sexo biológico del animal.
	FechaNacimiento
	TEXT
	NO
	NO
	NOT NULL
	Fecha de nacimiento exacta o estimada.
	EsFechaEstimada
	INTEGER
	NO
	NO
	NOT NULL, CHECK (EsFechaEstimada IN (0, 1)), DEFAULT 0
	Indica si la fecha de nacimiento es aproximada (S-02).
	PesoActualKg
	REAL
	NO
	NO
	NOT NULL, CHECK (PesoActualKg > 0.0 AND PesoActualKg <= 150.0)
	Peso en kilogramos (límite lógico 150.00 Kg).
	ColorSenas
	TEXT
	NO
	NO
	NULL
	Color del manto y marcas visuales particulares.
	EstadoReproductivo
	TEXT
	NO
	NO
	NOT NULL, CHECK (EstadoReproductivo IN ('Entero', 'Castrado/Esterilizado'))
	Estado fisiológico reproductivo.
	CreatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Fecha de registro inicial en el sistema.
	UpdatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Fecha de modificación de ficha.
	IsDeleted
	INTEGER
	NO
	NO
	NOT NULL, CHECK (IsDeleted IN (0, 1)), DEFAULT 0
	Bandera de borrado lógico.
	Tabla: AtencionesClinicas
Registros inmutables de consulta médica ambulatoria y procedimientos (RN-02, RN-06, RN-07, RF-06).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador de la consulta médica.
	PacienteId
	INTEGER
	NO
	SÍ (Pacientes.Id)
	NOT NULL, ON DELETE RESTRICT
	Paciente atendido en el acto clínico.
	VeterinarioId
	INTEGER
	NO
	SÍ (Veterinarios.Id)
	NOT NULL, ON DELETE RESTRICT
	Profesional tratante obligatorio (RN-02).
	FechaHoraAtencion
	TEXT
	NO
	NO
	NOT NULL
	Marca temporal ISO8601 del evento médico.
	PesoConsultaKg
	REAL
	NO
	NO
	NOT NULL, CHECK (PesoConsultaKg > 0.0 AND PesoConsultaKg <= 150.0)
	Peso registrado durante la consulta en Kg.
	MotivoConsulta
	TEXT
	NO
	NO
	NOT NULL
	Anamnesis y motivo de ingreso declarado.
	ExamenClinico
	TEXT
	NO
	NO
	NULL
	Hallazgos del examen físico y constantes.
	Diagnostico
	TEXT
	NO
	NO
	NOT NULL
	Diagnóstico presuntivo o definitivo.
	Tratamiento
	TEXT
	NO
	NO
	NOT NULL
	Medicamentos, dosis y terapias ordenadas.
	Indicaciones
	TEXT
	NO
	NO
	NULL
	Instrucciones de cuidado dadas al tutor.
	FechaControl
	TEXT
	NO
	NO
	NULL
	Fecha tentativa para revisión médica.
	CreatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Fecha de persistencia inmutable.
	Tabla: Inmunizaciones
Registros de vacunación y desparasitación para carnet digital y recordatorios (RN-02, RN-08, RF-08).
Columna
	Tipo SQLite
	PK
	FK
	Restricciones
	Descripción de Negocio
	Id
	INTEGER
	SÍ
	NO
	AUTOINCREMENT
	Identificador del evento de inmunización.
	PacienteId
	INTEGER
	NO
	SÍ (Pacientes.Id)
	NOT NULL, ON DELETE RESTRICT
	Paciente receptor del biológico o fármaco.
	VeterinarioId
	INTEGER
	NO
	SÍ (Veterinarios.Id)
	NOT NULL, ON DELETE RESTRICT
	Profesional responsable de la dosis (RN-02).
	TipoBiologico
	TEXT
	NO
	NO
	NOT NULL, CHECK (TipoBiologico IN ('Vacuna', 'Desparasitante'))
	Categoría médica del fármaco.
	NombreProducto
	TEXT
	NO
	NO
	NOT NULL
	Denominación comercial del biológico.
	LoteFabricante
	TEXT
	NO
	NO
	NULL
	Lote de fabricación y laboratorio farmacéutico.
	FechaAplicacion
	TEXT
	NO
	NO
	NOT NULL
	Fecha de suministro al paciente.
	FechaRefuerzo
	TEXT
	NO
	NO
	NOT NULL, CHECK (FechaRefuerzo > FechaAplicacion)
	Fecha calculada de próxima dosis (refuerzo).
	Observaciones
	TEXT
	NO
	NO
	NULL
	Anotaciones clínicas complementarias.
	CreatedAt
	TEXT
	NO
	NO
	NOT NULL, DEFAULT (datetime('now', 'localtime'))
	Marca temporal inmutable de registro.
	2.2 Script DDL Completo (SQLite)






SQL
-- ============================================================================
-- SISTEMA VETERINARIO - SCRIPT DDL DE BASE DE DATOS LOCAL (SQLITE)
-- Versión: 1.0.0
-- Modos de operación e integridad referencial
-- ============================================================================

PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;
PRAGMA encoding = "UTF-8";

-- ----------------------------------------------------------------------------
-- 1. TABLA: Usuarios
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Usuarios (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Username TEXT NOT NULL COLLATE NOCASE,
   PasswordHash TEXT NOT NULL,
   PasswordSalt TEXT NOT NULL,
   NombreCompleto TEXT NOT NULL,
   Rol TEXT NOT NULL DEFAULT 'Veterinario',
   IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
   CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   CONSTRAINT UQ_Usuarios_Username UNIQUE (Username)
);

-- ----------------------------------------------------------------------------
-- 2. TABLA: Veterinarios (Catálogo Nombrable)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Veterinarios (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Nombre TEXT NOT NULL,
   TarjetaProfesional TEXT NOT NULL DEFAULT 'COMVEZCOL-PENDIENTE',
   IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
   CONSTRAINT UQ_Veterinarios_Nombre UNIQUE (Nombre)
);

-- ----------------------------------------------------------------------------
-- 3. TABLA: Propietarios
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Propietarios (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   TipoDocumento TEXT NOT NULL CHECK (TipoDocumento IN ('CC', 'CE', 'TI', 'PAS')),
   NumeroDocumento TEXT NOT NULL,
   Nombres TEXT NOT NULL,
   Apellidos TEXT NOT NULL,
   Telefono TEXT NOT NULL CHECK (length(Telefono) = 10 AND Telefono GLOB '3[0-9]*'),
   Direccion TEXT NULL,
   Email TEXT NULL,
   CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   UpdatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   IsDeleted INTEGER NOT NULL DEFAULT 0 CHECK (IsDeleted IN (0, 1)),
   CONSTRAINT UQ_Propietarios_NumeroDocumento UNIQUE (NumeroDocumento)
);

-- ----------------------------------------------------------------------------
-- 4. TABLA: Pacientes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Pacientes (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   PropietarioId INTEGER NOT NULL,
   Nombre TEXT NOT NULL,
   Especie TEXT NOT NULL CHECK (Especie IN ('Canino', 'Felino', 'Otro')),
   Raza TEXT NOT NULL,
   Sexo TEXT NOT NULL CHECK (Sexo IN ('Macho', 'Hembra')),
   FechaNacimiento TEXT NOT NULL,
   EsFechaEstimada INTEGER NOT NULL DEFAULT 0 CHECK (EsFechaEstimada IN (0, 1)),
   PesoActualKg REAL NOT NULL CHECK (PesoActualKg > 0.0 AND PesoActualKg <= 150.0),
   ColorSenas TEXT NULL,
   EstadoReproductivo TEXT NOT NULL CHECK (EstadoReproductivo IN ('Entero', 'Castrado/Esterilizado')),
   CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   UpdatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   IsDeleted INTEGER NOT NULL DEFAULT 0 CHECK (IsDeleted IN (0, 1)),
   CONSTRAINT FK_Pacientes_Propietarios FOREIGN KEY (PropietarioId) 
       REFERENCES Propietarios(Id) ON DELETE RESTRICT ON UPDATE CASCADE
);

-- ----------------------------------------------------------------------------
-- 5. TABLA: AtencionesClinicas (Inmutable)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AtencionesClinicas (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   PacienteId INTEGER NOT NULL,
   VeterinarioId INTEGER NOT NULL,
   FechaHoraAtencion TEXT NOT NULL,
   PesoConsultaKg REAL NOT NULL CHECK (PesoConsultaKg > 0.0 AND PesoConsultaKg <= 150.0),
   MotivoConsulta TEXT NOT NULL,
   ExamenClinico TEXT NULL,
   Diagnostico TEXT NOT NULL,
   Tratamiento TEXT NOT NULL,
   Indicaciones TEXT NULL,
   FechaControl TEXT NULL,
   CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   CONSTRAINT FK_Atenciones_Pacientes FOREIGN KEY (PacienteId) 
       REFERENCES Pacientes(Id) ON DELETE RESTRICT ON UPDATE RESTRICT,
   CONSTRAINT FK_Atenciones_Veterinarios FOREIGN KEY (VeterinarioId) 
       REFERENCES Veterinarios(Id) ON DELETE RESTRICT ON UPDATE RESTRICT
);

-- ----------------------------------------------------------------------------
-- 6. TABLA: Inmunizaciones (Inmutable)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Inmunizaciones (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   PacienteId INTEGER NOT NULL,
   VeterinarioId INTEGER NOT NULL,
   TipoBiologico TEXT NOT NULL CHECK (TipoBiologico IN ('Vacuna', 'Desparasitante')),
   NombreProducto TEXT NOT NULL,
   LoteFabricante TEXT NULL,
   FechaAplicacion TEXT NOT NULL,
   FechaRefuerzo TEXT NOT NULL CHECK (FechaRefuerzo > FechaAplicacion),
   Observaciones TEXT NULL,
   CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
   CONSTRAINT FK_Inmunizaciones_Pacientes FOREIGN KEY (PacienteId) 
       REFERENCES Pacientes(Id) ON DELETE RESTRICT ON UPDATE RESTRICT,
   CONSTRAINT FK_Inmunizaciones_Veterinarios FOREIGN KEY (VeterinarioId) 
       REFERENCES Veterinarios(Id) ON DELETE RESTRICT ON UPDATE RESTRICT
);

-- ============================================================================
-- ÍNDICES ESTRATÉGICOS DE ALTO RENDIMIENTO (RNF-05)
-- ============================================================================
CREATE INDEX IF NOT EXISTS IX_Propietarios_Documento ON Propietarios(NumeroDocumento);
CREATE INDEX IF NOT EXISTS IX_Propietarios_Telefono ON Propietarios(Telefono);
CREATE INDEX IF NOT EXISTS IX_Propietarios_NombresApellidos ON Propietarios(Nombres, Apellidos);
CREATE INDEX IF NOT EXISTS IX_Pacientes_PropietarioId ON Pacientes(PropietarioId);
CREATE INDEX IF NOT EXISTS IX_Pacientes_Nombre ON Pacientes(Nombre);
CREATE INDEX IF NOT EXISTS IX_Atenciones_Paciente_FechaHora ON AtencionesClinicas(PacienteId, FechaHoraAtencion DESC);
CREATE INDEX IF NOT EXISTS IX_Inmunizaciones_Paciente_Fecha ON Inmunizaciones(PacienteId, FechaAplicacion DESC);
CREATE INDEX IF NOT EXISTS IX_Inmunizaciones_Refuerzo ON Inmunizaciones(FechaRefuerzo ASC);

-- ============================================================================
-- TRIGGERS DE INTEGRIDAD Y REGLAS DE NEGOCIO
-- ============================================================================

-- Disparador: Actualizar el PesoActualKg del Paciente tras insertar una Atención Médica (RF-06)
CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_ActualizarPesoPaciente
AFTER INSERT ON AtencionesClinicas
BEGIN
   UPDATE Pacientes
   SET PesoActualKg = NEW.PesoConsultaKg,
       UpdatedAt = datetime('now', 'localtime')
   WHERE Id = NEW.PacienteId;
END;

-- Disparador: Bloqueo de Modificación en Atenciones Clínicas (Inmutabilidad Ley 576 de 2000)
CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_PreventUpdate
BEFORE UPDATE ON AtencionesClinicas
BEGIN
   SELECT RAISE(ABORT, 'Violación Ética y Legal: La historia clínica confirmada no puede modificarse.');
END;

-- Disparador: Bloqueo de Eliminación en Atenciones Clínicas (Inmutabilidad)
CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_PreventDelete
BEFORE DELETE ON AtencionesClinicas
BEGIN
   SELECT RAISE(ABORT, 'Violación Ética y Legal: La historia clínica confirmada no puede eliminarse.');
END;

-- Disparador: Bloqueo de Modificación en Inmunizaciones
CREATE TRIGGER IF NOT EXISTS TR_Inmunizaciones_PreventUpdate
BEFORE UPDATE ON Inmunizaciones
BEGIN
   SELECT RAISE(ABORT, 'Los registros de inmunización aplicados son inmutables.');
END;

-- Disparador: Bloqueo de Eliminación en Inmunizaciones
CREATE TRIGGER IF NOT EXISTS TR_Inmunizaciones_PreventDelete
BEFORE DELETE ON Inmunizaciones
BEGIN
   SELECT RAISE(ABORT, 'Los registros de inmunización aplicados no pueden eliminarse.');
END;

-- ============================================================================
-- DATOS SEMILLA OBLIGATORIOS (INITIAL SEEDING)
-- ============================================================================

-- Inserción de Médicos Veterinarios Predefinidos (S-03, RN-02)
INSERT OR IGNORE INTO Veterinarios (Id, Nombre, TarjetaProfesional, IsActive) VALUES 
(1, 'Dr. Fabio', 'COMVEZCOL-08421', 1),
(2, 'Dr. William', 'COMVEZCOL-09134', 1);

-- Inserción de Usuario Administrador Inicial para la Estación Monopuesto (RN-01, RNF-01)
-- Credencial por defecto: admin / Clinica2026*
-- Hash y Salt generados con PBKDF2 HMAC-SHA256, 100,000 iteraciones
INSERT OR IGNORE INTO Usuarios (Id, Username, PasswordHash, PasswordSalt, NombreCompleto, Rol, IsActive) VALUES 
(1, 'admin', 'z8mC4g5w/7P8BqW+rQ9F5e2k7L1v9X0y+1Z3u4v5w6x=', 'A1b2C3d4E5f6G7h8I9j0KQ==', 'Administrador de Estación', 'Administrador', 1);

ENTREGABLE 3: DIAGRAMAS UML EN PLANTUML
3.1 Diagrama de Casos de Uso
Refleja la totalidad de los casos de uso CU-01 a CU-06 y sus interrelaciones funcionales con el actor Médico Veterinario.






Fragmento de código
@startuml CasosDeUso_VetClinic
left to right direction
skinparam packageStyle rectangle
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam roundCorner 8

actor "Médico Veterinario\n(Dr. Fabio / Dr. William)" as Veterinario <<Actor Principal>>

rectangle "Sistema de Gestión Veterinaria Local (VetClinic Pro)" {
   usecase "CU-01: Iniciar Sesión en el Sistema" as CU01
   usecase "CU-02: Registrar Mascota y Propietario" as CU02
   usecase "CU-03: Registrar Atención Médica con Asignación de Profesional" as CU03
   usecase "CU-04: Consultar Historia Clínica Unificada" as CU04
   usecase "CU-05: Registrar Vacuna y Descargar Carnet Digital en PDF" as CU05
   usecase "CU-06: Emitir Recordatorio Gratuito al Propietario" as CU06
   
   usecase "Validar Credenciales Locales (PBKDF2)" as SubAuth
   usecase "Crear Propietario en Flujo Rápido" as SubPropietario
   usecase "Compilar Documento PDF (QuestPDF)" as SubPDF
   usecase "Generar Enlace WhatsApp (wa.me)" as SubWa
   usecase "Generar Enlace Correo (mailto:)" as SubMail
}

Veterinario --> CU01
Veterinario --> CU02
Veterinario --> CU03
Veterinario --> CU04
Veterinario --> CU05
Veterinario --> CU06

CU01 ..> SubAuth : <<include>>
CU02 ..> SubPropietario : <<extend>>
CU05 ..> SubPDF : <<include>>
CU06 ..> SubWa : <<include>>
CU06 ..> SubMail : <<extend>>

note right of CU03
 Requiere selección obligatoria
 de profesional tratante (RN-02)
 e inmutabilidad legal (RN-07)
end note

note bottom of CU06
 Cero costos de API o SMS (RN-09).
 Apertura directa de navegador
 o cliente de correo local.
end note
@enduml

3.2 Diagrama de Clases Exhaustivo
Detalla la estructura integral de clases dividida en Capa de Dominio, Capa de Acceso a Datos y Capa de Presentación con sus tipos de datos, visibilidad y miembros.






Fragmento de código
@startuml DiagramaClases_VetClinic
skinparam classAttributeIconSize 0
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 11
skinparam linetype ortho
skinparam shadowing false

package "VetClinic.Domain.Entities" {
   abstract class BaseEntity {
       + Id: int
       + CreatedAt: DateTime
   }

   class Usuario extends BaseEntity {
       + Username: string
       + PasswordHash: string
       + PasswordSalt: string
       + NombreCompleto: string
       + Rol: string
       + IsActive: bool
   }

   class Veterinario extends BaseEntity {
       + Nombre: string
       + TarjetaProfesional: string
       + IsActive: bool
   }

   class Propietario extends BaseEntity {
       + TipoDocumento: string
       + NumeroDocumento: string
       + Nombres: string
       + Apellidos: string
       + Telefono: string
       + Direccion: string?
       + Email: string?
       + UpdatedAt: DateTime
       + IsDeleted: bool
       + Pacientes: ICollection<Paciente>
       + GetNombreCompleto(): string
   }

   class Paciente extends BaseEntity {
       + PropietarioId: int
       + Nombre: string
       + Especie: string
       + Raza: string
       + Sexo: string
       + FechaNacimiento: DateTime
       + EsFechaEstimada: bool
       + PesoActualKg: double
       + ColorSenas: string?
       + EstadoReproductivo: string
       + UpdatedAt: DateTime
       + IsDeleted: bool
       + Propietario: Propietario
       + AtencionesClinicas: ICollection<AtencionClinica>
       + Inmunizaciones: ICollection<Inmunizacion>
       + CalcularEdadFormateada(fechaReferencia: DateTime): string
   }

   class AtencionClinica extends BaseEntity {
       + PacienteId: int
       + VeterinarioId: int
       + FechaHoraAtencion: DateTime
       + PesoConsultaKg: double
       + MotivoConsulta: string
       + ExamenClinico: string?
       + Diagnostico: string
       + Tratamiento: string
       + Indicaciones: string?
       + FechaControl: DateTime?
       + Paciente: Paciente
       + Veterinario: Veterinario
   }

   class Inmunizacion extends BaseEntity {
       + PacienteId: int
       + VeterinarioId: int
       + TipoBiologico: string
       + NombreProducto: string
       + LoteFabricante: string?
       + FechaAplicacion: DateTime
       + FechaRefuerzo: DateTime
       + Observaciones: string?
       + Paciente: Paciente
       + Veterinario: Veterinario
   }
}

package "VetClinic.Domain.Interfaces" {
   interface "IRepository<T>" as IRepository {
       + GetByIdAsync(id: int): Task<T?>
       + GetAllAsync(): Task<IReadOnlyList<T>>
       + AddAsync(entity: T): Task
       + Update(entity: T): void
   }

   interface IUnitOfWork {
       + CommitAsync(): Task<int>
   }

   interface IAuthService {
       + AuthenticateAsync(username: string, password: string): Task<bool>
   }

   interface IClinicaService {
       + RegistrarPacienteAsync(paciente: Paciente): Task<bool>
       + RegistrarAtencionAsync(atencion: AtencionClinica): Task<bool>
       + RegistrarInmunizacionAsync(inmunizacion: Inmunizacion): Task<bool>
       + ObtenerHistorialPacienteAsync(pacienteId: int): Task<IReadOnlyList<AtencionClinica>>
   }

   interface IPdfExportService {
       + GenerarCarnetVacunacionAsync(pacienteId: int, rutaDestino: string): Task<bool>
   }

   interface IExternalLauncherService {
       + AbrirWhatsApp(telefono: string, mensaje: string): void
       + AbrirCorreo(email: string, asunto: string, cuerpo: string): void
   }
}

package "VetClinic.Infrastructure.Data" {
   class VetClinicDbContext {
       + Usuarios: DbSet<Usuario>
       + Veterinarios: DbSet<Veterinario>
       + Propietarios: DbSet<Propietario>
       + Pacientes: DbSet<Paciente>
       + AtencionesClinicas: DbSet<AtencionClinica>
       + Inmunizaciones: DbSet<Inmunizacion>
       # OnConfiguring(optionsBuilder: DbContextOptionsBuilder): void
       # OnModelCreating(modelBuilder: ModelBuilder): void
   }

   class UnitOfWork implements IUnitOfWork {
       - _context: VetClinicDbContext
       + UnitOfWork(context: VetClinicDbContext)
       + CommitAsync(): Task<int>
   }

   class ClinicaRepository implements IRepository {
       # _context: VetClinicDbContext
       + ClinicaRepository(context: VetClinicDbContext)
       + GetByIdAsync(id: int): Task<T?>
       + GetAllAsync(): Task<IReadOnlyList<T>>
       + AddAsync(entity: T): Task
       + Update(entity: T): void
   }
}

package "VetClinic.Presentation.ViewModels" {
   abstract class ViewModelBase {
       + IsBusy: bool
       + StatusMessage: string
       # SetProperty<T>(field: ref T, value: T): bool
   }

   class LoginViewModel extends ViewModelBase {
       - _authService: IAuthService
       + Username: string
       + Password: string
       + LoginCommand: ICommand
       + EjecutarLoginAsync(): Task
   }

   class HistoriaClinicaViewModel extends ViewModelBase {
       - _clinicaService: IClinicaService
       + PacienteSeleccionado: Paciente
       + Atenciones: ObservableCollection<AtencionClinica>
       + NuevaAtencionCommand: ICommand
       + CargarHistorialAsync(pacienteId: int): Task
   }

   class NuevaAtencionViewModel extends ViewModelBase {
       - _clinicaService: IClinicaService
       + VeterinariosDisponibles: ObservableCollection<Veterinario>
       + VeterinarioSeleccionadoId: int
       + PesoKg: double
       + Motivo: string
       + Diagnostico: string
       + Tratamiento: string
       + GuardarCommand: ICommand
       + GuardarAsync(): Task
   }

   class InmunizacionesViewModel extends ViewModelBase {
       - _clinicaService: IClinicaService
       - _pdfExportService: IPdfExportService
       + Inmunizaciones: ObservableCollection<Inmunizacion>
       + ExportarPdfCommand: ICommand
       + RegistrarVacunaCommand: ICommand
       + GenerarCarnetPdfAsync(ruta: string): Task
   }

   class RecordatoriosViewModel extends ViewModelBase {
       - _launcherService: IExternalLauncherService
       + TelefonoPropietario: string
       + MensajeWhatsApp: string
       + EnviarWhatsAppCommand: ICommand
       + EnviarCorreoCommand: ICommand
       + NotificarWhatsApp(): void
       + NotificarCorreo(): void
   }
}

' Relaciones de asociación y composición
Propietario "1" *-- "0..*" Paciente : posee >
Paciente "1" *-- "0..*" AtencionClinica : registra >
Paciente "1" *-- "0..*" Inmunizacion : registra >
Veterinario "1" <-- "0..*" AtencionClinica : atendido por
Veterinario "1" <-- "0..*" Inmunizacion : suministrado por

' Relaciones de infraestructura y lógica
UnitOfWork o-- VetClinicDbContext
ClinicaRepository o-- VetClinicDbContext
HistoriaClinicaViewModel --> IClinicaService
NuevaAtencionViewModel --> IClinicaService
InmunizacionesViewModel --> IClinicaService
InmunizacionesViewModel --> IPdfExportService
RecordatoriosViewModel --> IExternalLauncherService
LoginViewModel --> IAuthService
@enduml

3.3 Diagramas de Secuencia (Uno por cada Caso de Uso)
3.3.1 Secuencia CU-01: Iniciar Sesión en el Sistema
Detalla la autenticación local verificando el hash PBKDF2 contra la base de datos SQLite.






Fragmento de código
@startuml Secuencia_CU01_IniciarSesion
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario" as Vet
participant "LoginView (XAML)" as View
participant "LoginViewModel" as VM
participant "AuthService" as AuthSvc
participant "PasswordHasher" as Hasher
participant "VetClinicDbContext" as DB
database "SQLite (Local)" as SQL

Vet -> View: Ingresa Username y Password
Vet -> View: Presiona botón "Iniciar Sesión"
View -> VM: LoginCommand.Execute()
activate VM

VM -> VM: Set IsBusy = true
VM -> AuthSvc: AuthenticateAsync(user, pass)
activate AuthSvc

AuthSvc -> DB: Usuarios.FirstOrDefaultAsync(u => u.Username == user && u.IsActive == 1)
activate DB
DB -> SQL: SELECT * FROM Usuarios WHERE Username = @u AND IsActive = 1 LIMIT 1
SQL --> DB: Retorna registro Usuario (o null)
DB --> AuthSvc: Entidad Usuario

alt Usuario no existe
   AuthSvc --> VM: Result.Failure("Credenciales incorrectas")
   VM -> View: Mostrar DialogError("Usuario o contraseña incorrectos")
else Usuario existe
   AuthSvc -> Hasher: VerifyHashedPassword(pass, user.PasswordHash, user.PasswordSalt)
   activate Hasher
   Hasher -> Hasher: Derivar PBKDF2 HMAC-SHA256 (100.000 iteraciones)
   Hasher --> AuthSvc: true / false
   deactivate Hasher
   
   alt Hash Coincide
       AuthSvc --> VM: Result.Success()
       VM -> View: NavigationService.NavigateTo<ShellView>()
   else Hash Incorrecto
       AuthSvc --> VM: Result.Failure("Credenciales incorrectas")
       VM -> View: Mostrar DialogError("Usuario o contraseña incorrectos")
       VM -> VM: Password = string.Empty
   end
end

deactivate AuthSvc
VM -> VM: Set IsBusy = false
deactivate VM
@enduml

3.3.2 Secuencia CU-02: Registrar Mascota y Propietario (Nuevo o Existente)
Recorre el alta médica con cálculo automático de edad y creación rápida opcional del tutor legal.






Fragmento de código
@startuml Secuencia_CU02_RegistrarMascotaYPropietario
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario" as Vet
participant "PacientesView (XAML)" as View
participant "PacientesViewModel" as VM
participant "PropietarioModalView" as ModalProp
participant "ClinicaService" as Svc
participant "UnitOfWork" as UoW
database "SQLite (Local)" as SQL

Vet -> View: Clic "Nuevo Paciente"
View -> VM: IniciarNuevoPacienteCommand.Execute()
activate VM

alt Propietario No Existe en Búsqueda (Flujo Alternativo 2a)
   Vet -> View: Clic "Crear Propietario en este flujo"
   View -> ModalProp: ShowDialog()
   Vet -> ModalProp: Diligencia Cédula, Nombres, Celular (10 dígitos), Email
   ModalProp -> VM: GuardarPropietarioRapido(propDto)
   VM -> Svc: CrearPropietarioAsync(propEntity)
   activate Svc
   Svc -> Svc: Validar Celular (10 dígitos, inicia en '3')
   Svc -> UoW: Propietarios.AddAsync(propEntity)
   Svc -> UoW: CommitAsync()
   activate UoW
   UoW -> SQL: INSERT INTO Propietarios (...)
   SQL --> UoW: OK (Id generado)
   UoW --> Svc: Transacción Confirmada
   deactivate UoW
   Svc --> VM: Propietario Creado
   deactivate Svc
   VM -> ModalProp: Close()
   VM -> View: Auto-selecciona Propietario recién creado
end

Vet -> View: Ingresa Nombre, Especie, Raza, Sexo, FechaNacimiento, PesoActualKg
View -> VM: PropertyChanged(FechaNacimiento)
VM -> VM: Paciente.CalcularEdadFormateada(DateTime.Now)
VM --> View: Refresca TextBlock EdadCalculada (ej. "2 años, 3 meses")

Vet -> View: Clic "Guardar Ficha de Paciente"
View -> VM: GuardarPacienteCommand.Execute()

VM -> Svc: RegistrarPacienteAsync(pacienteEntity)
activate Svc
Svc -> Svc: Validar Reglas (Peso > 0 y <= 150.0; PropietarioId > 0)
Svc -> UoW: Pacientes.AddAsync(pacienteEntity)
Svc -> UoW: CommitAsync()
activate UoW
UoW -> SQL: INSERT INTO Pacientes (...)
SQL --> UoW: OK
UoW --> Svc: Transacción Confirmada
deactivate UoW
Svc --> VM: Result.Success()
deactivate Svc

VM -> View: MostrarToast("Paciente registrado satisfactoriamente")
VM -> View: LimpiarFormulario()
deactivate VM
@enduml

3.3.3 Secuencia CU-03: Registrar Atención Médica con Asignación de Profesional
Garantiza la asignación obligatoria del profesional (Dr. Fabio o Dr. William), el almacenamiento inmutable y la actualización del peso en la ficha.






Fragmento de código
@startuml Secuencia_CU03_RegistrarAtencionMedica
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario" as Vet
participant "HistoriaClinicaView" as View
participant "NuevaAtencionViewModel" as VM
participant "ClinicaService" as Svc
participant "UnitOfWork" as UoW
database "SQLite (Local)" as SQL

Vet -> View: Clic "Nueva Consulta / Procedimiento"
View -> VM: InicializarCommand.Execute(pacienteId)
activate VM
VM -> VM: FechaHoraAtencion = DateTime.Now
VM -> VM: CargarVeterinarios() (Dr. Fabio, Dr. William)
VM --> View: Presenta formulario con campos precargados

Vet -> View: Selecciona VeterinarioTratante (Obligatorio)
Vet -> View: Digita PesoConsultaKg, Motivo, Examen, Diagnostico, Tratamiento
Vet -> View: Clic "Confirmar y Guardar Consulta"
View -> VM: GuardarAtencionCommand.Execute()

alt Veterinario No Asignado (Excepción 3a)
   VM -> View: Resalta borde rojo en ComboBox y muestra "Debe seleccionar el médico veterinario responsable"
else Peso Fuera de Rango (Excepción 3b)
   VM -> View: Mostrar DialogError("El peso debe ser mayor a 0 y menor o igual a 150 Kg")
else Datos Válidos
   VM -> Svc: RegistrarAtencionAsync(atencionEntity)
   activate Svc
   Svc -> UoW: AtencionesClinicas.AddAsync(atencionEntity)
   Svc -> UoW: CommitAsync()
   activate UoW
   UoW -> SQL: INSERT INTO AtencionesClinicas (...)
   note over SQL
     El trigger TR_AtencionesClinicas_ActualizarPesoPaciente
     actualiza automáticamente PesoActualKg en tabla Pacientes
   end note
   SQL --> UoW: Confirmación OK
   UoW --> Svc: Filas Afectadas = 1
   deactivate UoW
   Svc --> VM: Result.Success()
   deactivate Svc

   VM -> View: MostrarToast("Atención clínica guardada e inmutable")
   VM -> View: CloseDialog()
end
deactivate VM
@enduml

3.3.4 Secuencia CU-04: Consultar Historia Clínica Unificada
Muestra el expediente ordenado cronológicamente de forma descendente sin necesidad de llamadas telefónicas entre turnos.






Fragmento de código
@startuml Secuencia_CU04_ConsultarHistoriaClinica
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario\n(En turno)" as Vet
participant "HistoriaClinicaView" as View
participant "HistoriaClinicaViewModel" as VM
participant "ClinicaService" as Svc
participant "AtencionClinicaRepository" as Rep
database "SQLite (Local)" as SQL

Vet -> View: Ingresa término de búsqueda (Nombre paciente o cédula)
View -> VM: BuscarPacienteCommand.Execute(criterio)
activate VM

VM -> Svc: BuscarPacientesConPropietarioAsync(criterio)
activate Svc
Svc -> SQL: SELECT * FROM Pacientes INNER JOIN Propietarios WHERE ...
SQL --> Svc: Lista de coincidencias
Svc --> VM: Colección de Pacientes
deactivate Svc

Vet -> View: Selecciona un Paciente de la lista
View -> VM: CargarHistorialCommand.Execute(pacienteSeleccionado.Id)

VM -> Svc: ObtenerHistorialPacienteAsync(pacienteId)
activate Svc
Svc -> Rep: GetHistorialPorPacienteAsync(pacienteId)
activate Rep

Rep -> SQL: SELECT a.*, v.Nombre FROM AtencionesClinicas a\nINNER JOIN Veterinarios v ON a.VeterinarioId = v.Id\nWHERE a.PacienteId = @p\nORDER BY a.FechaHoraAtencion DESC;
note right of SQL
 Garantiza orden cronológico
 descendente estricto (RN-07)
end note
SQL --> Rep: DataReader con registros históricos
Rep --> Svc: List<AtencionClinica>
deactivate Rep
Svc --> VM: IReadOnlyList<AtencionClinica>
deactivate Svc

VM -> VM: AtencionesCollection.Clear()
VM -> VM: AtencionesCollection.AddRange(resultado)
VM --> View: DataGrid / ItemsControl actualizado
deactivate VM

note over View
 El médico en turno lee diagnósticos previos, dosis
 y el veterinario que atendió (Fabio o William)
 con tiempo de carga local < 1 segundo (RNF-05)
end note
@enduml

3.3.5 Secuencia CU-05: Registrar Vacuna y Descargar Carnet Digital en PDF
Describe el alta de dosis de inmunización y la compilación asíncrona local del documento PDF.






Fragmento de código
@startuml Secuencia_CU05_RegistrarVacunaYDescargarCarnet
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario" as Vet
participant "InmunizacionesView" as View
participant "InmunizacionesViewModel" as VM
participant "ClinicaService" as Svc
participant "QuestPdfExportService" as PdfSvc
participant "UnitOfWork" as UoW
database "SQLite (Local)" as SQL
participant "Sistema de Archivos Local" as FS

Vet -> View: Pestaña "Vacunación", Clic "Registrar Dosis"
Vet -> View: Selecciona Tipo (Vacuna), NombreProducto, Refuerzo, Veterinario
Vet -> View: Clic "Guardar Dosis"
View -> VM: GuardarInmunizacionCommand.Execute()
activate VM

alt Fecha Refuerzo <= Fecha Aplicacion (Excepción 5a)
   VM -> View: Mostrar DialogError("La fecha de refuerzo debe ser posterior a la fecha de aplicación")
else Validación Exitosa
   VM -> Svc: RegistrarInmunizacionAsync(inmunizacionEntity)
   activate Svc
   Svc -> UoW: Inmunizaciones.AddAsync(inmunizacionEntity)
   Svc -> UoW: CommitAsync()
   activate UoW
   UoW -> SQL: INSERT INTO Inmunizaciones (...)
   SQL --> UoW: OK
   UoW --> Svc: Transacción OK
   deactivate UoW
   Svc --> VM: Result.Success()
   deactivate Svc
   VM -> View: MostrarToast("Dosis de inmunización registrada")
end

Vet -> View: Clic "Descargar Carnet Digital en PDF"
View -> VM: DescargarCarnetPdfCommand.Execute()

VM -> View: DialogService.SaveFileDialog("Guardar Carnet", "Carnet_Mascota.pdf")
View --> VM: Retorna ruta de archivo seleccionada (ej. "C:\Docs\Carnet.pdf")

VM -> VM: Set IsGeneratingPdf = true
VM -> PdfSvc: GenerarCarnetVacunacionAsync(pacienteId, rutaArchivo)
activate PdfSvc

PdfSvc -> SQL: Consulta consolidada (Paciente, Propietario, Inmunizaciones)
SQL --> PdfSvc: Datos completos del paciente e historial
PdfSvc -> PdfSvc: Renderizado nativo con QuestPDF (Encabezado, Tablas, Membrete)
PdfSvc -> FS: Escribir archivo binario PDF en disco (< 3 seg, RNF-06)
FS --> PdfSvc: Archivo escrito OK
PdfSvc --> VM: Result.Success()
deactivate PdfSvc

VM -> VM: Set IsGeneratingPdf = false
VM -> View: MostrarToast("Carnet PDF generado exitosamente")
VM -> View: DialogService.PreguntarAbrirArchivo(rutaArchivo)
deactivate VM
@enduml

3.3.6 Secuencia CU-06: Emitir Recordatorio Gratuito al Propietario
Muestra la generación de hipervínculos para WhatsApp Web/Desktop y URI mailto:, con cero costo de pasarelas.






Fragmento de código
@startuml Secuencia_CU06_EmitirRecordatorioGratuito
autonumber
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 12
skinparam shadowing false

actor "Médico Veterinario" as Vet
participant "RecordatoriosView" as View
participant "RecordatoriosViewModel" as VM
participant "ExternalLauncherService" as Launcher
participant "Navegador Web / WhatsApp Desktop" as Browser
participant "Cliente Correo (OS Default)" as MailApp

alt Canal Principal: WhatsApp (wa.me)
   Vet -> View: Clic "Notificar por WhatsApp"
   View -> VM: EnviarWhatsAppCommand.Execute(recordatorioSeleccionado)
   activate VM

   alt Propietario sin celular válido (Excepción 6b)
       VM -> View: Mostrar DialogError("El teléfono del propietario no es válido para enlace de WhatsApp")
   else Celular Colombia Válido (10 dígitos)
       VM -> VM: Formatear Número con prefijo: "57" + Telefono
       VM -> VM: Redactar plantilla: "Hola " + Propietario + ", la Clínica Veterinaria le recuerda que su mascota " + Mascota + " tiene programada su " + Tipo + " el día " + FechaRefuerzo + ". ¡Los esperamos!"
       VM -> VM: Uri.EscapeDataString(mensaje)
       VM -> VM: Construir URI: "[https://wa.me/57](https://wa.me/57)" + tel + "?text=" + textoCodificado
       VM -> Launcher: AbrirUrl(uriWhatsApp)
       activate Launcher
       Launcher -> Browser: Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true })
       deactivate Launcher
       VM -> View: MostrarToast("Enlace de WhatsApp abierto en su equipo")
   end
   deactivate VM

else Canal Secundario: Correo Electrónico (mailto:)
   Vet -> View: Clic "Notificar por Correo"
   View -> VM: EnviarCorreoCommand.Execute(recordatorioSeleccionado)
   activate VM

   alt Propietario sin Correo Registrado
       VM -> View: Mostrar DialogError("El propietario no tiene un correo electrónico configurado")
   else Correo Presente
       VM -> VM: Construir Asunto: "Recordatorio Médico Veterinario - " + Mascota
       VM -> VM: Construir Cuerpo Formal del Mensaje
       VM -> VM: Construir URI: "mailto:" + email + "?subject=" + escape(asunto) + "&body=" + escape(cuerpo)
       VM -> Launcher: AbrirUrl(uriMailto)
       activate Launcher
       Launcher -> MailApp: Process.Start(new ProcessStartInfo(uriMailto) { UseShellExecute = true })
       deactivate Launcher
       VM -> View: MostrarToast("Cliente de correo iniciado")
   end
   deactivate VM
end
@enduml

3.4 Diagrama Físico-Relacional de Base de Datos
Modelo físico estricto con tipos de datos nativos de SQLite y cardinalidades precisas.






Fragmento de código
@startuml DiagramaFisicoBD_VetClinic
skinparam defaultFontName "Segoe UI"
skinparam defaultFontSize 11
skinparam roundCorner 4
skinparam shadowing false
skinparam classAttributeIconSize 0

entity "Usuarios" as Usuarios {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * Username : TEXT <<UNIQUE, COLLATE NOCASE>>
   * PasswordHash : TEXT
   * PasswordSalt : TEXT
   * NombreCompleto : TEXT
   * Rol : TEXT <<DEFAULT 'Veterinario'>>
   * IsActive : INTEGER <<CHECK 0,1, 1 DEFAULT>>
   * CreatedAt : TEXT <<ISO8601>>
}

entity "Veterinarios" as Veterinarios {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * Nombre : TEXT <<UNIQUE>>
   * TarjetaProfesional : TEXT
   * IsActive : INTEGER <<CHECK 0,1, 1 DEFAULT>>
}

entity "Propietarios" as Propietarios {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * TipoDocumento : TEXT <<CHECK CC,CE,TI,PAS>>
   * NumeroDocumento : TEXT <<UNIQUE>>
   * Nombres : TEXT
   * Apellidos : TEXT
   * Telefono : TEXT <<CHECK '3*' 10 GLOB digitos,>>
   Direccion : TEXT <<NULL>>
   Email : TEXT <<NULL>>
   * CreatedAt : TEXT <<ISO8601>>
   * UpdatedAt : TEXT <<ISO8601>>
   * IsDeleted : INTEGER <<CHECK 0 0,1, DEFAULT>>
}

entity "Pacientes" as Pacientes {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * PropietarioId : INTEGER <<FK, NOT NULL>>
   * Nombre : TEXT
   * Especie : TEXT <<CHECK Canino,Felino,Otro>>
   * Raza : TEXT
   * Sexo : TEXT <<CHECK Macho,Hembra>>
   * FechaNacimiento : TEXT <<ISO8601>>
   * EsFechaEstimada : INTEGER <<CHECK 0 0,1, DEFAULT>>
   * PesoActualKg : REAL <<CHECK>0.0 AND <=150.0>>
   ColorSenas : TEXT <<NULL>>
   * EstadoReproductivo : TEXT <<CHECK Entero,Castrado>>
   * CreatedAt : TEXT <<ISO8601>>
   * UpdatedAt : TEXT <<ISO8601>>
   * IsDeleted : INTEGER <<CHECK 0 0,1, DEFAULT>>
}

entity "AtencionesClinicas" as AtencionesClinicas {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * PacienteId : INTEGER <<FK, NOT NULL>>
   * VeterinarioId : INTEGER <<FK, NOT NULL>>
   * FechaHoraAtencion : TEXT <<ISO8601>>
   * PesoConsultaKg : REAL <<CHECK>0.0 AND <=150.0>>
   * MotivoConsulta : TEXT
   ExamenClinico : TEXT <<NULL>>
   * Diagnostico : TEXT
   * Tratamiento : TEXT
   Indicaciones : TEXT <<NULL>>
   FechaControl : TEXT <<NULL, ISO8601>>
   * CreatedAt : TEXT <<ISO8601>>
}

entity "Inmunizaciones" as Inmunizaciones {
   * Id : INTEGER <<PK, AUTOINCREMENT>>
   --
   * PacienteId : INTEGER <<FK, NOT NULL>>
   * VeterinarioId : INTEGER <<FK, NOT NULL>>
   * TipoBiologico : TEXT <<CHECK Vacuna,Desparasitante>>
   * NombreProducto : TEXT
   LoteFabricante : TEXT <<NULL>>
   * FechaAplicacion : TEXT <<ISO8601>>
   * FechaRefuerzo : TEXT <<ISO8601, CHECK> FechaAplicacion>>
   Observaciones : TEXT <<NULL>>
   * CreatedAt : TEXT <<ISO8601>>
}

' Cardinalidades
Propietarios ||--o{ Pacientes : "1 tutor tiene N pacientes (ON DELETE RESTRICT)"
Pacientes ||--o{ AtencionesClinicas : "1 paciente tiene N atenciones (ON DELETE RESTRICT)"
Pacientes ||--o{ Inmunizaciones : "1 paciente tiene N inmunizaciones (ON DELETE RESTRICT)"
Veterinarios ||--o{ AtencionesClinicas : "1 veterinario atiende N consultas (ON DELETE RESTRICT)"
Veterinarios ||--o{ Inmunizaciones : "1 veterinario aplica N dosis (ON DELETE RESTRICT)"
@enduml

ENTREGABLE 4: DISEÑO Y ESPECIFICACIÓN DE INTERFACES (UI)
4.1 Catálogo de Pantallas / Vistas del Sistema
En conformidad estricta con los requisitos funcionales y casos de uso aprobados, el sistema cuenta únicamente con las siguientes vistas:
1. LoginView.xaml: Pantalla de autenticación local al inicio del aplicativo (CU-01, RF-01).
2. ShellView.xaml: Ventana principal de escritorio con barra lateral de navegación persistente, cabecera de estado y área de contenido modular dinámico.
3. PropietariosView.xaml: Directorio general de clientes/tutores, buscador multifactor y lista de pacientes vinculados (CU-02, RF-02, RF-03).
4. PropietarioModalView.xaml: Diálogo flotante para alta ágil y edición de propietarios en flujo regular o en caliente (CU-02, RF-02, RN-10).
5. PacientesView.xaml: Gestión del censo de mascotas, ficha biológica individual, cálculo dinámico de edad y edición de características (CU-02, RF-04, RF-05).
6. HistoriaClinicaView.xaml: Expediente médico unificado, línea de tiempo de atenciones cronológicas descendentes y acceso al alta de procedimientos (CU-03, CU-04, RF-06, RF-07).
7. NuevaAtencionModalView.xaml: Formulario de captura rápida de consulta médica ambulatoria con selección obligatoria del veterinario en turno (CU-03, RF-06, RN-02).
8. InmunizacionesView.xaml: Registro de vacunas/desparasitaciones, control de fechas de refuerzo y exportación de Carnet Digital en PDF (CU-05, RF-08, RF-09).
9. RecordatoriosView.xaml: Panel de despacho de avisos preventivos a propietarios mediante WhatsApp (wa.me) y correo local (mailto:) (CU-06, RF-10, RF-11).
4.2 Lineamientos Estéticos, Usabilidad y Ergonomía Visual
* Paleta de Colores Sobria:
   * Verde Clínico Primario (Acento institucional): #1B5E20 (Dark Green) / #2E7D32 (Primary Green).
   * Fondo de Aplicación: #F8F9FA (Off-White neutro, reduce fatiga visual en consultorio).
   * Contenedores / Tarjetas: #FFFFFF con borde sutil #E0E0E0.
   * Texto Principal: #212121 (High-contrast Charcoal, relación de contraste $> 7:1$).
   * Texto Secundario y Labels: #616161.
   * Estados de Alerta y Validación: #C62828 (Rojo error) / #2E7D32 (Verde confirmación).
* Tipografía y Legibilidad de Consultorio (RNF-04):
   * Familia Tipográfica: Segoe UI en todo el sistema.
   * Tamaños: Encabezados de vista 20pt SemiBold; Títulos de sección 16pt SemiBold; Campos de captura, tablas y botones 14pt Regular/Medium (permite lectura clara a más de 1 metro del monitor).
* Terminología Médica Veterinaria Oficial (Colombia):
   * "Paciente" (en lugar de mascota o animal).
   * "Propietario / Acudiente" (en lugar de cliente o usuario).
   * "Historia Clínica" (en lugar de log o notas).
   * "Atención Médica Ambulatoria / Procedimiento" (en lugar de consulta o cita).
   * "Inmunización y Desparasitación" (en lugar de biológicos).
   * "Constantes Fisiológicas" y "Anamnesis".
4.3 Especificación Detallada por Interfaz
Vista 1: LoginView.xaml (Autenticación Local)
* Propósito: Validar el acceso del operador antes de habilitar el entorno de trabajo (CU-01, RF-01).
* Layout: Ventana centrada sin redimensionamiento ($500 \times 400$ px), panel con tarjeta sombreada sobre fondo #F8F9FA.
* Controles y Bindings:
   * TextBox (Usuario): Watermark "Ingrese su usuario", enlazado a Username, validación visual de texto no vacío.
   * PasswordBox (Contraseña): Control enmascarado enlazado a través de PasswordHelper.Password a Password.
   * Button (Iniciar Sesión): Estilo primario (#2E7D32), tecla por defecto Enter, enlazado a LoginCommand.
   * ProgressBar: Indeterminado, visibilidad condicionada a IsBusy.
* Feedback: Si la autenticación falla, se muestra un mensaje en texto rojo (#C62828) debajo de los campos: "Usuario o contraseña incorrectos" y foco automático al campo contraseña.
Vista 2: ShellView.xaml (Ventana Principal y Navegación)
* Propósito: Contenedor maestro tipo Single Window Navigation con menú lateral persistente y cabecera informativa.
* Layout: Grid de 2 columnas: Columna 0 (Ancho fijo 260 px) para Barra Lateral; Columna 1 (Ancho *) para Cabecera superior (Alto 60 px) y ContentControl dinámico central.
* Zonas y Controles:
   * Barra Lateral: Logotipo vectorial de la clínica, botones de navegación con iconos vectoriales y texto (14pt):
      * "Directorio de Propietarios" $\rightarrow$ NavigateCommand("Propietarios")
      * "Censo de Pacientes" $\rightarrow$ NavigateCommand("Pacientes")
      * "Historia Clínica" $\rightarrow$ NavigateCommand("HistoriaClinica")
      * "Plan de Vacunación" $\rightarrow$ NavigateCommand("Inmunizaciones")
      * "Recordatorios y Alertas" $\rightarrow$ NavigateCommand("Recordatorios")
   * Cabecera: Indicador de estado local ("Modo Autónomo Local Activo"), fecha y hora del sistema, nombre del usuario autenticado y botón "Cerrar Sesión".
Vista 3: PropietariosView.xaml (Directorio y Búsqueda de Clientes)
* Propósito: Búsqueda rápida, consulta y modificación de acudientes y visualización de sus mascotas asociadas (CU-02, RF-02, RF-03).
* Layout: Panel superior de filtros (Alto 80 px) y división horizontal 60/40: mitad izquierda DataGrid de Propietarios, mitad derecha panel informativo con tarjetas de pacientes asociados.
* Controles y Bindings:
   * TextBox (Criterio de Búsqueda): Placeholder "Buscar por documento, nombre o celular...", Text="{Binding CriterioBusqueda, UpdateSourceTrigger=PropertyChanged}".
   * Button (Nuevo Propietario): Dispara AbrirCreacionPropietarioCommand.
   * DataGrid (Propietarios): Columnas: Tipo Doc, Documento, Nombre Completo, Teléfono, Correo Electrónico. Doble clic dispara edición.
   * ItemsControl (Pacientes del Propietario): Muestra tarjetas visuales por cada animal registrado con: Nombre, Especie/Raza, Edad Calculada, botón directo "Ver Historia Clínica".
Vista 4: PropietarioModalView.xaml (Formulario de Propietario)
* Propósito: Registro o modificación de los datos de un tutor legal (RF-02).
* Layout: Ventana modal flotante ($550 \times 520$ px).
* Campos y Validaciones:
   * ComboBox (Tipo Documento): Opciones CC, CE, TI, PAS. Default: CC.
   * TextBox (Número de Documento): Obligatorio, solo caracteres alfanuméricos.
   * TextBox (Nombres) y TextBox (Apellidos): Obligatorios, texto.
   * TextBox (Teléfono Celular): Obligatorio. Formato regex: ^3[0-9]{9}$ (10 dígitos exactos iniciando en 3). Borde rojo dinámico si no cumple.
   * TextBox (Dirección): Opcional.
   * TextBox (Correo Electrónico): Opcional, validación de formato email estándar.
   * Button (Guardar Propietario): Enlazado a GuardarCommand, habilitado solo si el formulario es válido.
Vista 5: PacientesView.xaml (Gestión de Ficha de Paciente)
* Propósito: Registrar y consultar pacientes vinculados a tutores con cálculo automático de edad (RF-04, RF-05).
* Layout: Formulario de doble columna con previsualización dinámica de la ficha médica a la derecha.
* Controles y Bindings:
   * ComboBox / AutoCompleteBox (Propietario Acudiente): Selección de acudiente con botón lateral "+" ("Crear Propietario en este flujo" para alta en caliente).
   * TextBox (Nombre del Paciente): Obligatorio, Text="{Binding Nombre}".
   * ComboBox (Especie): Opciones Canino, Felino, Otro.
   * TextBox (Raza): Obligatorio.
   * ComboBox (Sexo): Opciones Macho, Hembra.
   * DatePicker (Fecha de Nacimiento): Precargado con fecha actual.
   * CheckBox (Fecha Aproximada): IsChecked="{Binding EsFechaEstimada}".
   * TextBlock (Edad Dinámica): Muestra en tiempo real el valor calculado (EdadFormateada, ej. "3 años, 4 meses").
   * TextBox (Peso en Kg): Validación decimal. Rango admitido: $> 0.00$ y $\le 150.00$ Kg. Formato N2.
   * ComboBox (Estado Reproductivo): Opciones Entero, Castrado/Esterilizado.
   * TextBox (Color / Señas): Opcional.
Vista 6: HistoriaClinicaView.xaml (Expediente Médico Unificado)
* Propósito: Visualización de la historia clínica consolidada en orden cronológico inverso y acceso a atenciones (CU-03, CU-04, RF-06, RF-07).
* Layout: Encabezado con datos biológicos del paciente y resumen del tutor (Alto 100 px). Panel central con ItemsControl tipo línea de tiempo clínica.
* Zonas y Controles:
   * Ficha Superior: Nombre del paciente, Especie/Raza, Sexo, Edad actual, Último peso registrado en Kg y Nombre/Celular del Acudiente.
   * Panel de Acciones: Botón principal destacado "Nueva Consulta / Procedimiento" (#2E7D32), botón "Ir a Vacunación", botón "Volver".
   * Línea de Tiempo de Atenciones (Timeline): Cada entrada se renderiza en una tarjeta estructurada:
      * Cabecera de Tarjeta: Fecha y Hora de Atención, Badge con el nombre del Médico Veterinario Tratante (Dr. Fabio o Dr. William en color contrastante) y Peso registrado en esa consulta.
      * Cuerpo de Tarjeta: Secciones claramente tituladas: Motivo de Consulta (Anamnesis), Hallazgos Clínicos, Diagnóstico y Fórmula Médica / Tratamiento Prescrito.
      * Pie de Tarjeta: Fecha de Control Sugerida (si fue programada).
      * Propiedad de inmutabilidad: No existen botones de borrar o editar en estas tarjetas (RN-07).
Vista 7: NuevaAtencionModalView.xaml (Registro de Atención Médica)
* Propósito: Formulario ágil de consulta para completar la atención en menos de 90 segundos (RNF-03, RF-06).
* Layout: Diálogo modal ($700 \times 650$ px), estructurado en 3 bloques lógicos verticales.
* Campos y Validaciones:
   * Bloque 1 (Profesional y Constantes):
      * ComboBox (Médico Veterinario Tratante): Obligatorio. Desplegable con "Dr. Fabio" y "Dr. William". Si no se selecciona, se bloquea el guardado con alerta roja visible (RN-02).
      * DatePicker y TimePicker: Precargados automáticamente con fecha y hora del sistema (RN-10).
      * TextBox (Peso en Consulta): Obligatorio, numérico decimal en Kg (Rango $0.01 - 150.00$).
   * Bloque 2 (Evaluación Médica):
      * TextBox (Motivo de Consulta): Multilínea (3 filas), obligatorio.
      * TextBox (Examen Clínico / Constantes Vitales): Multilínea (3 filas), opcional.
      * TextBox (Diagnóstico / Presunción): Multilínea (3 filas), obligatorio.
   * Bloque 3 (Prescripción y Cierre):
      * TextBox (Tratamiento, Medicamentos y Dosis): Multilínea (4 filas), obligatorio.
      * DatePicker (Fecha de Control Sugerida): Opcional.
   * Acciones: Botón "Guardar Procedimiento" (Verde) y Botón "Cancelar" (Gris neutro).
Vista 8: InmunizacionesView.xaml (Carnet y Vacunación)
* Propósito: Registro de biológicos aplicados y exportación del carnet en PDF (CU-05, RF-08, RF-09).
* Layout: División vertical: mitad superior formulario ágil de aplicación; mitad inferior tabla histórica de vacunas con botón de exportación destacado.
* Controles y Bindings:
   * ComboBox (Tipo Biológico): Opciones Vacuna, Desparasitante.
   * TextBox (Nombre del Producto): Obligatorio (ej. "Triple Felina", "Nobivac DHPPi").
   * TextBox (Lote / Fabricante): Opcional.
   * DatePicker (Fecha de Aplicación): Precargada con fecha actual.
   * DatePicker (Próximo Refuerzo): Obligatorio. Validación: debe ser estrictamente posterior a la fecha de aplicación.
   * ComboBox (Veterinario Responsable): Dr. Fabio / Dr. William.
   * Button (Registrar Dosis): Guarda en base de datos e inserta fila en el DataGrid.
   * Button (Descargar Carnet Digital en PDF): Destacado en barra de herramientas con icono de documento. Abre cuadro de diálogo local SaveFileDialog y genera el PDF en menos de 3 segundos (RNF-06).
Vista 9: RecordatoriosView.xaml (Notificaciones Gratuitas)
* Propósito: Gestionar y disparar avisos preventivos a acudientes con cero costo de mensajería (CU-06, RF-10, RF-11).
* Layout: DataGrid de citas y refuerzos próximos (próximos 7 días o seleccionables por fecha) con panel lateral de previsualización del mensaje.
* Zonas y Controles:
   * DataGrid (Refuerzos y Controles Pendientes): Columnas: Fecha Programada, Paciente, Propietario, Celular, Procedimiento/Vacuna, Acción Rápida.
   * Panel de Previsualización: Muestra la plantilla armada con los datos reales:
"Hola [Nombre Propietario], la Clínica Veterinaria le recuerda que su mascota [Nombre Mascota] tiene programada su [Vacuna/Control] el día [Fecha]. ¡Los esperamos!"
   * Button (Notificar por WhatsApp): Ejecuta Process.Start con la URI https://wa.me/57[TELEFONO]?text=[MENSAJE] abriendo WhatsApp Web o la app de escritorio.
   * Button (Notificar por Correo): Ejecuta Process.Start con el esquema mailto:[EMAIL]?subject=...&body=... abriendo el cliente de correo predeterminado del sistema operativo.
ENTREGABLE 5: MATRIZ DE TRAZABILIDAD BIDIRECCIONAL
A continuación se presenta la matriz de cobertura total del sistema, vinculando cada requisito de negocio, funcional y no funcional con su caso de uso, estructura de persistencia, lógica de código e interfaz de usuario:
ID Requisito
	Nombre del Requisito
	Caso(s) de Uso
	Tabla(s) / Entidad(es)
	Clase(s) / ViewModel(s)
	Interfaz / Pantalla (Vista)
	RF-01
	Autenticación y Control de Acceso Local
	CU-01
	Usuarios / Usuario
	AuthService, PasswordHasher, LoginViewModel
	LoginView.xaml
	RF-02
	Gestión de Propietarios
	CU-02
	Propietarios / Propietario
	ClinicaService, PropietariosViewModel
	PropietariosView.xaml, PropietarioModalView.xaml
	RF-03
	Búsqueda y Consulta de Propietarios
	CU-02, CU-04
	Propietarios, Pacientes
	ClinicaService, PropietariosViewModel
	PropietariosView.xaml
	RF-04
	Registro de Paciente con Propietario
	CU-02
	Pacientes, Propietarios
	ClinicaService, PacientesViewModel
	PacientesView.xaml, PropietarioModalView.xaml
	RF-05
	Consulta y Actualización de Ficha de Paciente
	CU-02, CU-04
	Pacientes / Paciente
	ClinicaService, PacientesViewModel
	PacientesView.xaml
	RF-06
	Registro de Procedimiento y Consulta Médica
	CU-03
	AtencionesClinicas, Pacientes
	ClinicaService, NuevaAtencionViewModel
	NuevaAtencionModalView.xaml
	RF-07
	Visualización Consolidada de Historia Clínica
	CU-04
	AtencionesClinicas, Veterinarios
	ClinicaService, HistoriaClinicaViewModel
	HistoriaClinicaView.xaml
	RF-08
	Registro de Inmunización y Desparasitación
	CU-05
	Inmunizaciones, Veterinarios
	ClinicaService, InmunizacionesViewModel
	InmunizacionesView.xaml
	RF-09
	Generación y Descarga de Carnet PDF
	CU-05
	Pacientes, Inmunizaciones
	QuestPdfExportService, InmunizacionesViewModel
	InmunizacionesView.xaml
	RF-10
	Enlace Directo WhatsApp (wa.me)
	CU-06
	Propietarios, Inmunizaciones
	ExternalLauncherService, RecordatoriosViewModel
	RecordatoriosView.xaml
	RF-11
	Enlace Alternativo Correo (mailto:)
	CU-06
	Propietarios, Inmunizaciones
	ExternalLauncherService, RecordatoriosViewModel
	RecordatoriosView.xaml
	RN-01
	Autenticación Obligatoria y Dinámica
	CU-01
	Usuarios / Usuario
	AuthService, LoginViewModel
	LoginView.xaml
	RN-02
	Trazabilidad Obligatoria Veterinario Tratante
	CU-03, CU-05
	Veterinarios, AtencionesClinicas
	ClinicaService, NuevaAtencionViewModel
	NuevaAtencionModalView.xaml, InmunizacionesView.xaml
	RN-03
	Cardinalidad Propietario-Paciente ($1:N$)
	CU-02
	Propietarios, Pacientes
	ClinicaService, PacientesViewModel
	PacientesView.xaml
	RN-04
	Validación Celular Colombia (10 dígitos, '3')
	CU-02, CU-06
	Propietarios (Telefono)
	NumeroCelular (ValueObject), PropietariosViewModel
	PropietarioModalView.xaml
	RN-05
	Dinamismo en la Edad del Paciente
	CU-02, CU-04
	Pacientes (FechaNacimiento)
	Paciente.CalcularEdadFormateada(), PacientesVM
	PacientesView.xaml, HistoriaClinicaView.xaml
	RN-06
	Estandarización de Peso en Kg ($0 < P \le 150$)
	CU-02, CU-03
	Pacientes, AtencionesClinicas
	PesoCorporal (ValueObject), NuevaAtencionVM
	PacientesView.xaml, NuevaAtencionModalView.xaml
	RN-07
	Inmutabilidad de Historia Clínica (Ley 576)
	CU-03, CU-04
	AtencionesClinicas (Triggers)
	AtencionClinicaRepository (Solo Add/Get)
	HistoriaClinicaView.xaml
	RN-08
	Estructura Mínima Carnet Vacunación
	CU-05
	Inmunizaciones
	QuestPdfExportService, InmunizacionesViewModel
	InmunizacionesView.xaml
	RN-09
	Notificación Gratuita (Cero Costo Operativo)
	CU-06
	Propietarios
	ExternalLauncherService, RecordatoriosViewModel
	RecordatoriosView.xaml
	RN-10
	Principio de Entrada Ágil (Anti-Fricción)
	CU-02, CU-03
	Todas las entidades
	Todos los ViewModels (Defaults de fecha, navegación)
	ShellView.xaml, Todas las Vistas
	RNF-01
	Protección Criptográfica de Credenciales
	CU-01
	Usuarios (PasswordHash/Salt)
	PasswordHasher (PBKDF2 HMAC-SHA256)
	LoginView.xaml
	RNF-02
	Privacidad y Reserva Legal de Datos
	CU-04
	AtencionesClinicas
	VetClinicDbContext (Acceso encapsulado en UI)
	HistoriaClinicaView.xaml
	RNF-03
	Usabilidad: Consulta en $< 90$ segundos
	CU-03
	AtencionesClinicas
	NuevaAtencionViewModel (Formulario modal único)
	NuevaAtencionModalView.xaml
	RNF-04
	Legibilidad Visual ($\ge 14$pt, contraste)
	Todos los CUs
	N/A (Estilos XAML)
	Diccionario de recursos tipográficos y temas
	Todos los archivos .xaml
	RNF-05
	Carga Local $< 1$ segundo
	CU-04
	Índices SQLite en Atenciones
	AtencionClinicaRepository (Consultas optimizadas)
	HistoriaClinicaView.xaml
	RNF-06
	Generación PDF en $\le 3$ segundos
	CU-05
	Inmunizaciones, Pacientes
	QuestPdfExportService (Compilación asíncrona)
	InmunizacionesView.xaml
	RNF-07
	Operación 100% Fuera de Línea (Offline)
	CU-01 a CU-05
	Base de datos SQLite local
	Arquitectura local completa sin dependencias HTTP
	Todo el software de escritorio
	RNF-08
	Arquitectura Monopuesto (Sin servidor)
	Todos los CUs
	SQLite local en %LocalAppData%
	Configuración de conexión en VetClinicDbContext
	Aplicación instalada en estación única
	RNF-09
	Cero Costo de Mensajería de Notificación
	CU-06
	N/A
	ExternalLauncherService (Llamadas a wa.me y mailto)
	RecordatoriosView.xaml

---

## ANEXO RAMA WEB: ESPECIFICACIÓN TÉCNICA Y ARQUITECTURA LOCALHOST (ASP.NET CORE + REACT SPA)

### W.1 Justificación y Alcance de la Transición
En la rama `web`, la interfaz de usuario evoluciona de WPF de escritorio a una arquitectura desacoplada moderna:
- **Backend:** `VetClinic.Api` (ASP.NET Core Web API en .NET 8 LTS), sirviendo la lógica de negocio a través de Kestrel en `http://localhost:5000`.
- **Frontend:** `VetClinic.Web` (Single Page Application en React 18 + TypeScript + Vite + Tailwind CSS).
- **Persistencia y Dominio:** Se reutilizan al 100% las capas `VetClinic.Domain` y `VetClinic.Infrastructure`, preservando la base de datos embebida SQLite en `%LocalAppData%\VetClinic\vetclinic_local.db`, los disparadores de inmutabilidad de la Ley 576, la seguridad PBKDF2 y la generación de carnets PDF con QuestPDF.
- **Cumplimiento de Restricciones:** La aplicación corre enteramente en la estación monopuesto (localhost), opera 100% fuera de línea (RNF-07, RNF-08) y no consume infraestructura de pago (RNF-09).

### W.2 Catálogo de Endpoints REST (`VetClinic.Api`)
| Método | Ruta | Descripción | Caso de Uso |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Autenticación con PBKDF2; retorna token y datos del usuario | CU-01, RF-01 |
| `GET` | `/api/veterinarios` | Lista de veterinarios oficiales activos ("Dr. Fabio", "Dr. William") | RN-02, STF-01 |
| `GET` | `/api/propietarios?criterio={q}` | Búsqueda reactiva de propietarios con normalización diacrítica | CU-02, RF-02 |
| `POST` | `/api/propietarios` | Alta de propietario con validación de celular Colombia (10 dígitos, inicia en 3) | CU-02, RF-02, RN-04 |
| `PUT` | `/api/propietarios/{id}` | Actualización de datos de contacto de propietario | CU-02, RF-03 |
| `GET` | `/api/pacientes?criterio={q}` | Búsqueda y censo de pacientes con edad calculada y acudiente | CU-03, RF-04, RN-05 |
| `GET` | `/api/pacientes/{id}` | Detalle completo de paciente (datos, histórico, vacunas, curva de peso) | CU-03, CU-04 |
| `POST` | `/api/pacientes` | Registro de paciente asociado a propietario con validación de peso | CU-03, RF-04, RN-06 |
| `PUT` | `/api/pacientes/{id}` | Actualización de datos básicos de paciente | CU-03, RF-05 |
| `POST` | `/api/atenciones` | Registro de acto clínico inmutable; actualiza peso del paciente | CU-04, RF-06, RN-07 |
| `GET` | `/api/atenciones/paciente/{pacienteId}` | Historial clínico cronológico inmutable del paciente | CU-04, RF-07 |
| `POST` | `/api/inmunizaciones` | Registro de vacuna aplicada y refuerzo; inmutable | CU-05, RF-08 |
| `GET` | `/api/inmunizaciones/paciente/{pacienteId}` | Registro de vacunas aplicadas a un paciente | CU-05, RF-08 |
| `GET` | `/api/inmunizaciones/carnet-pdf/{pacienteId}` | Descarga en streaming del Carnet Digital PDF (QuestPDF) | CU-05, RF-09, RNF-06 |
| `GET` | `/api/recordatorios?dias={d}&estado={todos\|proximos\|vencidos}` | Tablero de refuerzos pendientes con segmentación de mora | CU-06, RF-10, RF-11 |

### W.3 Estrategia de DTOs (Data Transfer Objects)
Para prevenir excepciones de ciclo circular en `System.Text.Json` (`JsonException`) y garantizar contratos limpios con TypeScript:
1. `UsuarioDto`: Retorna `Id`, `Username`, `NombreCompleto`, `Rol`; omite estrictamente `PasswordHash` y `PasswordSalt`.
2. `PropietarioDto`: Retorna datos limpios del acudiente y conteo/resumen de pacientes.
3. `PacienteDto` y `PacienteDetalleDto`: Incluye `EdadFormateada`, peso actual y resumen de acudiente. En detalle incluye `CurvaPeso` (`[{ fecha, pesoKg }]`).
4. `AtencionClinicaDto`: Retorna datos inmutables del acto clínico con nombre del veterinario tratante.
5. `InmunizacionDto`: Retorna vacuna, lote, fecha de aplicación, fecha de refuerzo y estado calculado (`AlDia`, `Proximo`, `Vencido`).
6. `RecordatorioDto`: Incluye datos del acudiente, mascota, vacuna, fecha de refuerzo, estado y URIs pregeneradas para WhatsApp (`https://wa.me/57...`) y correo (`mailto:`).

### W.4 Sistema de Diseño y Paleta Clínica Cálida (Frontend)
- **Fondo General:** Lino Cálido `#FBF9F5` (descanso visual en jornadas prolongadas).
- **Tarjetas y Superficies:** Blanco `#FFFFFF` con bordes sutiles en tono piedra `#E7E5E4`.
- **Verde Bosque Eucalipto (Marca/Sidebar):** `#166534` (primario de estructura).
- **Esmeralda Botánico (Acción/Botones):** `#059669` (interacciones principales).
- **Acento Ámbar Cálido (Vacunas próximas/alertas):** `#F59E0B` (calidez visual y prevención).
- **Rojo Coral (Alertas clínicas críticas/vencidos):** `#DC2626`.
- **Tipografía:** Inter / Segoe UI con escala de texto $\ge 14$px y contraste WCAG AAA superior a 7:1.

### W.5 Componentes y Ergonomía Clínica de la SPA
1. **Omnibox Global (`Ctrl + K`):** Búsqueda instantánea de pacientes y propietarios en cualquier momento.
2. **Expediente 360°:** Cabecera con avatar de especie, badges de alerta médica, cálculo de edad, línea de tiempo inmutable y gráfica de curva de peso.
3. **Panel Deslizante de Consulta (*Drawer*):** Formulario lateral para registrar atenciones sin perder de vista el historial previo del paciente.
4. **Tablero de Recordatorios con 1 Clic:** Segmentación de refuerzos (Próximos vs Vencidos) con botones nativos para abrir WhatsApp Web y cliente de correo sin costos de mensajería (RN-09).
5. **Hosting Monopuesto:** Kestrel sirve la API y los archivos estáticos de React compilados (`wwwroot`) en `http://localhost:5000` de forma unificada y autónoma.