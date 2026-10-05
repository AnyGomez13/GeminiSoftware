using VetClinic.Domain.Interfaces;
using VetClinic.Infrastructure.Data;

namespace VetClinic.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly VetClinicDbContext _context;

    public UnitOfWork(VetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
