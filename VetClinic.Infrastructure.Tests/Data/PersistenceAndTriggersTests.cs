using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Repositories;
using VetClinic.Infrastructure.Security;
using Xunit;

namespace VetClinic.Infrastructure.Tests.Data;

public class PersistenceAndTriggersTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VetClinicDbContext _context;
    private readonly PasswordHasher _passwordHasher;

    public PersistenceAndTriggersTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VetClinicDbContext(options);
        _passwordHasher = new PasswordHasher();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task DbInitializer_SiembraVeterinariosYAdmin_Exitosamente_RN01_RN02_S03()
    {
        // Act
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        // Assert
        var veterinarios = await _context.Veterinarios.ToListAsync();
        Assert.Equal(2, veterinarios.Count);
        Assert.Contains(veterinarios, v => v.Nombre == "Dr. Fabio");
        Assert.Contains(veterinarios, v => v.Nombre == "Dr. William");

        var admin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
        Assert.NotNull(admin);
        Assert.True(_passwordHasher.VerifyPassword("Clinica2026*", admin.PasswordHash, admin.PasswordSalt));
    }

    [Fact]
    public async Task Trigger_AtencionClinica_ActualizaPesoPacienteAutomaticamente_RF06()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var propietario = new Propietario
        {
            NumeroDocumento = "1012345678",
            Nombres = "Carlos",
            Apellidos = "Pérez",
            Telefono = "3001112233"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Rocky",
            Especie = Especie.Canino,
            Raza = "Criollo",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-2),
            PesoActualKg = 10.0
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        // Act: Insertar atención con nuevo peso 15.75 Kg
        var atencion = new AtencionClinica
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            FechaHoraAtencion = DateTime.Now,
            PesoConsultaKg = 15.75,
            MotivoConsulta = "Control anual",
            Diagnostico = "Sano",
            Tratamiento = "Ninguno"
        };
        _context.AtencionesClinicas.Add(atencion);
        await _context.SaveChangesAsync();

        // Limpiar caché local de EF Core para leer directo de SQLite
        _context.Entry(paciente).Reload();

        // Assert: Trigger actualizó PesoActualKg en tabla Pacientes
        Assert.Equal(15.75, paciente.PesoActualKg);
    }

    [Fact]
    public async Task Trigger_AtencionClinica_BloqueaUpdate_ArrojaExcepcion_RN07_STF05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var propietario = new Propietario
        {
            NumeroDocumento = "1023456789",
            Nombres = "María",
            Apellidos = "Gómez",
            Telefono = "3102223344"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Luna",
            Especie = Especie.Felino,
            Raza = "Siamés",
            Sexo = Sexo.Hembra,
            FechaNacimiento = DateTime.Today.AddYears(-1),
            PesoActualKg = 4.2
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        var atencion = new AtencionClinica
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            FechaHoraAtencion = DateTime.Now,
            PesoConsultaKg = 4.2,
            MotivoConsulta = "Vacunación",
            Diagnostico = "Sano",
            Tratamiento = "Vacuna aplicada"
        };
        _context.AtencionesClinicas.Add(atencion);
        await _context.SaveChangesAsync();

        // Act & Assert: Intentar UPDATE debe fallar por el trigger de inmutabilidad
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            atencion.Diagnostico = "Diagnóstico modificado indebidamente";
            await _context.SaveChangesAsync();
        });

        Assert.Contains("Violación Ética y Legal", ex.InnerException?.Message ?? ex.Message);
    }

    [Fact]
    public async Task Trigger_AtencionClinica_BloqueaDelete_ArrojaExcepcion_RN07_STF05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var propietario = new Propietario
        {
            NumeroDocumento = "1034567890",
            Nombres = "Laura",
            Apellidos = "Torres",
            Telefono = "3203334455"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Simba",
            Especie = Especie.Felino,
            Raza = "Persa",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddMonths(-6),
            PesoActualKg = 2.8
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        var atencion = new AtencionClinica
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            FechaHoraAtencion = DateTime.Now,
            PesoConsultaKg = 2.8,
            MotivoConsulta = "Consulta general",
            Diagnostico = "Gastroenteritis leve",
            Tratamiento = "Probióticos"
        };
        _context.AtencionesClinicas.Add(atencion);
        await _context.SaveChangesAsync();

        // Act & Assert: Intentar DELETE debe ser abortado por el trigger
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            _context.AtencionesClinicas.Remove(atencion);
            await _context.SaveChangesAsync();
        });

        Assert.Contains("Violación Ética y Legal", ex.InnerException?.Message ?? ex.Message);
    }

    [Fact]
    public async Task Trigger_Inmunizaciones_BloqueaUpdateYDelete_RN08_STF05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var propietario = new Propietario
        {
            NumeroDocumento = "1045678901",
            Nombres = "Juan",
            Apellidos = "Díaz",
            Telefono = "3004445566"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Max",
            Especie = Especie.Canino,
            Raza = "Golden Retriever",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-3),
            PesoActualKg = 30.0
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        var inmunizacion = new Inmunizacion
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            TipoBiologico = TipoBiologico.Vacuna,
            NombreProducto = "Rabia Canina",
            FechaAplicacion = DateTime.Today,
            FechaRefuerzo = DateTime.Today.AddYears(1)
        };
        _context.Inmunizaciones.Add(inmunizacion);
        await _context.SaveChangesAsync();

        // Act & Assert: Intentar UPDATE
        var exUpdate = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            inmunizacion.NombreProducto = "Producto Alterado";
            await _context.SaveChangesAsync();
        });
        Assert.Contains("inmutables", exUpdate.InnerException?.Message ?? exUpdate.Message);

        // Descartar cambios de la entidad modificada en el contexto
        _context.Entry(inmunizacion).State = EntityState.Unchanged;

        // Act & Assert: Intentar DELETE
        var exDelete = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            _context.Inmunizaciones.Remove(inmunizacion);
            await _context.SaveChangesAsync();
        });
        Assert.Contains("eliminarse", exDelete.InnerException?.Message ?? exDelete.Message);
    }

    [Fact]
    public async Task SoftDelete_PropietarioYPaciente_OcultaRegistrosSinBorradoFisico_STF05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);

        var propietario = new Propietario
        {
            NumeroDocumento = "1056789012",
            Nombres = "Pedro",
            Apellidos = "Martínez",
            Telefono = "3155556677"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        // Act: Marcar borrado lógico
        propietario.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Assert: Query normal no retorna el registro debido al QueryFilter
        var propActivos = await _context.Propietarios.ToListAsync();
        Assert.DoesNotContain(propActivos, p => p.Id == propietario.Id);

        // Assert: Con IgnoreQueryFilters() el registro sigue existiendo físicamente en BD
        var propFisico = await _context.Propietarios.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == propietario.Id);
        Assert.NotNull(propFisico);
        Assert.True(propFisico.IsDeleted);
    }

    [Fact]
    public async Task AtencionClinicaRepository_GetHistorial_RetornaOrdenCronologicoDescendente_RN07_RNF05()
    {
        // Arrange
        await DbInitializer.InitializeAsync(_context, _passwordHasher);
        var repo = new AtencionClinicaRepository(_context);

        var propietario = new Propietario
        {
            NumeroDocumento = "1067890123",
            Nombres = "Andrés",
            Apellidos = "Castillo",
            Telefono = "3206667788"
        };
        _context.Propietarios.Add(propietario);
        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            PropietarioId = propietario.Id,
            Nombre = "Toby",
            Especie = Especie.Canino,
            Raza = "Beagle",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-1),
            PesoActualKg = 12.0
        };
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        var veterinario = await _context.Veterinarios.FirstAsync();

        var atencionAntigua = new AtencionClinica
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            FechaHoraAtencion = DateTime.Now.AddDays(-30),
            PesoConsultaKg = 11.0,
            MotivoConsulta = "Chequeo mensual",
            Diagnostico = "Sano",
            Tratamiento = "Desparasitante"
        };
        var atencionReciente = new AtencionClinica
        {
            PacienteId = paciente.Id,
            VeterinarioId = veterinario.Id,
            FechaHoraAtencion = DateTime.Now,
            PesoConsultaKg = 12.0,
            MotivoConsulta = "Consulta reciente",
            Diagnostico = "Alergia cutánea",
            Tratamiento = "Antihistamínico"
        };

        _context.AtencionesClinicas.AddRange(atencionAntigua, atencionReciente);
        await _context.SaveChangesAsync();

        // Act
        var historial = await repo.GetHistorialPorPacienteAsync(paciente.Id);

        // Assert: 2 registros, el primero es la atención más reciente (RN-07)
        Assert.Equal(2, historial.Count);
        Assert.Equal(atencionReciente.Id, historial[0].Id);
        Assert.Equal(atencionAntigua.Id, historial[1].Id);
        Assert.NotNull(historial[0].Veterinario);
    }
}
