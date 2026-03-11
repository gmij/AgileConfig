using Microsoft.EntityFrameworkCore;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public class EFCoreUow : IUow
{
    private readonly AgileConfigDbContext _context;

    public EFCoreUow(AgileConfigDbContext context)
    {
        _context = context;
    }

    public void Begin()
    {
        // EF Core doesn't require explicit transaction start for simple operations
        // Transactions are automatically managed by SaveChanges
    }

    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Rollback()
    {
        // Discard all changes tracked by the context
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
