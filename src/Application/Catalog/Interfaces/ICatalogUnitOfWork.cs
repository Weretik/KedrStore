namespace Application.Catalog.Interfaces
{
    public interface ICatalogUnitOfWork : IDisposable
    {
        ICatalogDbContext DbContext { get; }
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
