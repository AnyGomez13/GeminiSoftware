using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponseDto(false, "El nombre de usuario y la contraseña son obligatorios.", null));
        }

        var result = await _authService.AuthenticateAsync(request.Username.Trim(), request.Password, cancellationToken);
        if (!result.IsSuccess)
        {
            return Unauthorized(new LoginResponseDto(false, result.Error.Message, null));
        }

        var user = result.Value;
        var usuarioDto = new UsuarioDto(
            user.Id,
            user.Username,
            user.NombreCompleto,
            user.Rol.ToString(),
            user.IsActive
        );

        // En estación monopuesto local, se retorna un identificador de sesión local único
        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Username}:{Guid.NewGuid():N}"));

        return Ok(new LoginResponseDto(true, "Inicio de sesión exitoso.", usuarioDto, token));
    }
}
