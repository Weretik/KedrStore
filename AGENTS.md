# AGENTS.md

## Project Context
- Stack: `ASP.NET Core`
- Language: `C#`
- Package manager: `NuGet`
- Backend only repository (API/Application/Domain/Infrastructure).
- Architectural baseline: `CQRS + DDD + Mediator (MIT) + FluentValidation`.
- Follow existing solution structure, naming, and module boundaries.

## Engineering Standard (2026, Production-Grade)
- Prioritize correctness, deterministic behavior, operability, and backward compatibility.
- Prefer explicit design over implicit behavior.
- Optimize for maintainability in multi-team development:
  - clear ownership boundaries
  - explicit contracts
  - observable runtime behavior
  - safe evolvability

## Mandatory Delivery Flow
For non-trivial tasks always follow:
1. Short plan (scope + assumptions + risks)
2. Implementation
3. Verification (restore/build/tests + targeted checks)
4. Report (changes, verification, residual risks)

## Architecture & Layering (DDD + CQRS)
- Keep dependency direction strict:
  - `API -> Application -> Domain`
  - `Infrastructure` depends on inner layers, never vice versa.
- Domain layer:
  - owns business invariants, entities, value objects, and domain services.
  - must not depend on `Infrastructure`.
  - must not contain transport DTO concerns.
- Application layer:
  - orchestrates use-cases via commands/queries.
  - uses abstractions (repositories/services), not infrastructure implementations directly.
- API layer:
  - thin endpoints/controllers only.
  - no business rules in controllers.
- Infrastructure layer:
  - adapters for persistence/integration/external services.
  - no domain rule ownership.

## CQRS Rules
- Commands change state; queries do not change state.
- Do not mix write behavior into query handlers.
- Commands should return minimal required result (`Unit`, id, or status contract).
- Queries should use read-optimized projections and avoid unnecessary tracking.
- Keep command/query models task-specific; avoid "god DTOs".
- Preserve idempotency for commands where retries/replays are realistic.

## Mediator (MIT) Rules
- All use-case entry points should go through `Mediator` requests/handlers unless an existing module convention explicitly differs.
- Keep one handler responsible for one use-case.
- Use pipeline behaviors for cross-cutting concerns (validation, logging, transactions, performance, exception flow) according to current project conventions.
- Do not move business logic into behaviors; behaviors are cross-cutting only.
- Propagate `CancellationToken` through mediator flows.

## DDD Rules
- Enforce invariants inside domain model boundaries.
- Prefer value objects for validated conceptual primitives.
- Avoid anemic domain model when behavior belongs to entity/aggregate.
- Respect aggregate boundaries and transactional consistency rules.
- Do not leak persistence concerns into domain objects.
- Domain events: use existing project lifecycle only (`collect -> dispatch -> clear`).

## FluentValidation Rules
- Validation must be explicit and centralized via existing validator pipeline.
- Separate input validation from business invariants:
  - FluentValidation: contract/input rules
  - Domain/Application: business rules and state-based invariants
- Keep error messages consistent with existing API error format.
- Do not duplicate same rules in multiple layers unless intentionally defensive.

## Existing Project Approaches (Must Respect)
- `Ardalis.Result` for application result contracts and API mapping.
- `Ardalis.Specification` for reusable query specifications where applicable.
- `IUnitOfWork` for transaction boundary orchestration.
- Domain events via `IDomainEventContext` + dispatcher pipeline behavior.
- Assembly markers for DI scanning and registration boundaries.
- Background jobs via existing abstraction (`IBackgroundJobService`) instead of direct scheduler coupling.
- Existing logging stack is Serilog; keep current enrichment and sink strategy.

## ASP.NET Core Conventions
- Use project-configured .NET/C# versions and analyzers.
- Respect nullable reference types.
- Prefer async I/O end-to-end; avoid sync-over-async.
- Keep mappings explicit and consistent with project style.
- Do not silently alter serialization/model binding/global JSON behavior.
- Keep API error responses consistent with established project contract.

