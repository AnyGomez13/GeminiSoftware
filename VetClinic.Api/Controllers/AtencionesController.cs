using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtencionesController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public AtencionesController(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
    }

    [HttpGet("paciente/{pacienteId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<AtencionClinicaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorialPorPaciente(int pacienteId, CancellationToken cancellationToken)
    {
        var historial = await _clinicaService.ObtenerHistorialPacienteAsync(pacienteId, cancellationToken);
        var dtos = historial.Select(a => new AtencionClinicaDto(
            a.Id,
            a.PacienteId,
            a.Paciente?.Nombre,
            a.VeterinarioId,
            a.Veterinario?.Nombre ?? "Veterinario Tratante",
            a.FechaHoraAtencion,
            a.PesoConsultaKg,
            a.MotivoConsulta,
            a.ExamenClinico,
            a.Diagnostico,
            a.Tratamiento,
            a.Indicaciones,
            a.FechaControl,
            a.CreatedAt
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AtencionClinicaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarAtencion([FromBody] CrearAtencionDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos de la atención clínica son requeridos." });
        }

        var entidad = new AtencionClinica
        {
            PacienteId = dto.PacienteId,
            VeterinarioId = dto.VeterinarioId,
            FechaHoraAtencion = dto.FechaHoraAtencion ?? DateTime.Now,
            PesoConsultaKg = dto.PesoConsultaKg,
            MotivoConsulta = dto.MotivoConsulta?.Trim() ?? string.Empty,
            ExamenClinico = string.IsNullOrWhiteSpace(dto.ExamenClinico) ? null : dto.ExamenClinico.Trim(),
            Diagnostico = dto.Diagnostico?.Trim() ?? string.Empty,
            Tratamiento = dto.Tratamiento?.Trim() ?? string.Empty,
            Indicaciones = string.IsNullOrWhiteSpace(dto.Indicaciones) ? null : dto.Indicaciones.Trim(),
            FechaControl = dto.FechaControl
        };

        var result = await _clinicaService.RegistrarAtencionAsync(entidad, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        var creada = result.Value;
        var responseDto = new AtencionClinicaDto(
            creada.Id,
            creada.PacienteId,
            creada.Paciente?.Nombre,
            creada.VeterinarioId,
            creada.Veterinario?.Nombre ?? "Veterinario Tratante",
            creada.FechaHoraAtencion,
            creada.PesoConsultaKg,
            creada.MotivoConsulta,
            creada.ExamenClinico,
            creada.Diagnostico,
            creada.Tratamiento,
            creada.Indicaciones,
            creada.FechaControl,
            creada.CreatedAt
        );

        return CreatedAtAction(nameof(GetHistorialPorPaciente), new { pacienteId = creada.PacienteId }, responseDto);
    }
}
