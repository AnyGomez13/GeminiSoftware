using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Repositories;

public interface IInmunizacionRepository : IRepository<Inmunizacion>
{
    Task<IReadOnlyList<Inmunizacion>> GetPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inmunizacion>> GetProximosRefuerzosAsync(int dias, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inmunizacion>> GetRecordatoriosAsync(int dias, string? estado = "todos", CancellationToken cancellationToken = default);
}
