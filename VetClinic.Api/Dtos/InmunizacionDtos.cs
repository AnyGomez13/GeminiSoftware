using VetClinic.Domain.Enums;

namespace VetClinic.Api.Dtos;

public record InmunizacionDto(
    int Id,
    int PacienteId,
    string? NombrePaciente,
    int VeterinarioId,
    string? NombreVeterinario,
    TipoBiologico TipoBiologico,
    string NombreProducto,
    string? LoteFabricante,
    DateTime FechaAplicacion,
    DateTime FechaRefuerzo,
    string EstadoRefuerzo, // "AlDia", "Proximo", "Vencido"
    int DiasRestantes,
    string? Observaciones,
    DateTime CreatedAt
);

public record CrearInmunizacionDto(
    int PacienteId,
    int VeterinarioId,
    TipoBiologico TipoBiologico,
    string NombreProducto,
    string? LoteFabricante,
    DateTime FechaAplicacion,
    DateTime FechaRefuerzo,
    string? Observaciones
);
