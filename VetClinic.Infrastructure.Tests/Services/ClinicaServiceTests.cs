using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Repositories;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Services;

public class ClinicaServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VetClinicDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly ClinicaService _clinicaService;

    public ClinicaServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VetClinicDbContext(options);
        _passwordHasher = new PasswordHasher();

        var propietarioRepo = new PropietarioRepository(_context);
        var pacienteRepo = new PacienteRepository(_context);
        var atencionRepo = new AtencionClinicaRepository(_context);
        var inmunizacionRepo = new InmunizacionRepository(_context);
        var unitOfWork = new UnitOfWork(_context);

        _clinicaService = new ClinicaService(
            propietarioRepo,
            pacienteRepo,
            atencionRepo,
            inmunizacionRepo,
            unitOfWork,
            _context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CrearPropietarioAsync_DatosValidos_RegistraExitosamente_RF02_RN04()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var propietario = new Propietario
        {
            TipoDocumento = TipoDocumento.CC,
            NumeroDocumento = "1020304050",
            Nombres = "Carlos",
            Apellidos = "Mendoza",
            Telefono = "3158765432",
            Direccion = "Calle 123 #45-67",
            Email = "carlos.mendoza@email.com"
        };

        // Act
        var result = await _clinicaService.CrearPropietarioAsync(propietario);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Id > 0);
        Assert.Equal("3158765432", result.Value.Telefono);
    }

    [Fact]
    public async Task CrearPropietarioAsync_DocumentoDuplicado_RetornaFallo_RF02_RN03()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var prop1 = new Propietario
        {
            NumeroDocumento = "80123456",
            Nombres = "Ana",
            Apellidos = "Gómez",
            Telefono = "3001234567"
        };
        await _clinicaService.CrearPropietarioAsync(prop1);

        var prop2 = new Propietario
        {
            NumeroDocumento = "80123456",
            Nombres = "Ana María",
            Apellidos = "Gómez",
            Telefono = "3109876543"
        };

        // Act
        var result = await _clinicaService.CrearPropietarioAsync(prop2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Ya existe un propietario registrado con el documento especificado", result.Error.Message);
    }

    [Fact]
    public async Task CrearPropietarioAsync_CelularInvalido_RetornaFallo_RF02_RN04()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var propietario = new Propietario
        {
            NumeroDocumento = "1098765432",
            Nombres = "Pedro",
            Apellidos = "Pérez",
            Telefono = "2145678901" // Fijo o no inicia con 3
        };

        // Act
        var result = await _clinicaService.CrearPropietarioAsync(propietario);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("10 dígitos numéricos iniciando con 3", result.Error.Message);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_DatosValidos_RegistraExitosamente_RF04_RN05_RN06()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var propietario = new Propietario
        {
            NumeroDocumento = "11223344",
            Nombres = "Lucía",
            Apellidos = "Sánchez",
            Telefono = "3123456789"
        };
        await _clinicaService.CrearPropietarioAsync(propietario);

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Rocky",
            Especie = Especie.Canino,
            Raza = "Golden Retriever",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-2),
            PesoActualKg = 28.5
        };

        // Act
        var result = await _clinicaService.RegistrarPacienteAsync(paciente);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Id > 0);
        Assert.Contains("2 años", result.Value.EdadFormateada);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_FechaNacimientoFutura_RetornaFallo_RF04()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var paciente = new Paciente
        {
            PropietarioId = 1,
            Nombre = "Michi",
            Raza = "Criollo",
            FechaNacimiento = DateTime.Today.AddDays(5),
            PesoActualKg = 3.5
        };

        // Act
        var result = await _clinicaService.RegistrarPacienteAsync(paciente);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("posterior a la fecha actual", result.Error.Message);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_PesoFueraDeRango_RetornaFallo_RN06()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var paciente = new Paciente
        {
            PropietarioId = 1,
            Nombre = "Gigante",
            Raza = "San Bernardo",
            FechaNacimiento = DateTime.Today.AddYears(-1),
            PesoActualKg = 180.0 // Límite máximo es 150 Kg
        };

        // Act
        var result = await _clinicaService.RegistrarPacienteAsync(paciente);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("peso corporal", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistrarAtencionAsync_SinVeterinario_RetornaFallo_RN02_CU03()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var atencion = new AtencionClinica
        {
            PacienteId = 1,
            VeterinarioId = 0, // No asignado
            MotivoConsulta = "Control anual",
            Diagnostico = "Sano",
            Tratamiento = "Vacunación",
            PesoConsultaKg = 12.0
        };

        // Act
        var result = await _clinicaService.RegistrarAtencionAsync(atencion);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Dr. Fabio o Dr. William", result.Error.Message);
    }

    [Fact]
    public async Task RegistrarInmunizacionAsync_RefuerzoAnteriorAAplicacion_RetornaFallo_RN08_CU05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var inmunizacion = new Inmunizacion
        {
            PacienteId = 1,
            VeterinarioId = 1,
            NombreProducto = "Rabia Canina",
            FechaAplicacion = DateTime.Today,
            FechaRefuerzo = DateTime.Today.AddDays(-10) // Inválido
        };

        // Act
        var result = await _clinicaService.RegistrarInmunizacionAsync(inmunizacion);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("posterior a la fecha de aplicación", result.Error.Message);
    }
}
