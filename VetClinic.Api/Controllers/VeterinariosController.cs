using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Interfaces.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeterinariosController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public VeterinariosController(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VeterinarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVeterinariosActivos(CancellationToken cancellationToken)
    {
        var vets = await _clinicaService.ObtenerVeterinariosActivosAsync(cancellationToken);
        var dtos = vets.Select(v => new VeterinarioDto(
            v.Id,
            v.Nombre,
            v.TarjetaProfesional,
            v.IsActive
        )).ToList();

        return Ok(dtos);
    }
}
