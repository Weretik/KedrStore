# Sales — customers: data

**Source:** [Sales snapshot](../../../../src/Sales/Sales.Infrastructure/Migrations/SalesDbContextModelSnapshot.cs) and [configurations](../../../../src/Sales/Sales.Infrastructure/Configurations/), checked 2026-10-21. [Back to database map](README.md) · [customers feature](../../features/sales/customers.md).

| Table | PK | Purpose and important fields |
| --- | --- | --- |
| `Counterparties` | `Id` | Counterparty: `Name`, `Email`, `Phone`, `IdentityUserId`, `DefaultPriceTypeId`, dates. |
| `CounterpartyCategoryPriceTypes` | `CounterpartyId`, `CategoryId` | Price type `PriceTypeId` for a counterparty's category. |

The snapshot has no physical FKs from `CounterpartyCategoryPriceTypes` to `Counterparties` or Catalog tables, nor from `Counterparties.IdentityUserId` to Identity. These are semantic references, so the FK ER diagram for these tables is empty. `Orders.CounterpartyId` has a physical FK to `Counterparties.Id`; see [orders](sales-orders.md).
