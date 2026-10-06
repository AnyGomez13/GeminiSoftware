using VetClinic.Domain.Enums;

namespace VetClinic.Api.Dtos;

public record PacienteDto(
    int Id,
    int PropietarioId,
    string? NombrePropietario,
    string? TelefonoPropietario,
    string? EmailPropietario,
    string Nombre,
    Especie Especie,
    string Raza,
    Sexo Sexo,
    DateTime FechaNacimiento,
    bool EsFechaEstimada,
    string EdadFormateada,
    double PesoActualKg,
    string? ColorSenas,
    EstadoReproductivo EstadoReproductivo,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PuntoCurvaPesoDto(
    DateTime Fecha,
    double PesoKg,
    string TipoEvento, // "Registro Inicial" | "Consulta Médica"
    string? Detalle
);

public record PacienteDetalleDto(
    int Id,
    int PropietarioId,
    PropietarioDto? Propietario,
    string Nombre,
    Especie Especie,
    string Raza,
    Sexo Sexo,
    DateTime FechaNacimiento,
    bool EsFechaEstimada,
    string EdadFormateada,
    double PesoActualKg,
    string? ColorSenas,
    EstadoReproductivo EstadoReproductivo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<AtencionClinicaDto> Atenciones,
    IReadOnlyList<InmunizacionDto> Inmunizaciones,
    IReadOnlyList<PuntoCurvaPesoDto> CurvaPeso
);

public record CrearPacienteDto(
    int PropietarioId,
    string Nombre,
    Especie Especie,
    string Raza,
    Sexo Sexo,
    DateTime FechaNacimiento,
    bool EsFechaEstimada,
    double PesoActualKg,
    string? ColorSenas,
    EstadoReproductivo EstadoReproductivo
);

public record ActualizarPacienteDto(
    int PropietarioId,
    string Nombre,
    Especie Especie,
    string Raza,
    Sexo Sexo,
    DateTime FechaNacimiento,
    bool EsFechaEstimada,
    double PesoActualKg,
    string? ColorSenas,
    EstadoReproductivo EstadoReproductivo
);
