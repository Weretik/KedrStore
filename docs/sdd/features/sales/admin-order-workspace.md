# Admin order workspace

**Module:** Sales. **Consumer:** React Admin (kedr-web `libs/admin/orders/data-access`).

[AdminOrderWorkspaceController](../../../../src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs) exposes separate `/api/admin/order-workspace` routes (policy `CanManageOrders`) and dispatches each operation to its own Application handler. Existing `/api/admin/orders` remains a separate contract. The [order reader](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceOrderReader.cs) performs search, sorting, and paging; [pricing](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Pricing/AdminWorkspacePricing.cs) uses current stock and the customer price from the [price resolver](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Pricing/CustomerPriceResolver.cs): the price type of the nearest customer rule on the product's category or any parent category, otherwise the customer's default type. Each line may carry a per-unit `priceAdjustment` in UAH (negative is a discount, positive a markup); the unit price is the customer price plus the adjustment and a discount may not take it below 1 UAH per unit (400 `price_below_minimum`; a product cheaper than 1 UAH cannot be discounted). `POST customer-prices` returns customer prices for up to 100 products without checking stock, so the checkout table shows them even when the quote fails. Orders and drafts store the adjustment per line; the order list returns `adjustmentAmount`, and 1C receives only the final line amounts. Insufficient stock returns the expected 400 `unavailable_product` with product name and quantities.

The line price rules (stock, customer price, the 1 UAH floor, an amount that fits) live in the domain [OrderLinePricing](../../../../src/Sales/Sales.Domain/Orders/Pricing/OrderLinePricing.cs). Request input is checked by a validator per use case through the shared [input rules](../../../../src/Sales/Sales.Application/Features/Orders/AdminWorkspace/WorkspaceInputRules.cs); the services return `DomainResult` with the [workspace errors](../../../../src/Sales/Sales.Application/Features/Orders/AdminWorkspace/WorkspaceErrors.cs) and throw nothing for a business case. [WorkspaceErrorAnswers](../../../../src/Sales/Sales.Api/Controllers/WorkspaceErrorAnswers.cs) answers every failure as `{code, message}` in Ukrainian: a 400 keeps the code of the rule that failed (`invalid_lines`, `invalid_price_adjustment`, `unavailable_product`, `invalid_date_range`, `invalid_sort` and the others), a 404 says `not_found` and a 409 `conflict`. During checkout, [creator](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Checkout/WorkspaceOrderCreator.cs) recalculates the total, compares the fingerprint, and atomically writes Order and Pending 1C sync through the existing OrderCreationStore.

Drafts use a separate [model](../../../../src/Sales/Sales.Domain/Orders/Entities/OrderDraft.cs), [reader](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceDraftReader.cs), and [write adapter](../../../../src/Sales/Sales.Infrastructure/Orders/Admin/Drafts/WorkspaceDraftWriter.cs). They do not receive an Order number or go to 1C. The details response contains a current server-side quote or `pricingStatus=Unavailable` with saved lines and `null` for unavailable prices; PUT/DELETE use `If-Match`. Checkout from a draft does not change the draft.

## Structure diagram

```mermaid
flowchart LR
    Admin[React Admin] --> Controller[AdminOrderWorkspaceController]
    Controller --> Handlers[Application handlers by use case]
    Handlers --> Readers[Order and Draft readers]
    Handlers --> Creator[Order creator]
    Handlers --> DraftWriter[Draft writer]
    Handlers --> Pricing[AdminWorkspacePricing]
    Pricing --> Resolver[CustomerPriceResolver]
    Resolver --> Catalog[Catalog: products, categories and prices]
    Resolver --> Customers
    Pricing --> Customers[Sales: customers and category rules]
    Creator --> Orders[OrderCreationStore]
    Orders --> Order[Order and Pending 1C sync]
    DraftWriter --> DraftTables[OrderDrafts and OrderDraftLines]
    Readers --> DraftTables
    Sync[Existing 1C job] --> Order
```

## Code structure

