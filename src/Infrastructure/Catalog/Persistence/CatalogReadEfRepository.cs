namespace Infrastructure.Catalog.Persistence;

internal sealed class CatalogReadEfRepository<T>(IDbContextFactory<CatalogDbContext> factory)
    : ICatalogReadRepository<T>
    where T : class, IAggregateRoot
{
    private async Task<TResult> WithReadRepo<TResult>(
        Func<IRepositoryBase<T>, Task<TResult>> action, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var repo = new CatalogEfRepository<T>(db);
        return await action(repo);
    }

    public Task<List<T>> ListAsync(CancellationToken ct = default)
        => WithReadRepo(r => r.ListAsync(ct), ct);

    public Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.ListAsync(spec, ct), ct);

    public Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.ListAsync(spec, ct), ct);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.FirstOrDefaultAsync(spec, ct), ct);

    public Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.SingleOrDefaultAsync(spec, ct), ct);

    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.SingleOrDefaultAsync(spec, ct), ct);

    public Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.FirstOrDefaultAsync(spec, ct), ct);

    public Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.CountAsync(spec, ct), ct);

    public Task<int> CountAsync(CancellationToken ct = default)
        => WithReadRepo(r => r.CountAsync(ct), ct);

    public Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default)
        => WithReadRepo(r => r.AnyAsync(spec, ct), ct);

    public Task<bool> AnyAsync(CancellationToken ct = default)
        => WithReadRepo(r => r.AnyAsync(ct), ct);

    public async IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> spec)
    {
        await using var db = await factory.CreateDbContextAsync();
        var repo = new CatalogEfRepository<T>(db);

        await foreach (var item in repo.AsAsyncEnumerable(spec))
        {
            yield return item;
        }
    }

    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
        where TId : notnull
        => WithReadRepo(r => r.GetByIdAsync(id, ct), ct);


}
