# Архітектура backend

KedrStore — модульний backend на ASP.NET Core і .NET 10. Поточні підходи: Clean Architecture, DDD, CQRS, Mediator, FluentValidation, Ardalis.Result, EF Core/PostgreSQL і Serilog. Перелік прямих пакетів та їхніх оголошених версій є в [каталозі NuGet](platform/nuget-packages.md).

## Навігація за питанням

### Загальна картина

- [Карта системи](overview/system-map.md) — hosts, модулі та напрям залежностей.
- [Структура проєкту](overview/project-structure.md) — дерево solution і шари.
- [Межі шарів](overview/layers.md) — відповідальність і заборонені залежності.
- [Карта модулів](overview/modules.md) — відповідальність Catalog, Sales, Identity та BuildingBlocks.
- [Продуктовий словник](../../product/glossary.md) — терміни каталогу, дверей, фурнітури й Cosmos.

### Розміщення коду за шарами

- [Domain](layers/domain-structure.md)
- [Application](layers/application-structure.md)
- [Infrastructure](layers/infrastructure-structure.md)
- [API та Host](layers/api-host-structure.md)

### Виконання запиту й інтеграцій

- [Шлях сценарію використання](runtime/use-case-flow.md)
- [Mediator behaviors](runtime/mediator-behaviors.md)
- [Наскрізний pipeline](runtime/cross-cutting.md)
- [Дані й інтеграції](runtime/persistence-and-integrations.md)
- [Інтеграція з 1C](integrations/one-c/README.md)

### Платформа

- [Технологічний стек](platform/technology-stack.md)
- [Прямі NuGet-залежності](platform/nuget-packages.md)
- [Інструменти збірки й доставки](platform/tooling-and-delivery.md)
- [Довідник BuildingBlocks](platform/building-blocks/README.md)
- [Архітектурні рішення](decisions/README.md)

### Identity та контроль доступу

- [Identity, автентифікація й авторизація](security/README.md)

Цей розділ описує поточну архітектуру й сталі межі. Запланована зміна спершу належить до [специфікації](../specs/README.md); після прийняття й реалізації сталого рішення онови відповідний архітектурний документ і, за потреби, [ADR](decisions/README.md).
