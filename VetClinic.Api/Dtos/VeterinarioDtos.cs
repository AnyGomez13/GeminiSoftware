namespace VetClinic.Api.Dtos;

public record VeterinarioDto(
    int Id,
    string Nombre,
    string TarjetaProfesional,
    bool IsActive
);
