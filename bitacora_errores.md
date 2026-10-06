# Bitácora de Errores y Resoluciones Técnicas - VetClinic Pro

Este documento registra los incidentes, errores y comportamientos anómalos detectados durante el desarrollo, despliegue y ejecución del sistema **VetClinic Pro**, detallando el síntoma, la causa raíz a nivel de arquitectura/runtime, la solución implementada y las lecciones aprendidas para garantizar la estabilidad de la estación monopuesto.

---

## Estructura de Cada Registro

Cada entrada sigue el siguiente formato estándar:
- **ID del Incidente:** Código único (`INC-XXX`).
- **Fecha y Componente:** Módulo, capa o subsistema involucrado.
- **Severidad:** Crítica (bloquea compilación o ejecución), Alta (falla funcional), Media (inconsistencia parcial), Baja (cosmética/advertencia).
- **Síntoma Observado:** Mensaje de error, código de salida o conducta visible.
- **Causa Raíz Técnica:** Explicación a fondo del motivo del fallo (runtime, threading, persistencia, etc.).
- **Solución Implementada:** Código, configuración o comandos aplicados.
- **Verificación y Pruebas:** Cómo se comprobó la resolución.

---

## Registro de Incidentes

### INC-001: Ausencia del SDK de .NET 8 y Origen de Paquetes NuGet no Registrado

| Atributo | Detalle |
| :--- | :--- |
| **ID** | INC-001 |
| **Fecha** | 2026-10-05 |
| **Componente** | Entorno de Ejecución / Tooling CLI (.NET SDK & NuGet) |
| **Severidad** | Crítica (Bloqueante para compilación) |

#### 1. Síntoma Observado
Al ejecutar los comandos oficiales de compilación y prueba (`dotnet build VetClinicSolution.sln`):
```text
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'build' does not exist.
  * You intended to execute a .NET SDK command:
      No .NET SDKs were found.
```
Tras ubicar el binario `dotnet.exe`, la salida de `dotnet --info` confirmó:
```text
.NET SDKs installed:
  No SDKs were found.

.NET runtimes installed:
  Microsoft.NETCore.App 8.0.30
  Microsoft.WindowsDesktop.App 8.0.30
```
Posteriormente, al intentar restaurar dependencias con el SDK ya colocado, falló la descarga de paquetes (`NU1100: No se puede resolver 'Microsoft.EntityFrameworkCore.Sqlite (>= 8.0.10)'`) debido a que la lista de orígenes de NuGet estaba vacía (`No se encontró ningún origen`).

#### 2. Causa Raíz Técnica
1. **Diferencia entre Runtime y SDK:** El equipo disponía únicamente de los paquetes de tiempo de ejecución redistribuibles (*Runtimes* de .NET Core y Windows Desktop para usuarios finales), los cuales permiten correr ejecutables previamente compilados pero no incluyen el compilador Roslyn, los targets de MSBuild ni las herramientas de desarrollo necesarias para compilar código fuente en C# 12 / WPF.
2. **Inexistencia de credenciales administrativas para Winget:** La instalación mediante el gestor de paquetes de Windows requería privilegios elevados (UAC), ausentes en la sesión de terminal.
3. **Ausencia de `nuget.org` en la configuración global de NuGet:** El archivo de configuración de NuGet a nivel de usuario (`NuGet.Config`) carecía del feed oficial público de paquetes `api.nuget.org`.

#### 3. Solución Implementada
1. **Instalación de SDK en Espacio de Usuario:** Se descargó y ejecutó el script oficial de instalación desatendida de Microsoft (`dotnet-install.ps1`), instalando el SDK .NET `8.0.425` (x64) en el directorio local del usuario sin requerir elevación de privilegios:
   ```powershell
   Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "$env:TEMP\dotnet-install.ps1"
   & "$env:TEMP\dotnet-install.ps1" -Channel 8.0 -InstallDir "$env:LocalAppData\Microsoft\dotnet" -Architecture x64
   ```
2. **Configuración de Variables de Entorno Permanentes:**
   ```powershell
   [Environment]::SetEnvironmentVariable("DOTNET_ROOT", "$env:LocalAppData\Microsoft\dotnet", "User")
   [Environment]::SetEnvironmentVariable("PATH", "$env:LocalAppData\Microsoft\dotnet;" + [Environment]::GetEnvironmentVariable("PATH", "User"), "User")
   ```
