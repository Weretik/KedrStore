# Manager catalog

**Static code check:** 2026-10-16. **Module:** Sales.

Authorized product list for sales.

## Structure and relationships

[Sales CatalogController](../../../../src/Sales/Sales.Api/Controllers/CatalogController.cs) serves `GET /api/sales/{lang}/catalog/products`; the [handler](../../../../src/Sales/Sales.Application/Features/Catalog/GetList/GetCatalogListQueryHandler.cs) builds a read result through Catalog contracts. [Price policy](../../../../src/Sales/Sales.Infrastructure/Pricing/DefaultPricePolicyProvider.cs) belongs to Sales. This is a separate use case from the anonymous public catalog.

## Code structure

The tree lists verified entry points and key components, not every supporting file.

```text
src/
└── Sales/
    ├── Sales.Api/Controllers/CatalogController.cs  # HTTP entry point
    ├── Sales.Application/Features/Catalog/GetList/GetCatalogListQueryHandler.cs  # use-case handler
    └── Sales.Infrastructure/Pricing/DefaultPricePolicyProvider.cs  # infrastructure implementation
```

## Structure diagram

```mermaid
flowchart LR
    A["Sales CatalogController"] --> B["GetCatalogListQueryHandler"]
    B --> C["Catalog contracts"]
    B --> D["DefaultPricePolicyProvider"]
    C --> E["Manager catalog"]
    D --> E
```

## Related sources

[Contract](../../contracts/sales/catalog.openapi.yaml) · [Public catalog](../catalog/public-products.md).
