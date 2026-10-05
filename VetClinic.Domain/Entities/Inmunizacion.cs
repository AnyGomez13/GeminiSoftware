using VetClinic.Domain.Common;
using VetClinic.Domain.Enums;

namespace VetClinic.Domain.Entities;

public class Inmunizacion : BaseEntity
{
    public int PacienteId { get; set; }
    public int VeterinarioId { get; set; }
    public TipoBiologico TipoBiologico { get; set; } = TipoBiologico.Vacuna;
    public string NombreProducto { get; set; } = string.Empty;
    public string? LoteFabricante { get; set; }
    public DateTime FechaAplicacion { get; set; } = DateTime.Today;
    public DateTime FechaRefuerzo { get; set; }
    public string? Observaciones { get; set; }

    public Paciente? Paciente { get; set; }
    public Veterinario? Veterinario { get; set; }
}
