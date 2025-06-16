# ADR 0032: Use Unit, Integration, and Architecture Tests

## Date
2025-06-17

## Status
Accepted

## Context
Testing is a critical aspect of software development, ensuring the reliability and maintainability of the application. Dividing tests into Unit, Integration, and Architecture levels provides a structured approach to testing, enabling thorough validation of individual components, system interactions, and adherence to architectural principles.

## Decision
We decided to use Unit, Integration, and Architecture Tests in the project to:

1. Validate individual components and their logic through Unit Tests.
2. Ensure correct interaction between components and external systems through Integration Tests.
3. Verify adherence to architectural principles and constraints through Architecture Tests.
4. Align with best practices for structured and comprehensive testing.

## Consequences
### Positive
1. Improves reliability and maintainability of the application.
2. Provides a structured approach to testing, ensuring thorough validation.
3. Simplifies debugging by isolating issues at different levels of testing.
4. Promotes adherence to architectural principles.

### Negative
1. Adds complexity to the testing setup and maintenance.
2. Requires developers to understand and correctly implement tests at different levels.
3. May increase the time required for testing and CI/CD pipelines.

## Example
Testing is implemented as follows:

**Unit Tests**:
```csharp
[Fact]
public void CalculatePrice_ShouldReturnCorrectValue()
{
    var product = new Product("Test", 100);
    var price = product.CalculatePrice();

    Assert.Equal(100, price);
}
```

**Integration Tests**:
```csharp
[Fact]
public async Task GetProduct_ShouldReturnProductFromDatabase()
{
    var productId = Guid.NewGuid();
    await _dbContext.Products.AddAsync(new Product(productId, "Test", 100));
    await _dbContext.SaveChangesAsync();

    var product = await _productRepository.GetByIdAsync(productId);

    Assert.NotNull(product);
    Assert.Equal("Test", product.Name);
}
```

**Architecture Tests**:
```csharp
[Fact]
public void DomainLayer_ShouldNotDependOnApplicationLayer()
{
    var domainAssembly = typeof(Product).Assembly;
    var applicationAssembly = typeof(GetProductsQuery).Assembly;

    var result = ArchUnitNET.Domain.ArchRuleDefinition.Types()
        .That().ResideInAssembly(domainAssembly)
        .Should().NotDependOn(applicationAssembly)
        .Check();

    Assert.True(result.IsSuccessful);
}
```
