using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetClinic.Domain.Entities;

namespace VetClinic.Infrastructure.Data.Configurations;

public class InmunizacionConfiguration : IEntityTypeConfiguration<Inmunizacion>
{
    public void Configure(EntityTypeBuilder<Inmunizacion> builder)
    {
        builder.ToTable("Inmunizaciones");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.PacienteId)
            .IsRequired();

        builder.Property(i => i.VeterinarioId)
            .IsRequired();

        builder.Property(i => i.TipoBiologico)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.NombreProducto)
            .IsRequired();

        builder.Property(i => i.LoteFabricante);

        builder.Property(i => i.FechaAplicacion)
            .IsRequired();

        builder.Property(i => i.FechaRefuerzo)
            .IsRequired();

        builder.Property(i => i.Observaciones);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasIndex(i => new { i.PacienteId, i.FechaAplicacion })
            .HasDatabaseName("IX_Inmunizaciones_Paciente_Fecha");

        builder.HasIndex(i => i.FechaRefuerzo)
            .HasDatabaseName("IX_Inmunizaciones_Refuerzo");

        builder.HasOne(i => i.Paciente)
            .WithMany(p => p.Inmunizaciones)
            .HasForeignKey(i => i.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Veterinario)
            .WithMany(v => v.Inmunizaciones)
            .HasForeignKey(i => i.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
