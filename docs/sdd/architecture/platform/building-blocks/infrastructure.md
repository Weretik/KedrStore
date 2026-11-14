# BuildingBlocks.Infrastructure

## Structure

~~~text
BuildingBlocks.Infrastructure/
├── DependencyInjection/
│   ├── AddInfrastructureServices
│   └── OutboxRegistration (AddOutbox<TContext>(schema), AddOutboxHandler<TMessage, THandler>(name))
├── Outbox/
│   ├── OutboxMessage, ApplyOutbox(schema)   # <schema>."OutboxMessages"
│   ├── OutboxWriter                         # adds a message in the module's SaveChanges
│   ├── ModuleOutbox<TContext>, OutboxDispatcher  # IOutboxDispatcher: send after commit, due dispatch, retention
│   └── OutboxRetrySchedule                  # 1, 5, 15, 30, 60 minutes, then hourly; 10 attempts
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

The transactional outbox is described in [platform spec 006](../../../specs/platform/006-transactional-outbox/README.md) and the [module rules](../../../standards/module-rules.md#transactional-outbox).

## Registered shared services

`AddInfrastructureServices` registers:

- IDomainEventContext as EfDomainEventContext;
- IDomainEventDispatcher as MediatorDomainEventDispatcher;
- IHttpContextAccessor;
- ICurrentUserService as HttpContextCurrentUserService;
- IPermissionService as ClaimPermissionService;
- Catalog XML-to-JSON converter;
- typed Telegram HTTP client with Telegram options.

## Startup and data

`DbMigrator<TContext>` applies EF migrations through the provider execution strategy. The `UseAppMigrations` and `UseAppSeeders` extension methods remain in BuildingBlocks.Infrastructure, but `Host.Api` does not call them. The current execution path is the separate `Host.Jobs --job=migrate`: it runs the registered `IDatabaseMigrator` services, then `ISeeder` services unique by type. See the [operational sequence](../../../operations/data/migrations-and-seeding.md).

These are host-level operations. A feature handler does not apply migrations or run seeders itself.

## Security behavior

`HttpContextCurrentUserService` reads identity, claims, roles, and permission claims from the current HTTP principal. `ClaimPermissionService` grants access to the Admin role and, for others, requires the current numeric user ID to match along with a permission claim. Do not bypass it by reading `HttpContext` directly in Application.
