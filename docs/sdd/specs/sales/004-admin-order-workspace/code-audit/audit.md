# Аудит відповідальностей коду — Admin workspace Sales

- **Дата:** 2026-10-21.
- **Обсяг:** SDD [004](../README.md) і [005](../../005-admin-order-drafts/README.md); API, Application, Domain draft та Infrastructure.
- **Правила:** [межі шарів](../../../../architecture/overview/layers.md), [SDD виправлення](../../006-admin-workspace-structure-remediation/README.md).
- **Метод:** статичний огляд і вузькі автоматизовані перевірки після структурних змін.

## Знахідки й рішення

| ID | Початкова проблема | Рішення та фактичний власник | Статус і доказ |
| --- | --- | --- | --- |
| AF-001 | Один partial Infrastructure service тримав дев'ять сценаріїв; API викликав його без Mediator. | `Sales.Api/Controllers/AdminOrderWorkspaceController.cs` передає кожну операцію окремому handler у `Sales.Application/Features/Orders/AdminWorkspace/{Orders,Drafts}/<scenario>/`. Persistence, SQL, pricing і транзакції рознесено в адаптери `Sales.Infrastructure/Orders/Admin/{Read,Checkout,Drafts,Pricing,Idempotency}/`. Quote — read-only Query. | **verified** для структури шарів: Sales.Api build, 7 layer architecture tests. |
| AF-002 | Один контрактний файл містив DTO чотирьох потоків і широкий інтерфейс на дев'ять методів. | `Sales.Application/Contracts/Orders/Workspace{Quote,Order,Draft,Page}Contracts.cs` і `WorkspaceInputLine.cs` групують DTO за роллю; вузькі `IWorkspace*.cs` задають окремі порти. | **verified**: Sales.Api build; HTTP-дії й типи їхніх відповідей збережено. |
| AF-003 | Читання деталей створювало невикористану локальну проєкцію позицій. | `Sales.Infrastructure/Orders/Admin/Read/WorkspaceOrderReader.cs` бере позиції лише з SQL-запиту `OrderLines`. | **verified**: огляд коду й Sales.Api build. |

## Рішення залишити

`Sales.Domain/Orders/Entities/OrderDraft.cs` зберігає інваріанти агрегату; EF mappings і migration залишаються в Infrastructure. `WorkspaceDraftWriter.cs` має одну цілісну відповідальність запису агрегату, а `WorkspaceDraftDetails.cs` спільно формує відповідь для read/create/update без додаткового повторного читання чернетки після запису. Найдовший файл нової Admin Infrastructure області має 122 рядки; це спостереження, а не числовий поріг.

## Перевірка й межі

`dotnet build src/Sales/Sales.Api/Sales.Api.csproj --no-restore -v:q`: exit 0, 0 errors/warnings. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~Layers -v:q`: 7 тестів, exit 0. `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~OrderDraftTests -v:q`: 5 тестів, exit 0. Відносні Markdown-посилання перевірено. PostgreSQL integration, міграцію, job 1C та повний solution test не запускали; це не є доказом продуктивності SQL або поведінки реальної БД.
