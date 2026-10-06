using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public PacientesController(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PacienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPacientes([FromQuery] string? criterio, CancellationToken cancellationToken)
    {
        var pacientes = await _clinicaService.BuscarPacientesAsync(criterio ?? string.Empty, cancellationToken);
        var dtos = pacientes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PacienteDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var p = await _clinicaService.ObtenerPacientePorIdAsync(id, cancellationToken);
        if (p == null)
        {
            return NotFound(new { message = $"No se encontró el paciente con ID {id}." });
        }

        var propietarioDto = p.Propietario != null
            ? new PropietarioDto(
                p.Propietario.Id,
                p.Propietario.TipoDocumento,
                p.Propietario.NumeroDocumento,
                p.Propietario.Nombres,
                p.Propietario.Apellidos,
                p.Propietario.GetNombreCompleto(),
                p.Propietario.Telefono,
                p.Propietario.Email,
                p.Propietario.Direccion,
                p.Propietario.Pacientes?.Count ?? 0,
                p.Propietario.CreatedAt,
                p.Propietario.UpdatedAt
            )
            : null;

        var atencionesDto = p.AtencionesClinicas
            .OrderByDescending(a => a.FechaHoraAtencion)
            .Select(a => new AtencionClinicaDto(
                a.Id,
                a.PacienteId,
                p.Nombre,
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

        var hoy = DateTime.Today;
        var inmunizacionesDto = p.Inmunizaciones
            .OrderByDescending(i => i.FechaAplicacion)
            .Select(i =>
            {
                var dias = (i.FechaRefuerzo.Date - hoy).Days;
                string estado = dias < 0 ? "Vencido" : (dias <= 30 ? "Proximo" : "AlDia");
                return new InmunizacionDto(
                    i.Id,
                    i.PacienteId,
                    p.Nombre,
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

        // Construir curva histórica de peso ordenada cronológicamente
        var puntosPeso = new List<PuntoCurvaPesoDto>();

        // Punto de registro inicial
        puntosPeso.Add(new PuntoCurvaPesoDto(
            p.CreatedAt,
            p.PesoActualKg,
            "Registro Inicial",
            "Peso registrado al crear ficha"
        ));

        // Puntos de cada consulta médica registrada
        foreach (var atencion in p.AtencionesClinicas.OrderBy(a => a.FechaHoraAtencion))
        {
            puntosPeso.Add(new PuntoCurvaPesoDto(
                atencion.FechaHoraAtencion,
                atencion.PesoConsultaKg,
                "Consulta Médica",
                atencion.MotivoConsulta
            ));
        }

        var curvaOrdenada = puntosPeso
            .OrderBy(pt => pt.Fecha)
            .ToList();

        var detalleDto = new PacienteDetalleDto(
            p.Id,
            p.PropietarioId,
            propietarioDto,
            p.Nombre,
            p.Especie,
            p.Raza,
            p.Sexo,
            p.FechaNacimiento,
            p.EsFechaEstimada,
            p.EdadFormateada,
            p.PesoActualKg,
            p.ColorSenas,
            p.EstadoReproductivo,
            p.CreatedAt,
            p.UpdatedAt,
            atencionesDto,
            inmunizacionesDto,
            curvaOrdenada
        );

        return Ok(detalleDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearPaciente([FromBody] CrearPacienteDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos del paciente son requeridos." });
        }

        var entidad = new Paciente
        {
            PropietarioId = dto.PropietarioId,
            Nombre = dto.Nombre?.Trim() ?? string.Empty,
            Especie = dto.Especie,
            Raza = dto.Raza?.Trim() ?? string.Empty,
            Sexo = dto.Sexo,
            FechaNacimiento = dto.FechaNacimiento,
            EsFechaEstimada = dto.EsFechaEstimada,
            PesoActualKg = dto.PesoActualKg,
            ColorSenas = string.IsNullOrWhiteSpace(dto.ColorSenas) ? null : dto.ColorSenas.Trim(),
            EstadoReproductivo = dto.EstadoReproductivo
        };

        var result = await _clinicaService.RegistrarPacienteAsync(entidad, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        var responseDto = MapToDto(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPaciente(int id, [FromBody] ActualizarPacienteDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Los datos del paciente son requeridos." });
        }

        var existente = await _clinicaService.ObtenerPacientePorIdAsync(id, cancellationToken);
        if (existente == null)
        {
            return NotFound(new { message = $"No se encontró el paciente con ID {id}." });
        }

        existente.PropietarioId = dto.PropietarioId;
        existente.Nombre = dto.Nombre?.Trim() ?? string.Empty;
        existente.Especie = dto.Especie;
        existente.Raza = dto.Raza?.Trim() ?? string.Empty;
        existente.Sexo = dto.Sexo;
        existente.FechaNacimiento = dto.FechaNacimiento;
        existente.EsFechaEstimada = dto.EsFechaEstimada;
        existente.PesoActualKg = dto.PesoActualKg;
        existente.ColorSenas = string.IsNullOrWhiteSpace(dto.ColorSenas) ? null : dto.ColorSenas.Trim();
        existente.EstadoReproductivo = dto.EstadoReproductivo;

        var result = await _clinicaService.ActualizarPacienteAsync(existente, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error.Message });
        }

        return Ok(MapToDto(result.Value));
    }

    private static PacienteDto MapToDto(Paciente p)
    {
        return new PacienteDto(
            p.Id,
            p.PropietarioId,
            p.Propietario?.GetNombreCompleto(),
            p.Propietario?.Telefono,
            p.Propietario?.Email,
            p.Nombre,
            p.Especie,
            p.Raza,
            p.Sexo,
            p.FechaNacimiento,
            p.EsFechaEstimada,
            p.EdadFormateada,
            p.PesoActualKg,
            p.ColorSenas,
            p.EstadoReproductivo,
            p.CreatedAt,
            p.UpdatedAt
        );
    }
}
