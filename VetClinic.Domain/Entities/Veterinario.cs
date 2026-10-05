using VetClinic.Domain.Common;

namespace VetClinic.Domain.Entities;

public class Veterinario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string TarjetaProfesional { get; set; } = "COMVEZCOL-PENDIENTE";
    public bool IsActive { get; set; } = true;

    public ICollection<AtencionClinica> AtencionesClinicas { get; set; } = new List<AtencionClinica>();
    public ICollection<Inmunizacion> Inmunizaciones { get; set; } = new List<Inmunizacion>();
}
