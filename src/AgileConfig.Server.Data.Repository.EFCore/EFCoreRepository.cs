using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.EFCore;

namespace AgileConfig.Server.Data.Repository.EFCore;

public abstract class EFCoreRepository<T, TId> : IRepository<T, TId> where T : class, IEntity<TId>
{
    protected readonly AgileConfigDbContext _context;
    protected readonly DbSet<T> _dbSet;
    private IUow? _uow;

    public EFCoreRepository(AgileConfigDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IUow Uow
    {
        get => _uow!;
        set => _uow = value;
    }

    public async Task<List<T>> AllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task DeleteAsync(TId id)
    {
        var entity = await GetAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(IList<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<T?> GetAsync(TId id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> InsertAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task InsertAsync(IList<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<List<T>> QueryAsync(Expression<Func<T, bool>> exp)
    {
        return await _dbSet.Where(exp).ToListAsync();
    }

    public async Task<List<T>> QueryPageAsync(Expression<Func<T, bool>> exp, int pageIndex, int pageSize,
        string defaultSortField = "Id", string defaultSortType = "ASC")
    {
        var query = _dbSet.Where(exp);

        // Apply ordering
        var isAscending = defaultSortType.Equals("ASC", StringComparison.OrdinalIgnoreCase);
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, defaultSortField);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = isAscending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        query = query.Provider.CreateQuery<T>(resultExpression);

        // Apply paging
        return await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>>? exp = null)
    {
        return exp == null
            ? await _dbSet.LongCountAsync()
            : await _dbSet.Where(exp).LongCountAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(IList<T> entities)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        // Context is managed by DI container, so we don't dispose it here
    }
}