3. **Alta del Repositorio Oficial NuGet:**
   ```powershell
   dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
   ```

#### 4. Verificación y Pruebas
- `dotnet --info` reconoció el SDK `8.0.425` y el SDK de escritorio `Microsoft.WindowsDesktop.App`.
- `dotnet restore VetClinicSolution.sln` restauró con éxito las dependencias (EF Core Sqlite, QuestPDF, DependencyInjection, xUnit).

---

### INC-002: Cierre Prematuro y Silencioso de la Aplicación por Ciclo de Vida Asíncrono en WPF

| Atributo | Detalle |
| :--- | :--- |
| **ID** | INC-002 |
| **Fecha** | 2026-10-05 |
| **Componente** | `VetClinic.Presentation` (`App.xaml.cs` / WPF Application Lifecycle) |
| **Severidad** | Crítica (La aplicación terminaba de inmediato sin abrir la ventana de login) |

#### 1. Síntoma Observado
Al arrancar el ejecutable `VetClinic.Presentation.exe` o invocar `dotnet run`:
- El proceso iniciaba, consumía entre 70 MB y 170 MB de memoria durante 1 a 2 segundos y se cerraba de forma inmediata con código de salida `0` o `-1`.
- Ninguna ventana aparecía en pantalla (`LoginView`).
- No se registraban errores no controlados en el Visor de Eventos de Windows (`Application Error` o `.NET Runtime`), lo que indicaba que el proceso no sufría un *crash*, sino una finalización programada (*graceful shutdown*).

#### 2. Causa Raíz Técnica
El problema se debió a una interacción destructiva entre el despachador de mensajes de WPF (`Dispatcher`), el modo de apagado por defecto (`ShutdownMode`) y el uso de `async void` en el ciclo de vida del arranque:

1. **`ShutdownMode.OnLastWindowClose` por defecto:** WPF tiene como directiva predeterminada apagar el proceso cuando se cierra la última ventana activa. Sin embargo, en la implementación inicial de WPF, **si el bucle de despacho procesa mensajes y encuentra cero ventanas abiertas (`Windows.Count == 0`), asume que la aplicación ya cerró todas sus ventanas y llama a `Application.Shutdown()`**.
2. **Firma `async void OnStartup`:** El método `OnStartup` en `App.xaml.cs` estaba marcado como `async void`:
   ```csharp
   protected override async void OnStartup(StartupEventArgs e)
   {
       base.OnStartup(e);
       // ...
       await DbInitializer.InitializeAsync(dbContext, passwordHasher); // <-- CEDE EL CONTROL AL DISPATCHER
       // ...
       loginView.Show(); // NUNCA LLEGABA A TIEMPO
   }
   ```
3. **Secuencia de la Falla:**
   - La aplicación llamaba a `App.Run()`.
   - `OnStartup` comenzaba a ejecutarse en el hilo principal STA.
   - Al llegar a `await DbInitializer.InitializeAsync(...)`, el método retornaba un `Task` incompleto y **cedía de inmediato el hilo principal de vuelta al ciclo de mensajes de WPF**.
   - En ese instante exacto, `loginView.Show()` no había sido ejecutado. WPF consultó su lista interna de ventanas: `Application.Current.Windows.Count` era igual a `0`.
   - WPF consideró que la última ventana había cerrado e inició la secuencia de apagado (`Shutdown()`) antes de que `DbInitializer` concluyera su operación sobre SQLite.
4. **Pérdida del contexto STA por llamadas asíncronas de EF Core:** Internamente, las operaciones asíncronas de EF Core (`EnsureCreatedAsync`, `ExecuteSqlRawAsync`) pueden reanudar continuaciones en hilos del `ThreadPool` si no hay sincronización explícita, provocando violaciones de afinidad de hilo en componentes WPF (`InvalidOperationException: The calling thread must be STA`).

#### 3. Solución Implementada
Se reestructuró `App.xaml.cs` corrigiendo el ciclo de vida de la aplicación:

