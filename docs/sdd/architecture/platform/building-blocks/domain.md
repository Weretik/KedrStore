# BuildingBlocks.Domain

## Структура

~~~text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity<TId>, IEntityId<T>
│   ├── IAggregateRoot
│   ├── IAuditableEntity, ISoftDelete
│   ├── IDomainEvent, IHasDomainEvents
│   └── IDomainError, DomainError
├── Entity/
│   ├── BaseEntity<TId>
│   └── BaseAuditableEntity<TId>
└── Exceptions/
    └── DomainException
~~~

## Наявні підходи

- `BaseEntity<TId>` містить ідентичність, порівняння за конкретним типом і неперехідним ID та методи накопичення доменних подій.
- `BaseAuditableEntity<TId>` додає `CreatedAt`, `UpdatedAt`, `IsDeleted` і явні методи позначення змін.
- `DomainError` — стандартне значення помилки з кодом і повідомленням.
- `DomainException` несе `IDomainError`; `ExceptionBehavior` відображає його в `Result.Invalid` для handlers, що повертають Result.
- `IAggregateRoot` позначає корені aggregates; `IHasDomainEvents` задає контракт життєвого циклу подій.

## Використання

Успадковуй aggregate/сутність від відповідної бази лише коли підходять її ID і життєвий цикл подій. Створюй подію через `AddDomainEvent` після завершеної доменної операції. Не dispatch-уй її в Domain.

Не додавай сюди EF-атрибути, HTTP-поняття, реалізації репозиторіїв або DTO транспортного рівня.
