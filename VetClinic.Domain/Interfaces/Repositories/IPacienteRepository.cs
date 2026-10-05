using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Repositories;

public interface IPacienteRepository : IRepository<Paciente>
{
    Task<IReadOnlyList<Paciente>> GetByPropietarioIdAsync(int propietarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Paciente>> BuscarAsync(string criterio, CancellationToken cancellationToken = default);
    Task<Paciente?> GetDetalleCompletoAsync(int id, CancellationToken cancellationToken = default);
}