```text
src/Sales/
├── Sales.Api/Controllers/AdminOrderWorkspaceController.cs           # HTTP entry (CanManageOrders)
├── Sales.Api/Controllers/WorkspaceErrorAnswers.cs                   # failure result → status + {code, message}
├── Sales.Application/
│   ├── Contracts/Orders/
│   │   ├── WorkspaceOrderContracts.cs                         # order DTOs
│   │   ├── WorkspaceDraftContracts.cs                         # draft DTOs
│   │   ├── WorkspaceQuoteContracts.cs                         # quote DTOs
│   │   ├── WorkspacePageContracts.cs                          # paging
│   │   ├── WorkspaceInputLine.cs                              # request line
│   │   ├── IWorkspaceOrderReader.cs                           # order read port
│   │   └── IWorkspaceDraftWriter.cs                           # draft write port
│   └── Features/Orders/AdminWorkspace/
│       ├── WorkspaceInputRules.cs                             # shared validator rules with public error codes
│       ├── WorkspaceErrors.cs                                 # not found, version and idempotency conflicts
│       ├── WorkspaceDateRange.cs                              # Kyiv calendar-day filter
│       ├── WorkspaceSorts.cs                                  # allowed list sorts
│       ├── */*/Validators/*Validator.cs                       # one validator per query and command
│       ├── Orders/GetList/GetWorkspaceOrdersQueryHandler.cs  # order list
│       ├── Orders/Quote/QuoteWorkspaceOrderQueryHandler.cs   # server-side quote
│       ├── Orders/CustomerPrices/GetWorkspaceCustomerPricesQueryHandler.cs # customer prices without stock
│       ├── Orders/Create/CreateWorkspaceOrderCommandHandler.cs # checkout
│       ├── Drafts/GetList/GetWorkspaceDraftsQueryHandler.cs  # draft list
│       ├── Drafts/Create/CreateWorkspaceDraftCommandHandler.cs # create draft
│       ├── Drafts/Update/UpdateWorkspaceDraftCommandHandler.cs # edit draft
│       └── Drafts/Delete/DeleteWorkspaceDraftCommandHandler.cs # delete draft
├── Sales.Domain/Orders/Entities/OrderDraft.cs                       # persisted draft model
├── Sales.Domain/Orders/Pricing/OrderLinePricing.cs                  # line price rules → DomainResult
└── Sales.Infrastructure/
    ├── Orders/Admin/Read/                                      # order and draft reads
    ├── Orders/Admin/Checkout/WorkspaceOrderCreator.cs          # order checkout
    ├── Orders/Admin/Drafts/WorkspaceDraftWriter.cs             # draft writes
    ├── Orders/Admin/Drafts/WorkspaceDraftDetails.cs            # details projection with fresh prices
    ├── Orders/Admin/Pricing/AdminWorkspacePricing.cs           # quote: stock, adjustments, totals, fingerprint
    ├── Orders/Admin/Pricing/CustomerPriceResolver.cs           # customer price with category inheritance
    ├── Orders/Admin/Idempotency/WorkspaceIdempotency.cs        # hash and DB conflict handling for retries
    ├── Migrations/20261021135819_AddAdminOrderDrafts.cs             # additive table migration
    └── Migrations/20261029130909_AddOrderLinePriceAdjustment.cs     # PriceAdjustment on order and draft lines
```

## Tests

[OrderDraftTests.cs](../../../../tests/UnitTests/Sales/Domain/OrderDraftTests.cs) · [OrderLinePricingTests.cs](../../../../tests/UnitTests/Sales/Domain/OrderLinePricingTests.cs) · [WorkspaceValidatorsTests.cs](../../../../tests/UnitTests/Sales/Application/WorkspaceValidatorsTests.cs) · [WorkspaceDateRangeTests.cs](../../../../tests/UnitTests/Sales/Application/WorkspaceDateRangeTests.cs) · [OrderDraftPostgresTests.cs](../../../../tests/IntegrationTests/Sales/Persistence/OrderDraftPostgresTests.cs) · [WorkspacePricingTests.cs](../../../../tests/IntegrationTests/Sales/Persistence/WorkspacePricingTests.cs) · [WorkspaceOrderAdjustmentPostgresTests.cs](../../../../tests/IntegrationTests/Sales/Persistence/WorkspaceOrderAdjustmentPostgresTests.cs)

The checkout handler has no dedicated test.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Workspace|FullyQualifiedName~OrderDraft"` and `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~Workspace|FullyQualifiedName~OrderDraft"`.

## Related sources

[Order contract](../../contracts/sales/admin-order-workspace.openapi.yaml) · [Draft contract](../../contracts/sales/admin-order-drafts.openapi.yaml) · [SDD 004](../../specs/sales/004-admin-order-workspace/README.md) · [SDD 005](../../specs/sales/005-admin-order-drafts/README.md) · [SDD 008](../../specs/sales/008-workspace-customer-prices-and-adjustments/README.md).
