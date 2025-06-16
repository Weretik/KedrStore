# ADR 0020: Use Repositories

## Date
2025-06-17

## Status
Accepted

## Context
Repositories are a design pattern used to abstract data access logic and provide a clean interface for interacting with the data source. In the Kedr E-Commerce Platform, repositories ensure separation of concerns by isolating the domain layer from the infrastructure layer, enabling testability and maintainability.

## Decision
We decided to use repositories in the project to:

1. Encapsulate data access logic and provide a clean API for the domain layer.
2. Ensure separation of concerns between the domain and infrastructure layers.
3. Enable testability by allowing mocking of repositories in unit tests.
4. Align with Clean Architecture principles by defining repository interfaces in the domain layer and implementing them in the infrastructure layer.

## Consequences
### Positive
1. Improves maintainability by centralizing data access logic.
2. Enhances testability by enabling mocking of repository interfaces.
3. Ensures separation of concerns between layers.
4. Simplifies migration to different data sources by abstracting data access.

### Negative
1. Adds complexity by introducing additional abstraction layers.
2. Requires careful design of repository interfaces to avoid leaking infrastructure details.
3. May lead to over-engineering if repositories are used for trivial data access scenarios.

## Example
Repositories are implemented as interfaces in the domain layer and their concrete implementations in the infrastructure layer. For example:

**Domain Layer (IProductRepository.cs)**:
```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ProductId id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default);
}
```

**Infrastructure Layer (ProductRepository.cs)**:
```csharp
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        if (existingProduct != null)
        {
            existingProduct.Update(
                product.Name,
                product.Manufacturer,
                product.Price,
                product.CategoryId,
                product.Photo
            );
        }
        else
        {
            throw new InvalidOperationException("Product not found for update.");
        }
    }

    public async Task DeleteAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (product != null)
        {
            product.MarkAsDeleted();
        }
    }
}
```
