using VetClinic.Domain.Common;
using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Services;

public interface IClinicaService
{
    // Propietarios
    Task<Result<Propietario>> CrearPropietarioAsync(Propietario propietario, CancellationToken cancellationToken = default);
    Task<Result<Propietario>> ActualizarPropietarioAsync(Propietario propietario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Propietario>> BuscarPropietariosAsync(string criterio, CancellationToken cancellationToken = default);
    Task<Propietario?> ObtenerPropietarioPorIdAsync(int id, CancellationToken cancellationToken = default);

    // Pacientes
    Task<Result<Paciente>> RegistrarPacienteAsync(Paciente paciente, CancellationToken cancellationToken = default);
    Task<Result<Paciente>> ActualizarPacienteAsync(Paciente paciente, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Paciente>> BuscarPacientesAsync(string criterio, CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerPacientePorIdAsync(int id, CancellationToken cancellationToken = default);

    // Atenciones Médicas
    Task<Result<AtencionClinica>> RegistrarAtencionAsync(AtencionClinica atencion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AtencionClinica>> ObtenerHistorialPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);

    // Inmunizaciones
    Task<Result<Inmunizacion>> RegistrarInmunizacionAsync(Inmunizacion inmunizacion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inmunizacion>> ObtenerInmunizacionesPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inmunizacion>> ObtenerProximosRefuerzosAsync(int dias, CancellationToken cancellationToken = default);

    // Catálogo de Veterinarios
    Task<IReadOnlyList<Veterinario>> ObtenerVeterinariosActivosAsync(CancellationToken cancellationToken = default);
}
