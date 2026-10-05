using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetClinic.Domain.Entities;

namespace VetClinic.Infrastructure.Data.Configurations;

public class AtencionClinicaConfiguration : IEntityTypeConfiguration<AtencionClinica>
{
    public void Configure(EntityTypeBuilder<AtencionClinica> builder)
    {
        builder.ToTable("AtencionesClinicas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PacienteId)
            .IsRequired();

        builder.Property(a => a.VeterinarioId)
            .IsRequired();

        builder.Property(a => a.FechaHoraAtencion)
            .IsRequired();

        builder.Property(a => a.PesoConsultaKg)
            .IsRequired();

        builder.Property(a => a.MotivoConsulta)
            .IsRequired();

        builder.Property(a => a.ExamenClinico);

        builder.Property(a => a.Diagnostico)
            .IsRequired();

        builder.Property(a => a.Tratamiento)
            .IsRequired();

        builder.Property(a => a.Indicaciones);

        builder.Property(a => a.FechaControl);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Índice compuesto para acelerar consultas cronológicas descendentes (RNF-05)
        builder.HasIndex(a => new { a.PacienteId, a.FechaHoraAtencion })
            .HasDatabaseName("IX_Atenciones_Paciente_FechaHora");

        builder.HasOne(a => a.Paciente)
            .WithMany(p => p.AtencionesClinicas)
            .HasForeignKey(a => a.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Veterinario)
            .WithMany(v => v.AtencionesClinicas)
            .HasForeignKey(a => a.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
