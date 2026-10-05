using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Repositories;

public class PropietarioRepository : Repository<Propietario>, IPropietarioRepository
{
    public PropietarioRepository(VetClinicDbContext context) : base(context)
    {
    }

    public async Task<Propietario?> GetByDocumentoAsync(string numeroDocumento, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Pacientes)
            .FirstOrDefaultAsync(p => p.NumeroDocumento == numeroDocumento, cancellationToken);
    }

    public async Task<IReadOnlyList<Propietario>> BuscarAsync(string criterio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(criterio))
        {
            return await _dbSet
                .Include(p => p.Pacientes)
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToListAsync(cancellationToken);
        }

        var termino = criterio.Trim().ToLower();

        return await _dbSet
            .Include(p => p.Pacientes)
            .Where(p => p.NumeroDocumento.ToLower().Contains(termino)
                     || p.Nombres.ToLower().Contains(termino)
                     || p.Apellidos.ToLower().Contains(termino)
                     || p.Telefono.Contains(termino))
            .OrderBy(p => p.Apellidos)
            .ThenBy(p => p.Nombres)
            .ToListAsync(cancellationToken);
    }

    public async Task<Propietario?> GetConPacientesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Pacientes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
