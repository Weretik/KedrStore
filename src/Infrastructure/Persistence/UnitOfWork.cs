namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;
    private readonly DbContext _context;

    public IApplicationDbContext DbContext { get; }
    public IDomainEventDispatcher EventDispatcher { get; }

    public UnitOfWork(
        IApplicationDbContext dbContext,
        IDomainEventDispatcher eventDispatcher)
    {
        DbContext = dbContext;
        EventDispatcher = eventDispatcher;
        _context = (DbContext)dbContext;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Получаем все сущности, которые имеют доменные события
        var entitiesWithEvents = _context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        // Сохраняем изменения в БД
        var result = await DbContext.SaveChangesAsync(cancellationToken);

        // После успешного сохранения публикуем все доменные события
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            foreach (var domainEvent in events)
            {
                await EventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }

        return result;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync();
        try
        {
            await operation();
            await CommitTransactionAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        await BeginTransactionAsync();
        try
        {
            var result = await operation();
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
/*
 TODO:
Можно добавить логгирование транзакций — полезно в распределённых сценариях.
Можно обернуть DispatchAsync в try/catch — если событие не критично, но это зависит от политики твоего проекта.
Журналировать/метрики по ExecuteInTransactionAsync — для будущего мониторинга.
*/
