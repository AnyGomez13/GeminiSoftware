using VetClinic.Domain.Entities;

namespace VetClinic.Domain.Interfaces.Repositories;

public interface IPropietarioRepository : IRepository<Propietario>
{
    Task<Propietario?> GetByDocumentoAsync(string numeroDocumento, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Propietario>> BuscarAsync(string criterio, CancellationToken cancellationToken = default);
    Task<Propietario?> GetConPacientesAsync(int id, CancellationToken cancellationToken = default);
}
