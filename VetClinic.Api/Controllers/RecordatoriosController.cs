using Microsoft.AspNetCore.Mvc;
using VetClinic.Api.Dtos;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Infrastructure.Services;

namespace VetClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordatoriosController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public RecordatoriosController(IClinicaService clinicaService)
    {
        _clinicaService = clinicaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecordatorioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecordatorios(
        [FromQuery] int dias = 30,
        [FromQuery] string? estado = "todos",
        CancellationToken cancellationToken = default)
    {
        if (dias <= 0)
        {
            dias = 30;
        }

        var inmunizaciones = await _clinicaService.ObtenerRecordatoriosAsync(dias, estado, cancellationToken);
        var hoy = DateTime.Today;

        var dtos = new List<RecordatorioDto>();

        foreach (var inm in inmunizaciones)
        {
            var paciente = inm.Paciente;
            var tutor = paciente?.Propietario;

            var nombrePaciente = paciente?.Nombre ?? "Mascota";
            var especiePaciente = paciente?.Especie.ToString() ?? "Canino";
            var nombreTutor = tutor?.GetNombreCompleto() ?? "Tutor";
            var telefonoTutor = tutor?.Telefono ?? string.Empty;
            var emailTutor = tutor?.Email;

            var diasDiferencia = (inm.FechaRefuerzo.Date - hoy).Days;
            var esVencido = diasDiferencia < 0;
            var estadoStr = esVencido ? "Vencido" : "Proximo";

            string textoFecha = esVencido
                ? $"venció el {inm.FechaRefuerzo:dd/MM/yyyy} (hace {Math.Abs(diasDiferencia)} días)"
                : (diasDiferencia == 0 ? "vence hoy" : $"vence el {inm.FechaRefuerzo:dd/MM/yyyy} (en {diasDiferencia} días)");

            string mensajeSugerido = $"Hola {nombreTutor}, de parte de la Clínica Veterinaria (Dres. Fabio y William), te recordamos que el refuerzo de {inm.NombreProducto} para tu consentido {nombrePaciente} {textoFecha}. ¡Cuidemos su salud!";

            string whatsappUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(telefonoTutor))
            {
                try
                {
                    whatsappUrl = ExternalLauncherService.ConstruirUriWhatsApp(telefonoTutor, mensajeSugerido);
                }
                catch
                {
                    whatsappUrl = string.Empty;
                }
            }

            string? mailtoUrl = null;
            if (!string.IsNullOrWhiteSpace(emailTutor))
            {
                try
                {
                    mailtoUrl = ExternalLauncherService.ConstruirUriCorreo(
                        emailTutor,
                        $"Recordatorio de Refuerzo: {inm.NombreProducto} - {nombrePaciente}",
                        mensajeSugerido
                    );
                }
                catch
                {
                    mailtoUrl = null;
                }
            }

            dtos.Add(new RecordatorioDto(
                inm.Id,
                inm.PacienteId,
                nombrePaciente,
                especiePaciente,
                tutor?.Id ?? 0,
                nombreTutor,
                telefonoTutor,
                emailTutor,
                inm.TipoBiologico,
                inm.NombreProducto,
                inm.FechaAplicacion,
                inm.FechaRefuerzo,
                estadoStr,
                diasDiferencia,
                mensajeSugerido,
                whatsappUrl,
                mailtoUrl
            ));
        }

        return Ok(dtos);
    }
}
