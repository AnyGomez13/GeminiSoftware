using VetClinic.Domain.Common;
using VetClinic.Domain.Enums;

namespace VetClinic.Domain.Entities;

public class Propietario : BaseEntity
{
    public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.CC;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;

    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();

    public string GetNombreCompleto() => $"{Nombres} {Apellidos}".Trim();
    public string NombreCompleto => GetNombreCompleto();
}
