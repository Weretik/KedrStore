namespace Infrastructure.Catalog.Persistence;

public class CatalogUnitOfWork : ICatalogUnitOfWork
{
    private readonly ICatalogDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;

    private IDbContextTransaction? _currentTransaction;

    public ICatalogDbContext DbContext => _dbContext;
    public IDomainEventDispatcher EventDispatcher => _eventDispatcher;

    public CatalogUnitOfWork(
        ICatalogDbContext dbContext,
        IDomainEventDispatcher eventDispatcher)
    {
        _dbContext = dbContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(cancellationToken);

        return result;
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await _dbContext.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
            await _eventDispatcher.DispatchAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
            return;

        await _currentTransaction.RollbackAsync();
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync();

        try
        {
            await action();
            await CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync();

        try
        {
            var result = await action();
            await CommitTransactionAsync();
            return result;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }
}
/*
 TODO:
Можно добавить логгирование транзакций — полезно в распределённых сценариях.
Можно обернуть DispatchAsync в try/catch — если событие не критично, но это зависит от политики твоего проекта.
Журналировать/метрики по ExecuteInTransactionAsync — для будущего мониторинга.
*/
