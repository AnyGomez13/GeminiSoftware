using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Repositories;

public class PacienteRepository : Repository<Paciente>, IPacienteRepository
{
    public PacienteRepository(VetClinicDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Paciente>> GetByPropietarioIdAsync(int propietarioId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.PropietarioId == propietarioId)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Paciente>> BuscarAsync(string criterio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(criterio))
        {
            return await _dbSet
                .Include(p => p.Propietario)
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        var termino = criterio.Trim().ToLower();

        return await _dbSet
            .Include(p => p.Propietario)
            .Where(p => p.Nombre.ToLower().Contains(termino)
                     || (p.Propietario != null && (
                         p.Propietario.Nombres.ToLower().Contains(termino)
                         || p.Propietario.Apellidos.ToLower().Contains(termino)
                         || p.Propietario.NumeroDocumento.Contains(termino)
                         || p.Propietario.Telefono.Contains(termino))))
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Paciente?> GetDetalleCompletoAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Propietario)
            .Include(p => p.AtencionesClinicas.OrderByDescending(a => a.FechaHoraAtencion))
                .ThenInclude(a => a.Veterinario)
            .Include(p => p.Inmunizaciones.OrderByDescending(i => i.FechaAplicacion))
                .ThenInclude(i => i.Veterinario)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
