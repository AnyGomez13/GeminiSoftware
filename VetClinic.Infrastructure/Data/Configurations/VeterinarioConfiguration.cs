using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetClinic.Domain.Entities;

namespace VetClinic.Infrastructure.Data.Configurations;

public class VeterinarioConfiguration : IEntityTypeConfiguration<Veterinario>
{
    public void Configure(EntityTypeBuilder<Veterinario> builder)
    {
        builder.ToTable("Veterinarios");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Nombre)
            .IsRequired();

        builder.HasIndex(v => v.Nombre)
            .IsUnique();

        builder.Property(v => v.TarjetaProfesional)
            .IsRequired()
            .HasDefaultValue("COMVEZCOL-PENDIENTE");

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(v => v.AtencionesClinicas)
            .WithOne(a => a.Veterinario)
            .HasForeignKey(a => a.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Inmunizaciones)
            .WithOne(i => i.Veterinario)
            .HasForeignKey(i => i.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
