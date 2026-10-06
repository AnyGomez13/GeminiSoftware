using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public PropietariosController(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PropietarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPropietarios([FromQuery] string? criterio, CancellationToken cancellationToken)
    {
        var propietarios = await _clinicaService.BuscarPropietariosAsync(criterio ?? string.Empty, cancellationToken);
        var dtos = propietarios.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PropietarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var propietario = await _clinicaService.ObtenerPropietarioPorIdAsync(id, cancellationToken);
        if (propietario == null)
        {
            return NotFound(new { message = $"No se encontró el propietario con ID {id}." });
        }

        return Ok(MapToDto(propietario));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PropietarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPropietario([FromBody] CrearPropietarioDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos del propietario son requeridos." });
        }

        var entidad = new Propietario
        {
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento?.Trim() ?? string.Empty,
            Nombres = dto.Nombres?.Trim() ?? string.Empty,
            Apellidos = dto.Apellidos?.Trim() ?? string.Empty,
            Telefono = dto.Telefono?.Trim() ?? string.Empty,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim()
        };

        var result = await _clinicaService.CrearPropietarioAsync(entidad, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        var responseDto = MapToDto(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PropietarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPropietario(int id, [FromBody] ActualizarPropietarioDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos del propietario son requeridos." });
        }

        var existente = await _clinicaService.ObtenerPropietarioPorIdAsync(id, cancellationToken);
        if (existente == null)
        {
            return NotFound(new { message = $"No se encontró el propietario con ID {id}." });
        }

        existente.TipoDocumento = dto.TipoDocumento;
        existente.NumeroDocumento = dto.NumeroDocumento?.Trim() ?? string.Empty;
        existente.Nombres = dto.Nombres?.Trim() ?? string.Empty;
        existente.Apellidos = dto.Apellidos?.Trim() ?? string.Empty;
        existente.Telefono = dto.Telefono?.Trim() ?? string.Empty;
        existente.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        existente.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();

        var result = await _clinicaService.ActualizarPropietarioAsync(existente, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        return Ok(MapToDto(result.Value));
    }

    private static PropietarioDto MapToDto(Propietario p)
    {
        return new PropietarioDto(
            p.Id,
            p.TipoDocumento,
            p.NumeroDocumento,
            p.Nombres,
            p.Apellidos,
            p.GetNombreCompleto(),
            p.Telefono,
            p.Email,
            p.Direccion,
            p.Pacientes?.Count ?? 0,
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
