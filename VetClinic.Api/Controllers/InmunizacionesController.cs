using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InmunizacionesController : ControllerBase
{
    private readonly IClinicaService _clinicaService;
    private readonly IPdfExportService _pdfExportService;

    public InmunizacionesController(IClinicaService clinicaService, IPdfExportService pdfExportService)
    {
        _clinicaService = clinicaService;
        _pdfExportService = pdfExportService;
    }

    [HttpGet("paciente/{pacienteId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<InmunizacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorPaciente(int pacienteId, CancellationToken cancellationToken)
    {
        var lista = await _clinicaService.ObtenerInmunizacionesPacienteAsync(pacienteId, cancellationToken);
        var hoy = DateTime.Today;

        var dtos = lista.Select(i =>
        {
            var dias = (i.FechaRefuerzo.Date - hoy).Days;
            string estado = dias < 0 ? "Vencido" : (dias <= 30 ? "Proximo" : "AlDia");

            return new InmunizacionDto(
                i.Id,
                i.PacienteId,
                i.Paciente?.Nombre,
                i.VeterinarioId,
                i.Veterinario?.Nombre ?? "Veterinario Tratante",
                i.TipoBiologico,
                i.NombreProducto,
                i.LoteFabricante,
                i.FechaAplicacion,
                i.FechaRefuerzo,
                estado,
                dias,
                i.Observaciones,
                i.CreatedAt
            );
        }).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InmunizacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarInmunizacion([FromBody] CrearInmunizacionDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos de la inmunización son requeridos." });
        }

        var entidad = new Inmunizacion
        {
            PacienteId = dto.PacienteId,
            VeterinarioId = dto.VeterinarioId,
            TipoBiologico = dto.TipoBiologico,
            NombreProducto = dto.NombreProducto?.Trim() ?? string.Empty,
            LoteFabricante = string.IsNullOrWhiteSpace(dto.LoteFabricante) ? null : dto.LoteFabricante.Trim(),
            FechaAplicacion = dto.FechaAplicacion,
            FechaRefuerzo = dto.FechaRefuerzo,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim()
        };

        var result = await _clinicaService.RegistrarInmunizacionAsync(entidad, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        var creada = result.Value;
        var hoy = DateTime.Today;
        var dias = (creada.FechaRefuerzo.Date - hoy).Days;
        string estado = dias < 0 ? "Vencido" : (dias <= 30 ? "Proximo" : "AlDia");

        var responseDto = new InmunizacionDto(
            creada.Id,
            creada.PacienteId,
            creada.Paciente?.Nombre,
            creada.VeterinarioId,
            creada.Veterinario?.Nombre ?? "Veterinario Tratante",
            creada.TipoBiologico,
            creada.NombreProducto,
            creada.LoteFabricante,
            creada.FechaAplicacion,
            creada.FechaRefuerzo,
            estado,
            dias,
            creada.Observaciones,
            creada.CreatedAt
        );

        return CreatedAtAction(nameof(GetPorPaciente), new { pacienteId = creada.PacienteId }, responseDto);
    }

    [HttpGet("carnet-pdf/{pacienteId:int}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarCarnetPdf(int pacienteId, CancellationToken cancellationToken)
    {
        var result = await _pdfExportService.GenerarCarnetVacunacionBytesAsync(pacienteId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        return File(result.Value, "application/pdf", $"Carnet_Vacunacion_{pacienteId}.pdf");
    }
}
