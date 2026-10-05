using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetClinic.Domain.Entities;

namespace VetClinic.Infrastructure.Data.Configurations;

public class PropietarioConfiguration : IEntityTypeConfiguration<Propietario>
{
    public void Configure(EntityTypeBuilder<Propietario> builder)
    {
        builder.ToTable("Propietarios");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TipoDocumento)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.NumeroDocumento)
            .IsRequired();

        builder.HasIndex(p => p.NumeroDocumento)
            .IsUnique()
            .HasDatabaseName("IX_Propietarios_Documento");

        builder.Property(p => p.Nombres)
            .IsRequired();

        builder.Property(p => p.Apellidos)
            .IsRequired();

        builder.Property(p => p.Telefono)
            .IsRequired();

        builder.HasIndex(p => p.Telefono)
            .HasDatabaseName("IX_Propietarios_Telefono");

        builder.HasIndex(p => new { p.Nombres, p.Apellidos })
            .HasDatabaseName("IX_Propietarios_NombresApellidos");

        builder.Property(p => p.Direccion);

        builder.Property(p => p.Email);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Borrado lógico (STF-05)
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.Pacientes)
            .WithOne(pac => pac.Propietario)
            .HasForeignKey(pac => pac.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
