# BuildingBlocks Usage Rules

## Reuse before creating

| Need | Use first | Avoid |
| --- | --- | --- |
| Expected use-case error | Ardalis.Result and the existing Result mapping | Throwing ordinary validation or missing-data exceptions |
| Input validation | FluentValidation and ValidationBehavior | Validating only in the controller |
| Business invariant | DomainError and DomainException, or aggregate behavior | Moving a domain rule into a behavior |
| Current user/permissions | ICurrentUserService or IPermissionService | Injecting IHttpContextAccessor into Application |
| Domain event | BaseEntity collection and dispatcher behavior | Publishing directly from the controller |
| Database migration/seed | IDatabaseMigrator/ISeeder through the separate `Host.Jobs --job=migrate` | Running migrations/seeders inside a handler |
| 1C SOAP | OneCSoapClientFactory | Duplicating SOAP authentication setup |
| Request diagnostics | Existing behaviors/log helpers | Logging raw sensitive payloads |

## Extension rule

Add a shared primitive only when at least two modules need the same stable technical abstraction. An abstraction for one feature stays in its Application module. Business concepts do not move into BuildingBlocks.

Document additions to or ordering changes in cross-cutting infrastructure in an [ADR](../../decisions/README.md) and the feature specification.
