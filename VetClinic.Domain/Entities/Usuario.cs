using VetClinic.Domain.Common;

namespace VetClinic.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "Veterinario";
    public bool IsActive { get; set; } = true;
}
