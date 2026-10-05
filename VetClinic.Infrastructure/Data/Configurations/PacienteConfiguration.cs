using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Enums;

namespace VetClinic.Infrastructure.Data.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PropietarioId)
            .IsRequired();

        builder.HasIndex(p => p.PropietarioId)
            .HasDatabaseName("IX_Pacientes_PropietarioId");

        builder.Property(p => p.Nombre)
            .IsRequired();

        builder.HasIndex(p => p.Nombre)
            .HasDatabaseName("IX_Pacientes_Nombre");

        builder.Property(p => p.Especie)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Raza)
            .IsRequired();

        builder.Property(p => p.Sexo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.FechaNacimiento)
            .IsRequired();

        builder.Property(p => p.EsFechaEstimada)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.PesoActualKg)
            .IsRequired();

        builder.Property(p => p.ColorSenas);

        builder.Property(p => p.EstadoReproductivo)
            .IsRequired()
            .HasConversion(
                v => v == EstadoReproductivo.CastradoEsterilizado ? "Castrado/Esterilizado" : "Entero",
                v => v == "Castrado/Esterilizado" ? EstadoReproductivo.CastradoEsterilizado : EstadoReproductivo.Entero);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Propiedad calculada en memoria, ignorada en BD (STF-06)
        builder.Ignore(p => p.EdadFormateada);

        // Borrado lógico (STF-05)
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Propietario)
            .WithMany(prop => prop.Pacientes)
            .HasForeignKey(p => p.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.AtencionesClinicas)
            .WithOne(a => a.Paciente)
            .HasForeignKey(a => a.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Inmunizaciones)
            .WithOne(i => i.Paciente)
            .HasForeignKey(i => i.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
