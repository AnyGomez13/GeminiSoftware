using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Common;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Domain.Interfaces.Services;
using VetClinic.Domain.ValueObjects;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Services;

public class ClinicaService : IClinicaService
{
    private readonly IPropietarioRepository _propietarioRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IAtencionClinicaRepository _atencionRepository;
    private readonly IInmunizacionRepository _inmunizacionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VetClinicDbContext _context;

    public ClinicaService(
        IPropietarioRepository propietarioRepository,
        IPacienteRepository pacienteRepository,
        IAtencionClinicaRepository atencionRepository,
        IInmunizacionRepository inmunizacionRepository,
        IUnitOfWork unitOfWork,
        VetClinicDbContext context)
    {
        _propietarioRepository = propietarioRepository;
        _pacienteRepository = pacienteRepository;
        _atencionRepository = atencionRepository;
        _inmunizacionRepository = inmunizacionRepository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    // ==================== PROPIETARIOS ====================

    public async Task<Result<Propietario>> CrearPropietarioAsync(Propietario propietario, CancellationToken cancellationToken = default)
    {
        if (propietario == null)
        {
            return Result.Failure<Propietario>("El propietario no puede ser nulo.");
        }

        if (string.IsNullOrWhiteSpace(propietario.NumeroDocumento))
        {
            return Result.Failure<Propietario>("El número de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(propietario.Nombres) || string.IsNullOrWhiteSpace(propietario.Apellidos))
        {
            return Result.Failure<Propietario>("Los nombres y apellidos son obligatorios.");
        }

        var celularResult = NumeroCelular.Crear(propietario.Telefono);
        if (!celularResult.IsSuccess)
        {
            return Result.Failure<Propietario>(celularResult.Error.Message);
        }

        propietario.Telefono = celularResult.Value.Valor;

        var existente = await _propietarioRepository.GetByDocumentoAsync(propietario.NumeroDocumento.Trim(), cancellationToken);
        if (existente != null)
        {
            return Result.Failure<Propietario>("Ya existe un propietario registrado con el documento especificado.");
        }

        propietario.CreatedAt = DateTime.Now;
        propietario.UpdatedAt = DateTime.Now;

        await _propietarioRepository.AddAsync(propietario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(propietario);
    }

    public async Task<Result<Propietario>> ActualizarPropietarioAsync(Propietario propietario, CancellationToken cancellationToken = default)
    {
        if (propietario == null)
        {
            return Result.Failure<Propietario>("El propietario no puede ser nulo.");
        }

        if (string.IsNullOrWhiteSpace(propietario.NumeroDocumento))
        {
            return Result.Failure<Propietario>("El número de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(propietario.Nombres) || string.IsNullOrWhiteSpace(propietario.Apellidos))
        {
            return Result.Failure<Propietario>("Los nombres y apellidos son obligatorios.");
        }

        var celularResult = NumeroCelular.Crear(propietario.Telefono);
        if (!celularResult.IsSuccess)
        {
            return Result.Failure<Propietario>(celularResult.Error.Message);
        }

        propietario.Telefono = celularResult.Value.Valor;

        var existente = await _propietarioRepository.GetByDocumentoAsync(propietario.NumeroDocumento.Trim(), cancellationToken);
        if (existente != null && existente.Id != propietario.Id)
        {
            return Result.Failure<Propietario>("Ya existe otro propietario registrado con el documento especificado.");
        }

        propietario.UpdatedAt = DateTime.Now;

        _propietarioRepository.Update(propietario);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(propietario);
    }

    public async Task<IReadOnlyList<Propietario>> BuscarPropietariosAsync(string criterio, CancellationToken cancellationToken = default)
    {
        return await _propietarioRepository.BuscarAsync(criterio, cancellationToken);
    }

    public async Task<Propietario?> ObtenerPropietarioPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _propietarioRepository.GetConPacientesAsync(id, cancellationToken);
    }

    // ==================== PACIENTES ====================

    public async Task<Result<Paciente>> RegistrarPacienteAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        if (paciente == null)
        {
            return Result.Failure<Paciente>("El paciente no puede ser nulo.");
        }

        if (paciente.PropietarioId <= 0 && paciente.Propietario == null)
        {
            return Result.Failure<Paciente>("El paciente debe estar asociado obligatoriamente a un propietario.");
        }

        if (string.IsNullOrWhiteSpace(paciente.Nombre))
        {
            return Result.Failure<Paciente>("El nombre del paciente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(paciente.Raza))
        {
            return Result.Failure<Paciente>("La raza del paciente es obligatoria.");
        }

        if (paciente.FechaNacimiento.Date > DateTime.Today)
        {
            return Result.Failure<Paciente>("La fecha de nacimiento no puede ser posterior a la fecha actual.");
        }

        var pesoResult = PesoCorporal.Crear(paciente.PesoActualKg);
        if (!pesoResult.IsSuccess)
        {
            return Result.Failure<Paciente>(pesoResult.Error.Message);
        }

        paciente.PesoActualKg = pesoResult.Value.Valor;
        paciente.CreatedAt = DateTime.Now;
        paciente.UpdatedAt = DateTime.Now;

        await _pacienteRepository.AddAsync(paciente, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(paciente);
    }

    public async Task<Result<Paciente>> ActualizarPacienteAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        if (paciente == null)
        {
            return Result.Failure<Paciente>("El paciente no puede ser nulo.");
        }

        if (paciente.PropietarioId <= 0)
        {
            return Result.Failure<Paciente>("El paciente debe estar asociado obligatoriamente a un propietario.");
        }

        if (string.IsNullOrWhiteSpace(paciente.Nombre))
        {
            return Result.Failure<Paciente>("El nombre del paciente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(paciente.Raza))
        {
            return Result.Failure<Paciente>("La raza del paciente es obligatoria.");
        }

        if (paciente.FechaNacimiento.Date > DateTime.Today)
        {
            return Result.Failure<Paciente>("La fecha de nacimiento no puede ser posterior a la fecha actual.");
        }

        var pesoResult = PesoCorporal.Crear(paciente.PesoActualKg);
        if (!pesoResult.IsSuccess)
        {
            return Result.Failure<Paciente>(pesoResult.Error.Message);
        }

        paciente.PesoActualKg = pesoResult.Value.Valor;
        paciente.UpdatedAt = DateTime.Now;

        _pacienteRepository.Update(paciente);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(paciente);
    }

    public async Task<IReadOnlyList<Paciente>> BuscarPacientesAsync(string criterio, CancellationToken cancellationToken = default)
    {
        return await _pacienteRepository.BuscarAsync(criterio, cancellationToken);
    }

    public async Task<Paciente?> ObtenerPacientePorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _pacienteRepository.GetDetalleCompletoAsync(id, cancellationToken);
    }

    // ==================== ATENCIONES CLÍNICAS ====================

    public async Task<Result<AtencionClinica>> RegistrarAtencionAsync(AtencionClinica atencion, CancellationToken cancellationToken = default)
    {
        if (atencion == null)
        {
            return Result.Failure<AtencionClinica>("La atención médica no puede ser nula.");
        }

        if (atencion.PacienteId <= 0)
        {
            return Result.Failure<AtencionClinica>("Debe especificar un paciente válido.");
        }

        if (atencion.VeterinarioId <= 0)
        {
            return Result.Failure<AtencionClinica>("Debe asignar obligatoriamente un veterinario tratante (Dr. Fabio o Dr. William).");
        }

        var vet = await _context.Veterinarios.FirstOrDefaultAsync(v => v.Id == atencion.VeterinarioId && v.IsActive, cancellationToken);
        if (vet == null)
        {
            return Result.Failure<AtencionClinica>("El veterinario tratante seleccionado no es válido o no está activo.");
        }

        if (string.IsNullOrWhiteSpace(atencion.MotivoConsulta))
        {
            return Result.Failure<AtencionClinica>("El motivo de consulta es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(atencion.Diagnostico))
        {
            return Result.Failure<AtencionClinica>("El diagnóstico clínico es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(atencion.Tratamiento))
        {
            return Result.Failure<AtencionClinica>("El plan de tratamiento es obligatorio.");
        }

        var pesoResult = PesoCorporal.Crear(atencion.PesoConsultaKg);
        if (!pesoResult.IsSuccess)
        {
            return Result.Failure<AtencionClinica>(pesoResult.Error.Message);
        }

        var paciente = await _pacienteRepository.GetByIdAsync(atencion.PacienteId, cancellationToken);
        if (paciente == null)
        {
            return Result.Failure<AtencionClinica>("El paciente especificado no existe.");
        }

        atencion.PesoConsultaKg = pesoResult.Value.Valor;
        atencion.CreatedAt = DateTime.Now;

        // Sincronización automática de peso del paciente al registrar atención médica (RN-05, RN-06)
        paciente.PesoActualKg = atencion.PesoConsultaKg;
        paciente.UpdatedAt = DateTime.Now;
        _pacienteRepository.Update(paciente);

        await _atencionRepository.AddAsync(atencion, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(atencion);
    }

    public async Task<IReadOnlyList<AtencionClinica>> ObtenerHistorialPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        return await _atencionRepository.GetHistorialPorPacienteAsync(pacienteId, cancellationToken);
    }

    // ==================== INMUNIZACIONES ====================

    public async Task<Result<Inmunizacion>> RegistrarInmunizacionAsync(Inmunizacion inmunizacion, CancellationToken cancellationToken = default)
    {
        if (inmunizacion == null)
        {
            return Result.Failure<Inmunizacion>("La inmunización no puede ser nula.");
        }

        if (inmunizacion.PacienteId <= 0)
        {
            return Result.Failure<Inmunizacion>("Debe especificar un paciente válido.");
        }

        if (inmunizacion.VeterinarioId <= 0)
        {
            return Result.Failure<Inmunizacion>("Debe asignar obligatoriamente un veterinario tratante (Dr. Fabio o Dr. William).");
        }

        var vet = await _context.Veterinarios.FirstOrDefaultAsync(v => v.Id == inmunizacion.VeterinarioId && v.IsActive, cancellationToken);
        if (vet == null)
        {
            return Result.Failure<Inmunizacion>("El veterinario tratante seleccionado no es válido o no está activo.");
        }

        if (string.IsNullOrWhiteSpace(inmunizacion.NombreProducto))
        {
            return Result.Failure<Inmunizacion>("El nombre del producto biológico es obligatorio.");
        }

        if (inmunizacion.FechaRefuerzo.Date <= inmunizacion.FechaAplicacion.Date)
        {
            return Result.Failure<Inmunizacion>("La fecha del próximo refuerzo debe ser posterior a la fecha de aplicación.");
        }

        inmunizacion.CreatedAt = DateTime.Now;

        await _inmunizacionRepository.AddAsync(inmunizacion, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(inmunizacion);
    }

    public async Task<IReadOnlyList<Inmunizacion>> ObtenerInmunizacionesPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        return await _inmunizacionRepository.GetPorPacienteAsync(pacienteId, cancellationToken);
    }

    public async Task<IReadOnlyList<Inmunizacion>> ObtenerProximosRefuerzosAsync(int dias, CancellationToken cancellationToken = default)
    {
        return await _inmunizacionRepository.GetProximosRefuerzosAsync(dias, cancellationToken);
    }

    public async Task<IReadOnlyList<Inmunizacion>> ObtenerRecordatoriosAsync(int dias, string? estado = "todos", CancellationToken cancellationToken = default)
    {
        return await _inmunizacionRepository.GetRecordatoriosAsync(dias, estado, cancellationToken);
    }

    // ==================== VETERINARIOS ====================

    public async Task<IReadOnlyList<Veterinario>> ObtenerVeterinariosActivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Veterinarios
            .AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.Nombre)
            .ToListAsync(cancellationToken);
    }
}
