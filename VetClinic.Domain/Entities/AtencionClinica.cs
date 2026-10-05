using VetClinic.Domain.Common;

namespace VetClinic.Domain.Entities;

public class AtencionClinica : BaseEntity
{
    public int PacienteId { get; set; }
    public int VeterinarioId { get; set; }
    public DateTime FechaHoraAtencion { get; set; } = DateTime.Now;
    public double PesoConsultaKg { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;
    public string? ExamenClinico { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string Tratamiento { get; set; } = string.Empty;
    public string? Indicaciones { get; set; }
    public DateTime? FechaControl { get; set; }

    public Paciente? Paciente { get; set; }
    public Veterinario? Veterinario { get; set; }
}
