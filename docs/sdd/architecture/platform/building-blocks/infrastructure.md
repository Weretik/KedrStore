# BuildingBlocks.Infrastructure

## Структура

~~~text
BuildingBlocks.Infrastructure/
├── DependencyInjection/
│   └── AddInfrastructureServices
├── DomainEvents/
│   ├── EfDomainEventContext
│   └── MediatorDomainEventDispatcher
├── Migrations/
│   ├── IDatabaseMigrator
│   └── DbMigrator<TContext>
├── Seeding/
│   └── ISeeder
├── Extensions/
│   ├── UseAppMigrations
│   └── UseAppSeeders
└── Services/
    ├── HttpContextCurrentUserService
    ├── ClaimPermissionService
    ├── XmlToJsonConvector
    └── TelegramOptions
~~~

## Зареєстровані спільні сервіси

`AddInfrastructureServices` реєструє:

- IDomainEventContext як EfDomainEventContext;
- IDomainEventDispatcher як MediatorDomainEventDispatcher;
- IHttpContextAccessor;
- ICurrentUserService як HttpContextCurrentUserService;
- IPermissionService як ClaimPermissionService;
- конвертер Catalog XML-to-JSON;
- типізований HTTP-клієнт Telegram із параметрами Telegram.

## Запуск і дані

`DbMigrator<TContext>` застосовує EF-міграції через стратегію виконання провайдера. Методи розширення `UseAppMigrations` і `UseAppSeeders` залишаються в BuildingBlocks.Infrastructure, але `Host.Api` їх не викликає. Поточний шлях виконання — окремий `Host.Jobs --job=migrate`: він запускає зареєстровані `IDatabaseMigrator`, а потім унікальні за типом `ISeeder`. Див. [операційний порядок](../../../operations/data/migrations-and-seeding.md).

Це операції рівня host. Handler фічі не застосовує міграції та не виконує сидери самостійно.

## Поведінка безпеки

`HttpContextCurrentUserService` читає identity, claims, ролі та claims дозволів із поточного HTTP principal. `ClaimPermissionService` надає доступ ролі Admin, а для інших вимагає збіг поточного числового user ID та permission claim. Не обходь його прямим читанням `HttpContext` в Application.
