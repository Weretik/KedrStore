# ADR 0056: Use Mapster for Object Mapping

## Status

Accepted

## Date

2025-10-28

## Context

KedrStore requires efficient object mapping between DTOs, domain entities, and view models across different layers (Application ↔ Domain ↔ UI). Previously, AutoMapper was used for this purpose (see ADR 0006). However, as the application grew, we identified several performance and complexity concerns with AutoMapper:

- Runtime reflection-based mapping can impact performance in high-throughput scenarios
- Configuration complexity increases with custom mappings and nested objects
- Compilation time mapping validation is limited
- Memory overhead from expression tree compilation and caching

**Mapster** was selected as a replacement due to:

- Superior performance through source generation and compile-time mapping
- Minimal configuration required for common scenarios
- Type-safe mapping with compile-time validation
- Smaller memory footprint and faster execution
- Simpler API for custom mappings
- Built-in support for projection and query optimization

## Decision

We will migrate from **AutoMapper** to **Mapster** for mapping between:
- DTOs and Domain Entities
- Domain Entities and ViewModels (for UI)
- Command DTOs and Entities (in command handlers)
- Query projections for database optimization

Mapster configuration will be organized per feature/module using `IRegister` interface (e.g., `ProductMappingConfig`, `OrderMappingConfig`) and registered via DI during application startup.

## Consequences

### Positive

- Significantly improved mapping performance (up to 3-5x faster than AutoMapper)
- Reduced memory allocations and garbage collection pressure
- Compile-time type safety catches mapping errors early
- Simpler configuration syntax reduces maintenance overhead
- Better support for query projection optimization with EF Core
- Smaller library footprint

### Negative

- Requires migration effort from existing AutoMapper profiles
- Team needs to learn new mapping API (though simpler than AutoMapper)
- Less mature ecosystem compared to AutoMapper
- Some advanced AutoMapper features may need alternative implementation

## Implementation

Mapster configurations are registered using the `IRegister` interface:

```csharp
public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);

        config.NewConfig<CreateProductCommand, Product>()
            .IgnoreNullValues(true);
    }
}
```

The configurations are registered in the DI container:

```csharp
services.AddMapster();
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
```

## Example Usage

**Basic mapping:**
```csharp
var productDto = product.Adapt<ProductDto>();
```

**With custom configuration:**
```csharp
var productDto = product.Adapt<ProductDto>(config =>
    config.IgnoreNullValues(true));
```

**Query projection (EF Core optimization):**
```csharp
var products = await context.Products
    .ProjectToType<ProductDto>()
    .ToListAsync();
```

## Migration Notes

- Existing AutoMapper profiles should be gradually migrated to Mapster configurations
- Both libraries can coexist during the migration period
- Focus on migrating high-traffic mappings first for immediate performance benefits
- Update unit tests to use Mapster mapping assertions
