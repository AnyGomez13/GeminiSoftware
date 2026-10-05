using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Repositories;

public interface IAtencionClinicaRepository : IRepository<AtencionClinica>
{
    Task<IReadOnlyList<AtencionClinica>> GetHistorialPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
}