1. **Inicialización Síncrona Garantizada en Hilo STA:** Se eliminó la firma `async void` de `OnStartup`. La inicialización de la base de datos local SQLite y sembrado de datos se ejecuta de manera síncrona en el hilo principal antes de renderizar la primera vista:
   ```csharp
   DbInitializer.InitializeAsync(dbContext, passwordHasher).GetAwaiter().GetResult();
   ```
2. **Control Explícito del Modo de Apagado:** Se fijó `ShutdownMode = ShutdownMode.OnExplicitShutdown;` para impedir que WPF cierre la aplicación por cuenta propia mientras se instancian los servicios y vistas.
3. **Asignación Explícita de `MainWindow`:** Se asignó formalmente la ventana activa principal para que Windows y el administrador de ventanas de WPF reconozcan el `WindowHandle`:
   ```csharp
   MainWindow = loginView;
   ```
4. **Ciclo de Cierre Controlado:** Se conectaron los eventos `Closed` de `LoginView` y `ShellView`:
   - Si se cierra `LoginView` sin autenticarse (y `ShellView` no está visible), se apaga la aplicación limpiamente.
   - Si se cierra `ShellView`, se apaga la aplicación.
   - Al cerrar sesión, la propiedad `MainWindow` regresa a `LoginView` de forma transparente.

```csharp
// Fragmento definitivo en VetClinic.Presentation/App.xaml.cs:
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    ShutdownMode = ShutdownMode.OnExplicitShutdown;

    var services = new ServiceCollection();
    ConfigureServices(services);

    _serviceProvider = services.BuildServiceProvider();

    // Inicializar base de datos SQLite local, triggers y datos semilla de forma síncrona
    using (var scope = _serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        DbInitializer.InitializeAsync(dbContext, passwordHasher).GetAwaiter().GetResult();
    }

    var loginView = _serviceProvider.GetRequiredService<LoginView>();
    var shellView = _serviceProvider.GetRequiredService<ShellView>();
    var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
    var shellViewModel = _serviceProvider.GetRequiredService<ShellViewModel>();

    MainWindow = loginView;

    loginView.Closed += (s, ev) =>
    {
        if (!shellView.IsVisible)
        {
            Shutdown();
        }
    };

    shellView.Closed += (s, ev) =>
    {
        Shutdown();
    };

    loginViewModel.LoginSucceeded += usuario =>
    {
        shellViewModel.UsuarioActivo = usuario.NombreCompleto;
        shellViewModel.EjecutarNavegacion("Propietarios");
        MainWindow = shellView;
        shellView.Show();
        loginView.Hide();
    };

    shellViewModel.CerrarSesionSolicitado += () =>
    {
        MainWindow = loginView;
        shellView.Hide();
        loginViewModel.Password = string.Empty;
        loginView.Show();
    };

    loginView.Show();
}
```

#### 4. Verificación y Pruebas
1. **Persistencia SQLite:** La base de datos local `%LocalAppData%\VetClinic\vetclinic_local.db` se generó con integridad total (tablas, triggers de inmutabilidad y semillas de veterinarios y administrador).
2. **Estabilidad de Ejecución:** El proceso `VetClinic.Presentation` (PID 7496) permanece activo, consumiendo ~174 MB, respondiendo continuamente (`Responding = True`) y mostrando la ventana centrada de inicio de sesión en pantalla.
3. **Pruebas de Regresión:** La suite completa de 79 pruebas unitarias e integración en `VetClinic.Domain.Tests` y `VetClinic.Infrastructure.Tests` pasó al 100% sin roturas.

---

## Directrices para Registro de Futuros Errores

Al identificar un nuevo comportamiento anómalo o defecto en el software:
1. **Reproducción Aislada:** Comprobar si ocurre durante la compilación (`dotnet build`), ejecución de pruebas (`dotnet test`) o en tiempo de ejecución (`dotnet run`).
2. **Inspección de Hilos y Logs:** En aplicaciones de escritorio (WPF/Windows Desktop), verificar siempre si el fallo ocurre en el hilo STA de la UI o en hilos de fondo (`ThreadPool`), y revisar la salida del depurador y eventos de Windows.
3. **Documentación Oportuna:** Agregar la nueva sección con el código `INC-XXX` manteniendo la rigurosidad técnica, el contexto de negocio según `GEMINI.md` y la explicación comprensible para el equipo clínico y de desarrollo.
