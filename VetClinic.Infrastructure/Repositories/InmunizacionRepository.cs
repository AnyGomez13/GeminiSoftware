using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Repositories;

public class InmunizacionRepository : Repository<Inmunizacion>, IInmunizacionRepository
{
    public InmunizacionRepository(VetClinicDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Inmunizacion>> GetPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(i => i.Veterinario)
            .Where(i => i.PacienteId == pacienteId)
            .OrderByDescending(i => i.FechaAplicacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Inmunizacion>> GetProximosRefuerzosAsync(int dias, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.Today;
        var limite = hoy.AddDays(dias);

        return await _dbSet
            .AsNoTracking()
            .Include(i => i.Veterinario)
            .Include(i => i.Paciente)
                .ThenInclude(p => p!.Propietario)
            .Where(i => i.FechaRefuerzo.Date >= hoy && i.FechaRefuerzo.Date <= limite)
            .OrderBy(i => i.FechaRefuerzo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Inmunizacion>> GetRecordatoriosAsync(int dias, string? estado = "todos", CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.Today;
        var limite = hoy.AddDays(dias);

        var query = _dbSet
            .AsNoTracking()
            .Include(i => i.Veterinario)
            .Include(i => i.Paciente)
                .ThenInclude(p => p!.Propietario)
            .AsQueryable();

        var normalizedEstado = estado?.ToLower().Trim();

        if (normalizedEstado == "vencidos")
        {
            query = query.Where(i => i.FechaRefuerzo.Date < hoy);
        }
        else if (normalizedEstado == "proximos")
        {
            query = query.Where(i => i.FechaRefuerzo.Date >= hoy && i.FechaRefuerzo.Date <= limite);
        }
        else
        {
            query = query.Where(i => i.FechaRefuerzo.Date <= limite);
        }

        return await query
            .OrderBy(i => i.FechaRefuerzo)
            .ToListAsync(cancellationToken);
    }
}
