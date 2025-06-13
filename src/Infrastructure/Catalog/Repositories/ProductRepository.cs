using Domain.Catalog.Entities;
using Domain.Catalog.Interfaces;
using Domain.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Catalog.Repositories;

public class ProductRepository(CatalogDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await context.Products
            .AsNoTracking()
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

        // Загружаем существующую сущность для отслеживания
        var existingProduct = await context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        if (existingProduct != null)
        {
            // Используем доменный метод для обновления
            existingProduct.Update(product.Name, product.Manufacturer, product.Price, product.CategoryId, product.Photo);
            // EF Core автоматически отследит изменения
        }
        else
        {
            throw new InvalidOperationException("Product not found for update.");
        }
    }

    public async Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Загружаем сущность для отслеживания
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product != null)
        {
            product.MarkAsDeleted();
            // EF Core автоматически отследит изменение IsDeleted
        }
    }
}
