using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// A repository that only records changes: Add, Update and Delete never save, so a command saves everything once with
/// <see cref="RepositoryBase{T}.SaveChangesAsync(CancellationToken)"/> and its writes commit together or not at all
/// (database rules 3.1). Ardalis's own <see cref="RepositoryBase{T}"/> saves inside each of these calls.
/// </summary>
public abstract class UnitOfWorkRepositoryBase<T>(DbContext dbContext) : RepositoryBase<T>(dbContext)
    where T : class
{
    private readonly DbContext _dbContext = dbContext;

    public override Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().Add(entity);
        return Task.FromResult(entity);
    }

    public override Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        var added = entities as IReadOnlyCollection<T> ?? entities.ToArray();
        _dbContext.Set<T>().AddRange(added);
        return Task.FromResult<IEnumerable<T>>(added);
    }

    public override Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().Update(entity);
        return Task.FromResult(0);
    }

    public override Task<int> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().UpdateRange(entities);
        return Task.FromResult(0);
    }

    public override Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.FromResult(0);
    }

    public override Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().RemoveRange(entities);
        return Task.FromResult(0);
    }

    public override async Task<int> DeleteRangeAsync(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().RemoveRange(await ListAsync(specification, cancellationToken));
        return 0;
    }
}
