# Feature: Product Catalog – KedrStore

## Overview

This document describes the implementation of the **Product Catalog** feature in the KedrStore project. It covers how the catalog is structured across all layers of the Clean Architecture stack, including domain model, use cases, MediatR queries, and UI integration.

---

## Business Purpose

The catalog allows users to:

- Browse available products
- Filter by category
- Search by name
- Paginate product listings

This is a read-only feature at the current stage.

---

## Domain Layer

| Component    | Location                                                                      |
| ------------ | ----------------------------------------------------------------------------- |
| `Product`    | `Domain/Catalog/Entities/Product.cs`                                          |
| `Category`   | `Domain/Catalog/Entities/Category.cs`                                         |
| `ProductId`  | `Domain/Catalog/ValueObjects/ProductId.cs`                                    |
| `CategoryId` | `Domain/Catalog/ValueObjects/CategoryId.cs`                                   |
| `Money`      | `Domain/Common/ValueObjects/Money.cs`                                         |
| Repositories | `Domain/Catalog/Repositories/IProductRepository.cs`, `ICategoryRepository.cs` |

---

## Application Layer

| Type      | File Name                               |
| --------- | --------------------------------------- |
| Query     | `GetProductsQuery.cs`                   |
| Handler   | `GetProductsQueryHandler.cs`            |
| DTO       | `ProductDto.cs`                         |
| Result    | `AppResult<PagedResult<ProductDto>>`    |
| Validator | *(optional, not needed for read query)* |

### `GetProductsQuery`

```csharp
public record GetProductsQuery(int Page, int PageSize, string? Search, Guid? CategoryId)
    : IQuery<AppResult<PagedResult<ProductDto>>>;
```

### `GetProductsQueryHandler`

```csharp
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, AppResult<PagedResult<ProductDto>>>
{
    public async Task<AppResult<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var spec = new ProductSpecification(request.Search, request.CategoryId);
        var result = await _repository.Paginated(spec, request.Page, request.PageSize);
        return AppResult.Success(_mapper.Map<PagedResult<ProductDto>>(result));
    }
}
```

---

## Infrastructure Layer

| Type          | File                                        |
| ------------- | ------------------------------------------- |
| Repository    | `ProductRepository.cs`                      |
| EF Config     | `ProductConfiguration.cs`                   |
| Pagination    | `PaginationExtensions.cs`, `PagedResult.cs` |
| Specification | `ProductSpecification.cs` (if used)         |

---

## Presentation Layer (Blazor UI)

| Component | Location                             |
| --------- | ------------------------------------ |
| Page      | `Pages/Catalog.razor`                |
| Component | `Components/ProductCard.razor`       |
| State     | `CatalogStateContainer.cs` (if used) |
| ViewModel | `_mapped` from `ProductDto`          |

- UI triggers `GetProductsQuery` via DI
- Filters and paging parameters are bound to query inputs
- Results are shown via `MudGrid`, `ProductCard`, or other visual components

---

## Data Flow Diagram

```text
UI (Catalog.razor)
     ▼
[Inject IMediator] → Send(GetProductsQuery)
     ▼
Application Layer → GetProductsQueryHandler
     ▼
Repository → ProductRepository
     ▼
Database → EF Query + Pagination
     ▼
Handler → AutoMapper to ProductDto
     ▼
AppResult<PagedResult<ProductDto>>
     ▼
UI: Render Grid/Repeater
```

---

## Testing

| Type        | Coverage                                     |
| ----------- | -------------------------------------------- |
| Unit        | `GetProductsQueryHandlerTests` (mocked repo) |
| Integration | `ProductRepositoryTests`                     |

Tests ensure pagination works, filtering is respected, and queries return mapped results.

---

## Summary

The Catalog feature in KedrStore is a read-focused use case implemented using the CQRS pattern with MediatR. It is modular, testable, and cleanly separated by layer. Future enhancements may include caching, category tree UI, and admin-side mutation features (CRUD).

