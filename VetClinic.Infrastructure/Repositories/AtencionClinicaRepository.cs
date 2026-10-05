using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Entities;
using VetClinic.Domain.Interfaces.Repositories;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Repositories;

public class AtencionClinicaRepository : Repository<AtencionClinica>, IAtencionClinicaRepository
{
    public AtencionClinicaRepository(VetClinicDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AtencionClinica>> GetHistorialPorPacienteAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        // Orden estrictamente cronológico descendente y con el veterinario tratante cargado (RN-02, RN-07, RNF-05)
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Veterinario)
            .Where(a => a.PacienteId == pacienteId)
            .OrderByDescending(a => a.FechaHoraAtencion)
            .ToListAsync(cancellationToken);
    }
}
