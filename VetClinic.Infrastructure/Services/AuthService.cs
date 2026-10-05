using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Common;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly VetClinicDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(VetClinicDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<Usuario>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure<Usuario>("Debe ingresar usuario y contraseña.");
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure<Usuario>("Usuario o contraseña incorrectos.");
        }

        var passwordValido = _passwordHasher.VerifyPassword(password, usuario.PasswordHash, usuario.PasswordSalt);
        if (!passwordValido)
        {
            return Result.Failure<Usuario>("Usuario o contraseña incorrectos.");
        }

        return Result.Success(usuario);
    }
}
