# AGENTS.md

## Project Context
- Stack: `ASP.NET Core`
- Language: `C#`
- Package manager: `NuGet`
- This repository contains only the backend application/services.
- Follow the existing solution structure, naming, and architectural boundaries.
- Do not introduce new infrastructure, libraries, or patterns unless explicitly requested.

## Goals for Codex
- Make the smallest possible change that solves the task.
- Preserve existing public API contracts unless explicitly instructed otherwise.
- Reuse existing application services, domain abstractions, validators, mappings, and infrastructure registrations before adding new ones.
- For non-trivial tasks: brief plan -> implementation -> verification -> report risks.

## Backend Architecture Rules
- Respect existing architecture and layer boundaries.
- If the project uses layered architecture, keep the dependency direction intact, for example:
    - `API -> Application -> Domain`
    - `Infrastructure` depends on inner layers, not the other way around
- Do not reference `Infrastructure` from `Domain`.
- Do not put business logic into controllers/endpoints.
- Do not use transport models/DTOs as domain models.
- Keep domain rules in the appropriate application/domain layer.
- Prefer extending existing handlers/services/use-cases instead of duplicating logic.

## ASP.NET Core Conventions
- Use the C# version configured by the project.
- Respect nullable reference types if enabled.
- Prefer async I/O with `async/await`.
- Pass `CancellationToken` through async flows where appropriate.
- Use dependency injection through the existing container configuration.
- Validate input models using the project’s existing validation approach.
- Use structured logging through `ILogger<T>`.
- Never log secrets, tokens, connection strings, personal data, or sensitive payloads.
- Keep controllers/endpoints thin.
- Keep mappings explicit and consistent with the existing style in the project.
- Do not silently change serialization behavior, casing, enum handling, or model binding defaults.

## Data & Persistence Rules
- Respect existing persistence patterns (`EF Core`, Dapper, repositories, query services, etc.).
- Do not introduce schema changes unless explicitly requested.
- Do not create EF Core migrations unless explicitly requested.
- Do not change existing database contracts or query semantics unless required by the task.
- Be careful with query performance, N+1 issues, unnecessary tracking, and over-fetching.
- For write operations, preserve transactional behavior already established in the project.

## API Contract Rules
- Do not change public request/response contracts unless explicitly requested.
- Do not rename routes, controller actions, endpoint paths, or response fields without explicit approval.
- Preserve backward compatibility where possible.
- If a breaking change is unavoidable, state it clearly in the report.

## Security & Reliability Rules
- Do not weaken authentication, authorization, validation, or data protection.
- Do not bypass permission checks for convenience.
- Do not expose internal exception details through API responses unless the project explicitly does that in development only.
- Prefer explicit error handling consistent with the existing project approach.
- Avoid introducing hidden side effects or implicit behavior changes.

## Testing & Verification
Before завершением задачи, Codex should run what is available and relevant in this repo:

1. Restore:
    - `dotnet restore`

2. Build:
    - `dotnet build`

3. Tests:
    - `dotnet test`

If the repo requires a specific solution or project file, use that instead of generic commands.

If a command fails, explicitly report:
- which command was run
- the failure point
- whether the issue appears related to the change or already existed

## Change Policy
- Do not perform broad refactoring unless explicitly requested.
- Do not rename files, namespaces, folders, or projects without need.
- Do not change `appsettings.*`, secrets, environment configuration, Docker, deployment, CI/CD, or infrastructure unless the task is specifically about them.
- Do not add new external dependencies unless there is a clear need and it is justified in the final report.
- Do not change formatting across unrelated files.

## Output Format
For each task, report in this order:
1. What was changed
2. Which files were modified
3. What was verified
4. What could not be verified
5. Risks / what should be checked manually

## Definition of Done
The task is done when:
- The requested backend behavior is implemented.
- Public contracts remain stable unless explicitly changed by request.
- The change respects existing architecture and layer boundaries.
- Build/tests were run when available, or the reason they were not run is clearly stated.
- No obvious unrelated regressions were introduced.
