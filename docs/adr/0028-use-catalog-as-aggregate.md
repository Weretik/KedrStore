# ADR 0028: Use Catalog Aggregates

## Date
2025-06-17

## Status
Accepted

## Context
In Domain-Driven Design (DDD), aggregates are clusters of domain objects that are treated as a single unit for data changes. The Catalog module in the Kedr E-Commerce Platform is divided into two aggregates: Product and Category. This separation ensures that each aggregate encapsulates its own business rules and invariants, while maintaining consistency within its boundary.

## Decision
We decided to implement the Catalog module as two separate aggregates (Product and Category) to:

1. Encapsulate business rules and invariants within the boundaries of each aggregate.
2. Ensure consistency across related entities within each aggregate.
3. Simplify interactions by treating Product and Category as independent units.
4. Align with DDD principles by defining clear aggregate boundaries.

## Consequences
### Positive
1. Improves consistency by enforcing business rules within each aggregate.
2. Simplifies interactions with related entities by treating them as independent units.
3. Promotes separation of concerns by encapsulating logic within each aggregate.
4. Aligns with DDD principles, ensuring a clean and maintainable domain model.

### Negative
1. Adds complexity by introducing multiple aggregate boundaries.
2. Requires careful design to avoid aggregate size becoming too large.
3. May lead to performance issues if aggregate operations are not optimized.

## Example
The Catalog aggregates are implemented as follows:

**Product.cs**:
```csharp
public class Product : IAggregateRoot
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public CategoryId CategoryId { get; private set; }

    public Product(ProductId id, string name, decimal price, CategoryId categoryId)
    {
        Id = id;
        Name = name;
        Price = price;
        CategoryId = categoryId;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            throw new InvalidOperationException("Price must be greater than zero.");
        }

        Price = newPrice;
    }
}
```

**Category.cs**:
```csharp
public class Category : IAggregateRoot
{
    public CategoryId Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public Category(CategoryId id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddProduct(Product product)
    {
        if (_products.Any(p => p.Id == product.Id))
        {
            throw new InvalidOperationException("Product already exists in the category.");
        }

        _products.Add(product);
    }

    public void RemoveProduct(ProductId productId)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found in the category.");
        }

        _products.Remove(product);
    }
}
```

**Usage in Repository**:
```csharp
public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task SaveCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```
