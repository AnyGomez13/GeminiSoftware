using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(VetClinicDbContext context, IPasswordHasher passwordHasher)
    {
        // Asegurar creación de base de datos
        await context.Database.EnsureCreatedAsync();

        // 1. Ejecutar PRAGMAs obligatorios de rendimiento e integridad (RNF-05, RNF-08)
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");
        await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL;");
        await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 5000;");

        // 2. Disparadores de Inmutabilidad y Lógica de Negocio (RN-07, STF-05)
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_ActualizarPesoPaciente
            AFTER INSERT ON AtencionesClinicas
            BEGIN
                UPDATE Pacientes
                SET PesoActualKg = NEW.PesoConsultaKg,
                    UpdatedAt = datetime('now', 'localtime')
                WHERE Id = NEW.PacienteId;
            END;");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_PreventUpdate
            BEFORE UPDATE ON AtencionesClinicas
            BEGIN
                SELECT RAISE(ABORT, 'Violación Ética y Legal: La historia clínica confirmada no puede modificarse.');
            END;");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TRIGGER IF NOT EXISTS TR_AtencionesClinicas_PreventDelete
            BEFORE DELETE ON AtencionesClinicas
            BEGIN
                SELECT RAISE(ABORT, 'Violación Ética y Legal: La historia clínica confirmada no puede eliminarse.');
            END;");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TRIGGER IF NOT EXISTS TR_Inmunizaciones_PreventUpdate
            BEFORE UPDATE ON Inmunizaciones
            BEGIN
                SELECT RAISE(ABORT, 'Los registros de inmunización aplicados son inmutables.');
            END;");

        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TRIGGER IF NOT EXISTS TR_Inmunizaciones_PreventDelete
            BEFORE DELETE ON Inmunizaciones
            BEGIN
                SELECT RAISE(ABORT, 'Los registros de inmunización aplicados no pueden eliminarse.');
            END;");

        // 3. Sembrado de Veterinarios Oficiales (RN-02, S-03)
        if (!await context.Veterinarios.AnyAsync())
        {
            context.Veterinarios.AddRange(
                new Veterinario
                {
                    Nombre = "Dr. Fabio",
                    TarjetaProfesional = "COMVEZCOL-08421",
                    IsActive = true
                },
                new Veterinario
                {
                    Nombre = "Dr. William",
                    TarjetaProfesional = "COMVEZCOL-09134",
                    IsActive = true
                });

            await context.SaveChangesAsync();
        }

        // 4. Sembrado de Usuario Administrador Inicial (RN-01, RNF-01)
        if (!await context.Usuarios.AnyAsync())
        {
            var hash = passwordHasher.HashPassword("Clinica2026*", out var salt);

            context.Usuarios.Add(new Usuario
            {
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                NombreCompleto = "Administrador de Estación",
                Rol = "Administrador",
                IsActive = true
            });

            await context.SaveChangesAsync();
        }
    }
}
