namespace Application.Catalog.Interfaces
{
    public interface ICatalogDbContext
    {
        DbSet<Category> Categories { get; }
        DbSet<Product> Products { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        void DiscardChanges();
    }
}
