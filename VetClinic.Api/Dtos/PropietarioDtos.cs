using VetClinic.Domain.Enums;

namespace VetClinic.Api.Dtos;

public record PropietarioDto(
    int Id,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string Telefono,
    string? Email,
    string? Direccion,
    int CantidadPacientes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CrearPropietarioDto(
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string Telefono,
    string? Email,
    string? Direccion
);

public record ActualizarPropietarioDto(
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string Telefono,
    string? Email,
    string? Direccion
);
