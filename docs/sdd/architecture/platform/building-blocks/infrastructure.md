# BuildingBlocks.Infrastructure

## Structure

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

## Registered shared services

AddInfrastructureServices registers:

- IDomainEventContext as EfDomainEventContext;
- IDomainEventDispatcher as MediatorDomainEventDispatcher;
- IHttpContextAccessor;
- ICurrentUserService as HttpContextCurrentUserService;
- IPermissionService as ClaimPermissionService;
- Catalog XML-to-JSON converter;
- Telegram typed HTTP client, configured from Telegram options.

## Startup and data approach

DbMigrator<TContext> applies EF migrations through the provider execution strategy. UseAppMigrations runs all registered IDatabaseMigrator instances. UseAppSeeders runs all unique registered ISeeder instances in a scoped startup operation.

These startup extensions are host-level operations. A feature handler must not apply migrations or seed data itself.

## Security behavior

HttpContextCurrentUserService reads identity, claims, roles and permission claims from the current HTTP principal. ClaimPermissionService grants Admin role access, otherwise requires the matching current numeric user ID and permission claim. Do not bypass it by reading HttpContext in Application.
