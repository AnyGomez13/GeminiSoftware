namespace VetClinic.Api.Dtos;

public record LoginRequestDto(string Username, string Password);

public record UsuarioDto(
    int Id,
    string Username,
    string NombreCompleto,
    string Rol,
    bool IsActive
);

public record LoginResponseDto(
    bool Success,
    string Mensaje,
    UsuarioDto? Usuario,
    string? Token = null
);
