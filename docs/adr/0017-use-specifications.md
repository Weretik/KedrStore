# ADR 0017: Use Specifications

## Date
2025-06-17

## Status
Accepted

## Context
Specifications are a design pattern used to encapsulate business rules and filtering logic in a reusable and composable manner. In the context of the Kedr E-Commerce Platform, specifications provide a way to define complex filtering criteria for domain entities, such as products, without polluting the domain model or application logic.

## Decision
We decided to use the Specification pattern in the project to:

1. Encapsulate filtering logic for domain entities in a reusable and testable manner.
2. Enable composability of business rules using logical operators (AND, OR, NOT).
3. Improve separation of concerns by keeping filtering logic out of the domain model and application layer.
4. Align with DDD principles by defining specifications as part of the domain layer.

## Consequences
### Positive
1. Provides a reusable and testable way to define business rules.
2. Enables composability of specifications using logical operators (AND, OR, NOT).
3. Improves separation of concerns by keeping filtering logic out of the domain model and application layer.
4. Simplifies testing and maintenance of business rules.

### Negative
1. Adds complexity to the codebase by introducing additional classes for specifications.
2. Requires developers to understand and correctly implement the Specification pattern.
3. May lead to performance issues if specifications are not optimized for large datasets.

## Example
Specifications will be implemented as classes inheriting from `SpecificationBase<T>` in the domain layer. For example:

**CategorySpecification.cs**:
```csharp
public class CategorySpecification : SpecificationBase<Product>
{
    private readonly CategoryId? _categoryId;

    public CategorySpecification(CategoryId? categoryId)
    {
        _categoryId = categoryId;
    }

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => _categoryId == null || product.CategoryId == _categoryId;
    }
}
```

**Usage in Repository**:
```csharp
var specification = new CategorySpecification(new CategoryId(1));
var filteredProducts = _dbContext.Products.ApplySpecification(specification);
```