## Data & Persistence
- Respect existing persistence style (`EF Core`, Dapper, repositories, query services, etc.).
- No schema changes/migrations unless explicitly requested.
- Preserve query semantics and database contracts.
- Avoid:
  - N+1 query patterns
  - unnecessary tracking
  - full materialization when projection/paging is expected
- Preserve existing transactional boundaries and concurrency behavior.
- Do not weaken consistency guarantees silently.

## API Contracts & Versioning
- Public request/response contracts are stable by default.
- Do not rename routes, response fields, action names, or endpoint paths without approval.
- For critical write endpoints (`POST`/`PUT`/`PATCH` with side effects), support `Idempotency-Key` when duplicate execution can create business or financial risk.
- Idempotency implementation requirements (when enabled by endpoint/module):
  - same key + same request intent -> same logical result
  - repeated calls must not produce duplicate side effects
  - key scope and retention window must be explicit in module docs/config
  - do not persist sensitive payload data in idempotency storage
- Breaking changes require explicit report:
  - exact break
  - impacted consumers
  - migration path
  - rollout/backward-compat strategy
- Prefer additive evolution over destructive changes.

## Security & Privacy
- Do not weaken authentication/authorization/data protection.
- Never bypass permission checks for convenience.
- Never log secrets/tokens/connection strings/PII/sensitive payloads.
- Do not expose internal exception details in production responses.
- Apply least-privilege and deny-by-default mindset where applicable.

## Reliability & Resilience
- Avoid hidden side effects and non-deterministic behavior.
- Preserve idempotency guarantees where expected.
- Respect existing retry/timeout/circuit-breaker policies.
- Do not introduce implicit retries for write operations that can duplicate effects.
- Handle transient failures explicitly and observably.

## Observability & Diagnostics
- Structured logging with stable templates and useful context fields.
- Keep correlation/trace propagation intact if present.
- Log at appropriate levels (`Information`, `Warning`, `Error`) without noise.
- Error handling should be diagnosable without leaking sensitive internals.
- Preserve Serilog integration and existing enrichers/configuration approach.

## Performance Expectations
- Avoid algorithmic regressions in hot paths.
- Prefer projection-focused queries for read APIs.
- Minimize allocations and over-fetching on large datasets.
- Keep latency/throughput neutral or improved unless explicit tradeoff is requested.

## Testing Strategy
- Add/update tests proportionally to risk and change surface.
- Prefer:
  - unit tests for domain/application rules
  - integration tests for persistence/API contracts where project supports them
  - architecture tests when changing boundaries/dependencies
- Do not rewrite unrelated tests.

## Required Verification Commands
Run relevant checks before completion:

1. Restore
   - `dotnet restore`
2. Build
   - `dotnet build`
3. Tests
   - `dotnet test`

If solution/project path is required, use explicit `.sln`/`.csproj`.

If any command fails, report:
- exact command
- failure point
- likely cause (pre-existing vs introduced by current change)

## Change Policy
- No broad refactoring unless requested.
- No renames of files/namespaces/folders/projects without clear need.
- Do not modify `appsettings.*`, secrets, Docker, deployment, CI/CD, or infrastructure unless task-specific.
- No new external dependency unless clearly justified and approved by task context.
- Keep changes scoped, reviewable, and consistent with existing coding style.

## Output Format (Task Report)
For each task, report in this order:
1. What was changed
2. Which files were modified
3. What was verified
4. What could not be verified
5. Risks / what should be checked manually

## Definition of Done
Task is done when:
- requested behavior is implemented correctly;
- `CQRS + DDD + Mediator (MIT) + FluentValidation` boundaries are respected;
- public contracts remain stable unless explicitly changed by request;
- restore/build/tests were executed (or reason clearly stated);
- no obvious unrelated regressions were introduced.
