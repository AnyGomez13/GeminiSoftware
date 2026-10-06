using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;
using VetClinic.Infrastructure.Data;
using VetClinic.Infrastructure.Repositories;
using VetClinic.Infrastructure.Security;
using VetClinic.Infrastructure.Services;

namespace VetClinic.Api.Tests.TestHelpers;

public class TestDatabaseFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public VetClinicDbContext Context { get; }
    public PasswordHasher PasswordHasher { get; }
    public ClinicaService ClinicaService { get; }
    public AuthService AuthService { get; }
    public QuestPdfExportService PdfExportService { get; }

    public TestDatabaseFixture()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<VetClinicDbContext>()
            .UseSqlite(Connection)
            .Options;

        Context = new VetClinicDbContext(options);
        PasswordHasher = new PasswordHasher();

        var propietarioRepo = new PropietarioRepository(Context);
        var pacienteRepo = new PacienteRepository(Context);
        var atencionRepo = new AtencionClinicaRepository(Context);
        var inmunizacionRepo = new InmunizacionRepository(Context);
        var unitOfWork = new UnitOfWork(Context);

        ClinicaService = new ClinicaService(
            propietarioRepo,
            pacienteRepo,
            atencionRepo,
            inmunizacionRepo,
            unitOfWork,
            Context);

        AuthService = new AuthService(Context, PasswordHasher);
        PdfExportService = new QuestPdfExportService(Context);
    }

    public async Task InitializeAsync()
    {
        await DbInitializer.InitializeAsync(Context, PasswordHasher);

        // Sembrar Propietario y Paciente base para pruebas de controladores
        var prop = new Propietario
        {
            TipoDocumento = TipoDocumento.CC,
            NumeroDocumento = "1234567890",
            Nombres = "Juan",
            Apellidos = "Pérez",
            Telefono = "3101234567",
            Email = "juan.perez@email.com",
            Direccion = "Calle 1 # 2-3"
        };
        Context.Propietarios.Add(prop);
        await Context.SaveChangesAsync();

        var pac = new Paciente
        {
            PropietarioId = prop.Id,
            Nombre = "Toby",
            Especie = Especie.Canino,
            Raza = "Golden Retriever",
            Sexo = Sexo.Macho,
            FechaNacimiento = DateTime.Today.AddYears(-3),
            EsFechaEstimada = false,
            PesoActualKg = 30.0,
            ColorSenas = "Dorado",
            EstadoReproductivo = EstadoReproductivo.Entero
        };
        Context.Pacientes.Add(pac);
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
