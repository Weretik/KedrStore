# Sales — клієнти: дані

**Джерело:** [Sales snapshot](../../../../src/Sales/Sales.Infrastructure/Migrations/SalesDbContextModelSnapshot.cs) і [конфігурації](../../../../src/Sales/Sales.Infrastructure/Configurations/), перевірено 2026-10-21. [Назад до карти БД](README.md) · [фіча клієнтів](../../features/sales/customers.md).

| Таблиця | PK | Призначення й важливі поля |
| --- | --- | --- |
| `Counterparties` | `Id` | Контрагент: `Name`, `Email`, `Phone`, `IdentityUserId`, `DefaultPriceTypeId`, дати. |
| `CounterpartyCategoryPriceTypes` | `CounterpartyId`, `CategoryId` | Тип ціни `PriceTypeId` для категорії контрагента. |

У snapshot немає фізичних FK від `CounterpartyCategoryPriceTypes` до `Counterparties` чи таблиць Catalog, а також від `Counterparties.IdentityUserId` до Identity. Це смислові посилання між даними, тому ER-діаграма FK для цих двох таблиць порожня. `Orders.CounterpartyId` має фізичний FK до `Counterparties.Id`; замовлення описано [окремо](sales-orders.md).
