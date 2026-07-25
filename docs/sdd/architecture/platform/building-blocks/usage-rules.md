# BuildingBlocks usage rules

## Reuse before creating

| Need | Use first | Do not do |
| --- | --- | --- |
| Expected use-case failure | Ardalis.Result and existing Result mapping | throw ordinary validation/not-found exceptions |
| Input validation | FluentValidation plus ValidationBehavior | validate only in controller |
| Business invariant | DomainError and DomainException or aggregate behavior | move domain rule into a behavior |
| Current user/permissions | ICurrentUserService or IPermissionService | inject IHttpContextAccessor into Application |
| Domain event | BaseEntity collection plus dispatcher behavior | publish directly from controller |
| DB migration/seeding | IDatabaseMigrator/ISeeder startup extensions | migrate/seed inside a handler |
| 1C SOAP | OneCSoapClientFactory | duplicate SOAP authentication setup |
| Request diagnostics | existing behaviors/log helpers | log raw sensitive payloads |

## Extension rule

Add a shared primitive only when at least two modules need the same stable technical abstraction. A feature-specific abstraction stays in its owning Application module. A business concept never moves to BuildingBlocks.

Document an addition or order change to cross-cutting infrastructure in an ADR and in the feature specification.
