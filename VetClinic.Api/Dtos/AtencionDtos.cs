namespace VetClinic.Api.Dtos;

public record AtencionClinicaDto(
    int Id,
    int PacienteId,
    string? NombrePaciente,
    int VeterinarioId,
    string? NombreVeterinario,
    DateTime FechaHoraAtencion,
    double PesoConsultaKg,
    string MotivoConsulta,
    string? ExamenClinico,
    string Diagnostico,
    string Tratamiento,
    string? Indicaciones,
    DateTime? FechaControl,
    DateTime CreatedAt
);

public record CrearAtencionDto(
    int PacienteId,
    int VeterinarioId,
    DateTime? FechaHoraAtencion,
    double PesoConsultaKg,
    string MotivoConsulta,
    string? ExamenClinico,
    string Diagnostico,
    string Tratamiento,
    string? Indicaciones,
    DateTime? FechaControl
);
