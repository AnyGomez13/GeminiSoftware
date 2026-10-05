using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Repositories;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Integration;

public class EndToEndFlowTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VetClinicDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly AuthService _authService;
    private readonly ClinicaService _clinicaService;
    private readonly QuestPdfExportService _pdfService;
    private readonly ExternalLauncherService _launcherService;
    private readonly string _tempPdfDir;

    public EndToEndFlowTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VetClinicDbContext(options);
        _passwordHasher = new PasswordHasher();

        var propRepo = new PropietarioRepository(_context);
        var pacRepo = new PacienteRepository(_context);
        var atenRepo = new AtencionClinicaRepository(_context);
        var inmRepo = new InmunizacionRepository(_context);
        var uow = new UnitOfWork(_context);

        _authService = new AuthService(_context, _passwordHasher);
        _clinicaService = new ClinicaService(propRepo, pacRepo, atenRepo, inmRepo, uow, _context);
        _pdfService = new QuestPdfExportService(_context);
        _launcherService = new ExternalLauncherService();

        _tempPdfDir = Path.Combine(Path.GetTempPath(), "VetClinic_E2E_Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempPdfDir);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();

        if (Directory.Exists(_tempPdfDir))
        {
            try { Directory.Delete(_tempPdfDir, true); } catch { /* Ignore cleanup errors */ }
        }
    }

    [Fact]
    public async Task FlujoCompleto_JornadaClinicaVeterinaria_E2E_Aprobado()
    {
        // =========================================================================
        // 1. INICIALIZACIÓN DE LA ESTACIÓN CLÍNICA (STF-01, STF-02, STF-05)
        // =========================================================================
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var veterinarios = await _clinicaService.ObtenerVeterinariosActivosAsync();
        Assert.Equal(2, veterinarios.Count);
        var drFabio = veterinarios.First(v => v.Nombre == "Dr. Fabio");
        var drWilliam = veterinarios.First(v => v.Nombre == "Dr. William");

        // =========================================================================
        // 2. AUTENTICACIÓN OPERATIVA (CU-01, RF-01, RN-01, RNF-01)
        // =========================================================================
        var authResult = await _authService.AuthenticateAsync("admin", "Clinica2026*");
        Assert.True(authResult.IsSuccess);
        Assert.Equal("admin", authResult.Value.Username);

        // =========================================================================
        // 3. REGISTRO DE PROPIETARIO Y VALIDACIÓN CELULAR (CU-02, RF-02, RN-04)
        // =========================================================================
        var nuevoPropietario = new Propietario
        {
            TipoDocumento = TipoDocumento.CC,
            NumeroDocumento = "1098765432",
            Nombres = "María Camila",
            Apellidos = "Rodríguez",
            Telefono = "3119876543",
            Direccion = "Carrera 45 #12-34",
            Email = "maria.rodriguez@email.com"
        };
        var propResult = await _clinicaService.CrearPropietarioAsync(nuevoPropietario);
        Assert.True(propResult.IsSuccess);
        var propietarioId = propResult.Value.Id;

        // =========================================================================
        // 4. ALTA DE PACIENTE CON CÁLCULO DINÁMICO DE EDAD (RF-04, RN-05, RN-06)
        // =========================================================================
        var nuevoPaciente = new Paciente
        {
            PropietarioId = propietarioId,
            Nombre = "Toby",
            Especie = Especie.Canino,
            Raza = "Criollo Colombiano",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-2).AddMonths(-3),
            EsFechaEstimada = false,
            PesoActualKg = 15.4,
            EstadoReproductivo = EstadoReproductivo.CastradoEsterilizado
        };
        var pacResult = await _clinicaService.RegistrarPacienteAsync(nuevoPaciente);
        Assert.True(pacResult.IsSuccess);
        var pacienteId = pacResult.Value.Id;
        Assert.Contains("2 años", pacResult.Value.EdadFormateada);

        // =========================================================================
        // 5. ATENCIÓN CLÍNICA Y TRIGGER DE ACTUALIZACIÓN DE PESO (CU-03, RF-06, STF-05)
        // =========================================================================
        var nuevaAtencion = new AtencionClinica
        {
            PacienteId = pacienteId,
            VeterinarioId = drWilliam.Id,
            FechaHoraAtencion = DateTime.Now,
            PesoConsultaKg = 16.2, // Nuevo peso en consulta
            MotivoConsulta = "Cuadro de apatía y pérdida de apetito desde hace 24 horas.",
            ExamenClinico = "T: 38.8°C, FC: 110 lpm, mucosas rosadas, hidratación adecuada.",
            Diagnostico = "Gastroenteritis aguda alimentaria de curso benigno.",
            Tratamiento = "Suero oral a libre demanda, dieta blanda (pollo hervido y arroz) por 3 días.",
            FechaControl = DateTime.Today.AddDays(7)
        };
        var atencionResult = await _clinicaService.RegistrarAtencionAsync(nuevaAtencion);
        Assert.True(atencionResult.IsSuccess);

        // Verificación de Trigger SQLite: el peso del paciente fue actualizado automáticamente en base de datos
        var pacienteActualizado = await _clinicaService.ObtenerPacientePorIdAsync(pacienteId);
        Assert.NotNull(pacienteActualizado);
        Assert.Equal(16.2, pacienteActualizado.PesoActualKg);

        // =========================================================================
        // 6. INMUTABILIDAD LEGAL DE LA HISTORIA CLÍNICA (RN-07, STF-05, Ley 576 de 2000)
        // =========================================================================
        var atencionEnDb = await _context.AtencionesClinicas.FirstAsync(a => a.Id == atencionResult.Value.Id);
        atencionEnDb.Diagnostico = "Diagnóstico alterado fraudulentamente";
        var exUpdate = await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
        Assert.Contains("Violación Ética y Legal", exUpdate.InnerException?.Message ?? exUpdate.Message, StringComparison.OrdinalIgnoreCase);

        // Descartar cambios inválidos en el change tracker para continuar
        _context.Entry(atencionEnDb).State = EntityState.Unchanged;

        // Intento de DELETE también debe ser abortado por trigger
        _context.AtencionesClinicas.Remove(atencionEnDb);
        var exDelete = await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
        Assert.Contains("eliminarse", exDelete.InnerException?.Message ?? exDelete.Message, StringComparison.OrdinalIgnoreCase);

        _context.Entry(atencionEnDb).State = EntityState.Unchanged;

        // =========================================================================
        // 7. INMUNIZACIÓN Y VALIDACIÓN DE REFUERZO POSTERIOR (CU-05, RF-08, RN-08)
        // =========================================================================
        var nuevaVacuna = new Inmunizacion
        {
            PacienteId = pacienteId,
            VeterinarioId = drFabio.Id,
            TipoBiologico = TipoBiologico.Vacuna,
            NombreProducto = "Nobivac DHPPi + L4",
            LoteFabricante = "L-884210",
            FechaAplicacion = DateTime.Today,
            FechaRefuerzo = DateTime.Today.AddYears(1),
            Observaciones = "Vía subcutánea escapular izquierda sin reacciones adversas inmediatas."
        };
        var inmResult = await _clinicaService.RegistrarInmunizacionAsync(nuevaVacuna);
        Assert.True(inmResult.IsSuccess);

        // =========================================================================
        // 8. GENERACIÓN DE CARNET DIGITAL EN PDF (< 3 SEGUNDOS) (RF-09, RNF-06, STF-03)
        // =========================================================================
        var rutaPdf = Path.Combine(_tempPdfDir, "Carnet_Toby_E2E.pdf");
        var cronometro = Stopwatch.StartNew();
        var pdfResult = await _pdfService.GenerarCarnetVacunacionAsync(pacienteId, rutaPdf);
        cronometro.Stop();

        Assert.True(pdfResult.IsSuccess);
        Assert.True(File.Exists(rutaPdf));
        var infoArchivo = new FileInfo(rutaPdf);
        Assert.True(infoArchivo.Length > 1000); // PDF estructurado con contenido real
        Assert.True(cronometro.ElapsedMilliseconds < 3000); // RNF-06 cumplido

        // =========================================================================
        // 9. RECORDATORIOS Y DISPARADORES DE MENSAJERÍA GRATUITA (CU-06, RF-10, RF-11, RN-09, RNF-09)
        // =========================================================================
        var proximos = await _clinicaService.ObtenerProximosRefuerzosAsync(370);
        Assert.NotEmpty(proximos);
        var alerta = proximos.First(p => p.PacienteId == pacienteId);
        Assert.Equal("Nobivac DHPPi + L4", alerta.NombreProducto);
        Assert.Equal("María Camila Rodríguez", alerta.Paciente!.Propietario!.NombreCompleto);

        // Validar generación de URI para WhatsApp sin costos de mensajería
        var mensajeWa = $"Hola {alerta.Paciente.Propietario.NombreCompleto}, le recordamos el refuerzo de {alerta.Paciente.Nombre}.";
        var uriWa = ExternalLauncherService.ConstruirUriWhatsApp(alerta.Paciente.Propietario.Telefono, mensajeWa);
        Assert.StartsWith("https://wa.me/573119876543?text=", uriWa);

        // Validar generación de URI mailto:
        var uriMail = ExternalLauncherService.ConstruirUriCorreo(alerta.Paciente.Propietario.Email!, "Recordatorio VetClinic", "Cuerpo del correo");
        Assert.StartsWith("mailto:maria.rodriguez@email.com?subject=", uriMail);
    }
}
