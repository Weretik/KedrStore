# ADR 0026: Use IUnitOfWork

## Date
2025-06-17

## Status
Accepted

## Context
The Unit of Work pattern is used to manage transactions and coordinate changes across multiple repositories. It ensures that all operations within a transaction are either committed or rolled back as a single unit, maintaining data consistency. In the Kedr E-Commerce Platform, `IUnitOfWork` provides a clean abstraction for managing transactions and integrating domain events.

## Decision
We decided to use the Unit of Work pattern in the project to:

1. Manage transactions across multiple repositories.
2. Ensure data consistency by committing or rolling back operations as a single unit.
3. Integrate domain events with transaction management.
4. Align with DDD principles by providing a clean abstraction for transaction management.

## Consequences
### Positive
1. Simplifies transaction management across multiple repositories.
2. Ensures data consistency by treating operations as a single unit.
3. Improves maintainability by centralizing transaction logic.
4. Enables integration of domain events with transaction management.

### Negative
1. Adds complexity by introducing an additional abstraction layer.
2. Requires careful implementation to avoid performance bottlenecks.
3. May lead to over-engineering if used for simple scenarios.

## Example
The Unit of Work pattern is implemented as follows:

**IUnitOfWork.cs**:
```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
}
```

**CatalogUnitOfWork.cs**:
```csharp
public class CatalogUnitOfWork : IUnitOfWork
{
    private readonly ICatalogDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;

    private IDbContextTransaction? _currentTransaction;

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
```
