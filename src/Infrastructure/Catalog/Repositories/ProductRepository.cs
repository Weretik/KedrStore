namespace Infrastructure.Catalog.Repositories;

public class ProductRepository(
    ICatalogDbContext context, IDateTimeProvider clock)
    : IProductRepository
{
    public IQueryable<Product> QueryProducts()
    {
        return context.Products.AsNoTracking();
    }
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await context.Products.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingProduct = await context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        if (existingProduct != null)
        {
            existingProduct.Update(
                product.Name,
                product.Manufacturer,
                product.Price,
                product.CategoryId,
                product.Photo,
                clock.UtcToday
            );
        }
        else
        {
            throw new InvalidOperationException("Product not found for update.");
        }
    }

    public async Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = await context.Products
            .FirstOrDefaultAsync(product => product.Id == id && !product.IsDeleted, cancellationToken);

        if (product != null)
        {
            product.MarkAsDeleted(clock.UtcToday);
        }
    }

}
