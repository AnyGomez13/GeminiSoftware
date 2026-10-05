using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Services;

public class QuestPdfExportServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VetClinicDbContext _context;
    private readonly QuestPdfExportService _pdfService;
    private readonly string _tempPdfPath;

    public QuestPdfExportServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VetClinicDbContext(options);
        _pdfService = new QuestPdfExportService(_context);
        _tempPdfPath = Path.Combine(Path.GetTempPath(), $"Carnet_Test_{Guid.NewGuid():N}.pdf");
    }

    public void Dispose()
    {
        if (File.Exists(_tempPdfPath))
        {
            try { File.Delete(_tempPdfPath); } catch { /* Ignore */ }
        }

        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GenerarCarnetVacunacionAsync_PacienteConInmunizaciones_GeneraPdfEnMenorDe3Segundos_RNF06_RF09()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, new PasswordHasher());

        var propietario = new Propietario
        {
            TipoDocumento = TipoDocumento.CC,
            NumeroDocumento = "1098765432",
            Nombres = "Diana",
            Apellidos = "Restrepo",
            Telefono = "3109876543",
            Email = "diana@example.com"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Lucas",
            Especie = Especie.Canino,
            Raza = "Poodle",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-2).AddMonths(-3),
            PesoActualKg = 6.50
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        var vacuna1 = new Inmunizacion
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            TipoBiologico = TipoBiologico.Vacuna,
            NombreProducto = "Séxtuple Canina",
            LoteFabricante = "LT-99238",
            FechaAplicacion = DateTime.Today.AddMonths(-6),
            FechaRefuerzo = DateTime.Today.AddMonths(6)
        };
        var vacuna2 = new Inmunizacion
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            TipoBiologico = TipoBiologico.Desparasitante,
            NombreProducto = "Total F Total",
            LoteFabricante = "DS-4412",
            FechaAplicacion = DateTime.Today.AddMonths(-1),
            FechaRefuerzo = DateTime.Today.AddMonths(2)
        };
        _context.Inmunizaciones.AddRange(vacuna1, vacuna2);
        await _context.SaveChangesAsync();

        // Act
        var cronometro = Stopwatch.StartNew();
        var resultado = await _pdfService.GenerarCarnetVacunacionAsync(paciente.Id, _tempPdfPath);
        cronometro.Stop();

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(_tempPdfPath, resultado.Value);
        Assert.True(File.Exists(_tempPdfPath));

        var fileInfo = new FileInfo(_tempPdfPath);
        Assert.True(fileInfo.Length > 1000, "El archivo PDF generado debe contener contenido binario válido.");

        // Verificación de RNF-06: compilación y escritura en disco en <= 3 segundos (3000 ms)
        Assert.True(cronometro.ElapsedMilliseconds <= 3000,
            $"La generación del carnet PDF excedió los 3 segundos: {cronometro.ElapsedMilliseconds} ms.");
    }

    [Fact]
    public async Task GenerarCarnetVacunacionAsync_PacienteInexistente_RetornaFallo()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, new PasswordHasher());

        // Act
        var resultado = await _pdfService.GenerarCarnetVacunacionAsync(9999, _tempPdfPath);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Contains("No se encontró el paciente", resultado.Error.Message);
        Assert.False(File.Exists(_tempPdfPath));
    }
}
